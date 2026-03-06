using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageObservatory
{
    internal class PopupObsSensorInspect : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;


        Button btnClose, btnAlarm, btnHistory, btnSetting;

        RectTransform trendMeas, trendPred;

        TMP_Text txtTitle, txtSensorInfo, txtAlarmState, txtThreshold;

        private void Start()
        {
            UiManager.Instance.Register(UiEventType.SelectSensor, OnSelectSensor);

            //버튼 패스
            btnClose = transform.Find("Btn_Close").GetComponent<Button>();
            btnClose.onClick.AddListener(OnClickClose);

            btnAlarm = transform.Find("PanelDetail").Find("BtnAlarm").GetComponent<Button>();
            btnAlarm.onClick.AddListener(OnClickAlarm);
            btnHistory = transform.Find("PanelDetail").Find("BtnHistory").GetComponent<Button>();
            btnHistory.onClick.AddListener(OnClickHistory);
            btnSetting = transform.Find("PanelDetail").Find("BtnSetting").GetComponent<Button>();
            btnSetting.onClick.AddListener(OnClickSetting);

            //트렌드 패스
            trendMeas = transform.Find("SensorTrendPv").GetComponent<RectTransform>();
            trendPred = transform.Find("SensorTrendAi").GetComponent<RectTransform>();

            //텍스트 패스
            txtTitle = transform.Find("PanelDetail").Find("imgTitle").Find("txtTitle").GetComponent<TMP_Text>();
            txtSensorInfo = transform.Find("PanelDetail").Find("txtSensorInfo").GetComponent<TMP_Text>();
            txtAlarmState = transform.Find("PanelDetail").Find("txtAlarmState").GetComponent<TMP_Text>();
            txtThreshold = transform.Find("PanelDetail").Find("txtThreshold").GetComponent<TMP_Text>();

            gameObject.SetActive(false);
        }


        private void OnClickAlarm()
        {
            UiManager.Instance.Invoke(UiEventType.NavigateHistory);
            //UiManager.Instance.Invoke(UiEventType.SelectAlarm, alarmInfo.alarmIdx);
            //TODO
        }

        private void OnClickHistory()
        {
            UiManager.Instance.Invoke(UiEventType.NavigateHistory);
            //UiManager.Instance.Invoke(UiEventType.SelectTimeSeries, (0,0,DateTime.Now.AddDays(-1), DateTime.Now));
            //TODO
        }

        private void OnClickSetting()
        {
            UiManager.Instance.Invoke(UiEventType.NavigateSetting);
        }

        private void OnSelectSensor(object obj)
        {
            if (obj is not (int obsIdx, int sensorIdx)) throw new ArgumentException("Invalid argument for SelectSensor event. Expected (int obsIdx, int sensorIdx).");

            gameObject.SetActive(true);
            Debug.Log($"PopupObsSensorInspect received SelectSensor event with obsIdx: {obsIdx}, sensorIdx: {sensorIdx}");
            //@TODO

        }

        void OnClickClose() 
        {
            gameObject.SetActive(false);
        }
    }
}
