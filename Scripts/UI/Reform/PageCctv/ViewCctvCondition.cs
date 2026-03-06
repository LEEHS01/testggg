using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageCctv
{
    internal class ViewCctvCondition : MonoBehaviour
    {
        [SerializeField]
        public bool isFirstCctv;    //Editor에서 첫 번째 CCTV인지 여부 설정

        Image imgAlarmType;
        TMP_Text txtTitle, txtComEndPoint, txtComState, txtFunctionList;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiated);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);

            imgAlarmType = transform.Find("titleCircle").Find("AlarmTypeLamp").GetComponent<Image>();
            imgAlarmType.color = new Color(0, 0, 0, 0); //초기에는 알람 유형 램프 투명 처리
            txtComEndPoint = transform.Find("TxtComEndpoint").GetComponent<TMP_Text>();
            txtComState = transform.Find("TxtComState").GetComponent<TMP_Text>();
            txtFunctionList = transform.Find("TxtFunctionList").GetComponent<TMP_Text>();
            txtTitle = transform.Find("TxtTitle").GetComponent<TMP_Text>();
        }

        private void OnSelectObs(object obj)
        {            
            bool isValidObs = true;
            int obsIdx = -1;
            if (obj is not int) isValidObs = false;
            else obsIdx = (int)obj;

            var obs = modelProvider.GetObsByIdx(obsIdx);
            if (isValidObs && obs is null) isValidObs = false;

            var cctvInfo = obs == null ? null : obs.cctvs[isFirstCctv ? 0 : 1];
            if (isValidObs &&!cctvInfo.isValid) isValidObs = false;

            //유효하지 않은 관측소 선택 시 기본값 또는 오류 메시지 표시
            if (!isValidObs)
            {
                txtComState.text = $"연결 상태 : --";
                txtComEndPoint.text = $"연결 정보 : --";
                txtFunctionList.text = $"기능 목록 : --";
                imgAlarmType.color = new Color(0, 0, 0, 0); //알람 유형 램프 투명 처리
                txtTitle.text = $"-- CCTV 정보";
                return;
            }

            //연결 상태
            //TODO 간단하게 rtsp 연결 여부를 확인한 뒤, True False 형식으로 표시할 예정
            txtComState.text = $"연결 상태 : {0}";

            //연결 정보
            txtComEndPoint.text = $"연결 정보 : {cctvInfo.endpoint}";
    
            //기능 목록
            txtFunctionList.text = $"기능 목록 : {string.Join(", ", cctvInfo.functionMap.Where(kvp => kvp.Value == true).Select(kvp => kvp.Key))}";

            //알람 유형에 따른 색상 변경
            //TODO
            //통신 시도 중 : 회색
            //통신 실패 : 빨간색
            //통신 성공 : 초록색

            //제목 설정
            txtTitle.text = $"'{cctvInfo.locationText}' CCTV 정보";
        }

        private void OnInitiated(object obj)
        {
            //throw new NotImplementedException();
        }
    }
}
