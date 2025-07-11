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
    public TMP_Text txtName;
    public UniversalMediaPlayer video;
    public GameObject loading;
    public ARVideoCanvasHelper gui;
    private ObsData data;

    public GameObject btnPlay = null;
    public GameObject btnPause = null;
    private bool isEventRegistered = false;

    [Header("CCTV Settings")]
    public string cctvBaseUrl;
    public string cctvUsername = "admin";
    public string cctvPassword = "HNS_qhdks_!Q@W3";

    private void OnEnable()
    {
        // 팝업 열릴 때 버튼 상태 초기화
        if(btnPlay) btnPlay?.SetActive(true);
        if(btnPause) btnPause?.SetActive(false);
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

    public void OnSelectCCTV(ObsData area)
    {
        this.data = area;
        this.txtName.text = area.areaName + " - " + area.obsName;
        this.gui.gameObject.SetActive(true);
    }


    public void OnVideoUp() {
        Debug.Log("up");
        SendPTZCommandAsync("up", 2, 1000);
    }

    public void OnVideoDown() {
        Debug.Log("down");
        SendPTZCommandAsync("down", 2, 1000);
    }

    public void OnVideoLeft() {
        Debug.Log("left");
        SendPTZCommandAsync("left", 2, 1000);
    }

    public void OnVideoRight() {
        Debug.Log("right");
        SendPTZCommandAsync("right", 2, 1000);
    }

    public void OnVideoIn() {
        Debug.Log("in");
        SendPTZCommandAsync("zoomin", 6, 1000);
    }

    public void OnVideoOut() {
        Debug.Log("out");
        SendPTZCommandAsync("zoomout", 6, 1000);
    }

    public void OnVideoEvent() {
        //Debug.Log("MediaPlayer " + mp.name + " generated event: " + eventType.ToString());
        
    }

    private async Task SendPTZCommandAsync(string direction, int speed, int timeout)
    {
        // 담당자가 제공한 형식: http://username:password@host:port/path
        string baseUrl = cctvBaseUrl;
        if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
        {
            baseUrl = "http://" + baseUrl;
        }

        // URL에서 호스트 부분만 추출
        string hostPart = baseUrl.Replace("http://", "").Replace("https://", "");

        // 담당자 제공 형식으로 URL 구성
        string requestUrl = $"http://{cctvUsername}:{cctvPassword}@{hostPart}/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";

        Debug.Log($"Sending PTZ request (format check): http://***:***@{hostPart}/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}");

        try
        {
            // UnityWebRequest 사용으로 변경 (Unity에서 더 안정적)
            UnityWebRequest www = UnityWebRequest.Get(requestUrl);
            www.timeout = 10; // 10초 타임아웃

            var operation = www.SendWebRequest();

            // 비동기 대기
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"PTZ Command Sent to {baseUrl}: {direction} - Response: {www.downloadHandler.text}");

                // PTZ 명령 후 비디오 리프레시 시도
                await RefreshVideoStreamAsync();
            }
            else
            {
                Debug.LogError($"PTZ Command Failed: {www.error}");
            }

            www.Dispose();
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending PTZ command to {baseUrl}: {ex.Message}");

            // HttpClient를 백업으로 시도
            await TryHttpClientRequest(direction, speed, timeout, hostPart);
        }
    }

    private async Task TryHttpClientRequest(string direction, int speed, int timeout, string hostPart)
    {
        try
        {
            HttpClient client = new HttpClient();

            // Basic Authentication 헤더 설정
            var byteArray = Encoding.ASCII.GetBytes($"{cctvUsername}:{cctvPassword}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            string backupUrl = $"http://{hostPart}/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";

            HttpResponseMessage response = await client.GetAsync(backupUrl);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log($"Backup HttpClient PTZ Command Success: {direction} - Response: {responseBody}");

            await RefreshVideoStreamAsync();

            client.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Backup HttpClient also failed: {ex.Message}");
        }
    }

    // 비디오 스트림 리프레시를 위한 메서드
    private async Task RefreshVideoStreamAsync()
    {
        try
        {
            // 잠시 대기 후 비디오 플레이어 리프레시
            await Task.Delay(100);

            if (video != null && video.isActiveAndEnabled)
            {
                // 현재 재생 위치 저장
                bool wasPlaying = video.IsPlaying;

                // 비디오 리로드
                video.Stop();
                await Task.Delay(50);

                if (wasPlaying)
                {
                    video.Play();
                }

                Debug.Log("Video stream refreshed after PTZ command");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error refreshing video stream: {ex.Message}");
        }
    }

    /*private async Task SendPTZCommandAsync(string direction, int speed, int timeout)
    {
        //string requestUrl = $"http://115.91.85.42:50081/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
        string requestUrl = $"http://192.168.1.109:50081/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
        //url2 = $"http://192.168.1.108:50080/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
        HttpClient client = new HttpClient();
        
            // 사용자 이름과 비밀번호 설정
            var byteArray = Encoding.ASCII.GetBytes("admin:HNS_qhdks_!Q@W3");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                // GET 요청
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                // 예외 발생
                response.EnsureSuccessStatusCode();

                // 응답 본문 읽기
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"HttpRequest : {responseBody}");

                // 요청 성공 메시지
                //MessageBox.Show($"Command {direction} sent successfully.");
            }
            catch (HttpRequestException httpEx)
            {
                Debug.Log($"HttpRequestException: {httpEx.Message}");
                Console.WriteLine($"HttpRequestException: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending PTZ command: {ex.Message}");
                Console.WriteLine($"Exception: {ex.Message}");
            }
        
        *//*
        string requestUrl = $"http://115.91.85.42:50081/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
        UnityWebRequest www = UnityWebRequest.Get(requestUrl);
        www.SetRequestHeader("Authorization", "admin:HNS_qhdks_!Q@W3");
        www.SendWebRequest();

        var byteArray = Encoding.ASCII.GetBytes("admin:HNS_qhdks_!Q@W3");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        *//*
    }*/


}
