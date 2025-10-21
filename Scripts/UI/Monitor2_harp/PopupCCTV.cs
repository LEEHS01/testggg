using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Onthesys;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;
using System.Net;
using UnityEngine.Networking;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using UMP;
using Unity.VisualScripting;

public class PopupCCTV : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    public TMP_Text txtName;
    public UniversalMediaPlayer video;
    public GameObject loading;
    public ARVideoCanvasHelper gui;
    private ObsData data;

    public GameObject btnPlay = null;
    public GameObject btnPause = null;

    [Header("CCTV Settings")]
    private string cctvIP;  // 동적으로 설정

    // 어느 CCTV인지 구분 (A or B)
    public enum CCTVType { VideoA, VideoB }
    public CCTVType cctvType = CCTVType.VideoA;  // Inspector에서 설정

    private void OnEnable()
    {
        // 팝업 열릴 때 버튼 상태 초기화
        if (btnPlay) btnPlay?.SetActive(true);
        if (btnPause) btnPause?.SetActive(false);

        // ✅ Popup이 열릴 때마다 현재 관측소의 CCTV URL 로드
        LoadCurrentCCTV();

        Debug.Log($"[PopupCCTV] OnEnable 완료 - cctvType: {cctvType}, cctvIP: {cctvIP}");
    }

    void Start()
    {
        this.gui.gameObject.SetActive(false);
        this.loading.SetActive(false);

        // 버퍼링 시 로딩 표시
        this.video.AddBufferingEvent((progress) =>
        {
            bool isBuffering = progress < 100;
            this.loading.SetActive(isBuffering);
            this.gui.gameObject.SetActive(isBuffering);
        });
    }

    /// <summary>
    /// 현재 관측소의 CCTV URL 로드
    /// </summary>
    private void LoadCurrentCCTV()
    {
        int currentObsId = modelProvider.GetCurrentObsId();

        if (currentObsId <= 0)
        {
            Debug.LogWarning("[PopupCCTV] 선택된 관측소가 없습니다!");
            return;
        }

        ObsData obs = modelProvider.GetObs(currentObsId);

        if (obs == null)
        {
            Debug.LogError($"[PopupCCTV] 관측소 {currentObsId}를 찾을 수 없습니다!");
            return;
        }

        // ✅ CCTV 타입에 따라 URL 선택
        string rtspUrl = cctvType == CCTVType.VideoA ? obs.src_video1 : obs.src_video2;

        if (string.IsNullOrEmpty(rtspUrl))
        {
            Debug.LogWarning($"[PopupCCTV] CCTV URL이 비어있습니다! (Type: {cctvType})");
            return;
        }

        Debug.Log($"[PopupCCTV] CCTV Type: {cctvType}");
        Debug.Log($"[PopupCCTV] CCTV URL 설정: {rtspUrl}");

        // ✅ PTZ 제어 IP 추출 먼저! (video.Path 설정 전)
        ExtractCCTVIP(rtspUrl);

        // ✅ MediaPlayer에 URL 설정 (IP 추출 후)
        video.Path = rtspUrl;
        video.Prepare();

        // ✅ 관측소 이름 설정
        this.data = obs;
        this.txtName.text = $"{obs.areaName} - {obs.obsName} (CCTV {(cctvType == CCTVType.VideoA ? "A" : "B")})";
    }

    /// <summary>
    /// RTSP URL에서 IP와 포트 추출하여 PTZ 제어용 IP 설정
    /// </summary>
    private void ExtractCCTVIP(string rtspUrl)
    {
        if (string.IsNullOrEmpty(rtspUrl))
        {
            Debug.LogWarning("[PopupCCTV] RTSP URL이 비어있습니다!");
            return;
        }

        Debug.Log($"[PopupCCTV] IP 추출 시작: {rtspUrl}");

        // rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42:554/video1
        // → 115.91.85.42:554 추출
        var match = System.Text.RegularExpressions.Regex.Match(
            rtspUrl, @"@([\d\.]+):(\d+)");

        if (match.Success)
        {
            string ip = match.Groups[1].Value;      // 115.91.85.42
            string port = match.Groups[2].Value;    // 554 또는 50556

            // ⭐ RTSP 포트에 따라 PTZ 포트 매핑
            string ptzPort = "50080";  // 기본값

            if (port == "554")
            {
                ptzPort = "50080";  // Video A
            }
            else if (port == "50556")
            {
                ptzPort = "50081";  // Video B
            }

            cctvIP = $"{ip}:{ptzPort}";

            Debug.Log($"[PopupCCTV] ✅ PTZ 제어 IP 설정 완료: {cctvIP} (RTSP 포트: {port})");
        }
        else
        {
            Debug.LogError($"[PopupCCTV] ❌ IP 추출 실패: {rtspUrl}");
        }
    }

    public void OnVideoUp()
    {
        Debug.Log("up");
        SendPTZCommandAsync("up", 2, 1000);
    }

    public void OnVideoDown()
    {
        Debug.Log("down");
        SendPTZCommandAsync("down", 2, 1000);
    }

    public void OnVideoLeft()
    {
        Debug.Log("left");
        SendPTZCommandAsync("left", 2, 1000);
    }

    public void OnVideoRight()
    {
        Debug.Log("right");
        SendPTZCommandAsync("right", 2, 1000);
    }

    public void OnVideoIn()
    {
        Debug.Log("in");
        SendPTZCommandAsync("zoomin", 6, 1000);
    }

    public void OnVideoOut()
    {
        Debug.Log("out");
        SendPTZCommandAsync("zoomout", 6, 1000);
    }

    private async Task SendPTZCommandAsync(string direction, int speed, int timeout)
    {
        if (string.IsNullOrEmpty(cctvIP))
        {
            Debug.LogError("[PopupCCTV] cctvIP가 설정되지 않았습니다!");
            return;
        }

        string requestUrl = $"http://{cctvIP}/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";

        Debug.Log($"PTZ 제어 요청: {direction} - IP: {cctvIP}");
        Debug.Log($"요청 URL: {requestUrl}");

        HttpClient client = new HttpClient();

        var byteArray = Encoding.ASCII.GetBytes("admin:HNS_qhdks_!Q@W3");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        try
        {
            HttpResponseMessage response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log($"HttpRequest : {responseBody}");
        }
        catch (HttpRequestException httpEx)
        {
            Debug.LogError($"HttpRequestException: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending PTZ command: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}