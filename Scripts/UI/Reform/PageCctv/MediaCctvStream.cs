using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMP;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.Info.ObservatoryInfo;

namespace Assets.Scripts.UI.Reform.PageCctv
{
    internal class MediaCctvStream : MonoBehaviour
    {
        //참고용
        VideoBufferUI videoBufferUI;


        [SerializeField]
        public bool isFirstCctv;    //Editor에서 첫 번째 CCTV인지 여부 설정

        ObservatoryInfo obsInfo;

        Button btnPopup;
        UniversalMediaPlayer mediaPlayer;
        GameObject spinner;


        ModelProvider modelProvider => UiManager.Instance.modelProvider;


        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiated);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
            btnPopup = transform.Find("btnPopup").GetComponent<Button>();
            btnPopup.onClick.AddListener(OnClickPopup);
            mediaPlayer = transform.Find("MediaPlayer").GetComponent<UniversalMediaPlayer>();

        }

        private void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) throw new ArgumentException("Invalid argument for SelectObs event. Expected int obsIdx.");

            //유효한 관측소 인덱스인지 확인
            obsInfo = obsIdx <= 0? null : modelProvider.GetObsByIdx(obsIdx);


        }

        private void OnClickPopup()
        {
            if(obsInfo != null && obsInfo.cctvs[isFirstCctv? 0 : 1].isValid)
                UiManager.Instance.Invoke(UiEventType.PopupCctv, obsInfo.cctvs[isFirstCctv ? 0 : 1]);
        }

        private void OnInitiated(object obj)
        {
            if (mediaPlayer == null || spinner == null)
            {
                Debug.LogWarning("VideoBufferUI: player 또는 spinner가 연결되지 않았습니다.");
                return;
            }

            spinner.SetActive(false); // 초기 상태 비활성화

            mediaPlayer.AddBufferingEvent(progress =>
            {
                bool isBuffering = progress < 100;
                spinner.SetActive(isBuffering);
            });
        }
    }
}
