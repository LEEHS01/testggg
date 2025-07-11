using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Onthesys;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UMP;

public class TMVideo : MonoBehaviour
{
    public TMP_Text areaName1;
    public TMP_Text areaName2;

    public GameObject video_ui1;
    public GameObject video_ui2;
    public UniversalMediaPlayer video1;
    public UniversalMediaPlayer video2;

    public GameObject btnZoom1;
    public GameObject btnZoom2;

    public GameObject btnPlay1;
    public GameObject btnPlay2;

    public GameObject loading1;
    public GameObject loading2;

    private LogData data;

    void Start()
    {
        //DataManager.Instance.OnSelectLog.AddListener(this.UpdateVideo);
        this.video_ui1.gameObject.SetActive(false);
        this.video_ui2.gameObject.SetActive(false);
        this.btnZoom1.gameObject.SetActive(false);
        this.btnZoom2.gameObject.SetActive(false);
        this.btnPlay1.gameObject.SetActive(false);
        this.btnPlay2.gameObject.SetActive(false);
        this.areaName1.text = "Cctv1";
        this.areaName2.text = "Cctv2";
        this.loading1.SetActive(false);
        this.loading2.SetActive(false);
    }

    public void OnSelectLog(LogData data = null)
    {
        this.data = data;
        this.video_ui1.gameObject.SetActive(true);
        this.video_ui2.gameObject.SetActive(true);
        this.btnZoom1.gameObject.SetActive(true);
        this.btnZoom2.gameObject.SetActive(true);
        this.btnPlay1.gameObject.SetActive(true);
        this.btnPlay2.gameObject.SetActive(true);
        this.areaName1.text = "Cctv1";
        this.areaName2.text = "Cctv2";
        
        this.video_ui1.gameObject.SetActive(false);
        this.video_ui2.gameObject.SetActive(false);

        this.video1.AddBufferingEvent((progress)=>{
            if(progress == 100) this.loading1.SetActive(false);
            else {
                this.loading1.SetActive(true);
                this.video_ui1.gameObject.SetActive(true);
            }
        });
        this.video2.AddBufferingEvent((progress)=>{
            if(progress == 100) this.loading2.SetActive(false);
            else {
                this.loading2.SetActive(true);
                this.video_ui2.gameObject.SetActive(true);
            }
        });
        // this.video1.m_AutoOpen = true;
        // this.video1.m_AutoStart = true;
        // this.video1.PlatformOptionsWindows.path = DataManager.Instance.areas[data.areaIdx].src_video2;
        // this.video2.PlatformOptionsWindows.path = DataManager.Instance.areas[data.areaIdx].src_video2;
        // this.video1.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, this.video1.PlatformOptionsWindows.path, true);
        // this.video2.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, this.video2.PlatformOptionsWindows.path, true);
    }

    /// <summary>
    /// Cctv 비디오 선택 기능으로 PopUP_Btn을 통해 활성화됨
    /// </summary>
    public void OnSelectVideo() {
        //DataManager.Instance.RequestOnSelectCctv(this.data.obsId);
    }
}
