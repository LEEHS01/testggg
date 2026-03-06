using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewRegionObsItem : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        Image alarmTypeLamp;
        Dictionary<BoardSpecInfo.BoardType, Image> boardLampMap = new();

        TMP_Text txtObsName, txtNormalCount, txtAlarmActivatedCount;
        Button btn;

        public ObservatoryInfo obsInfo { private set; get; } = null;

        bool isStartCalled = false;

        private void Start()
        {
            if (isStartCalled) return;

            TryGetComponent<Button>(out btn);
            transform.Find("AlarmTypeLamp").TryGetComponent<Image>(out alarmTypeLamp);
            transform.Find("TxtContainer").Find("TxtObsName").TryGetComponent<TMP_Text>(out txtObsName);
            transform.Find("TxtContainer").Find("TxtCountNormal").TryGetComponent<TMP_Text>(out txtNormalCount);
            transform.Find("TxtContainer").Find("TxtCountAlarmActivated").TryGetComponent<TMP_Text>(out txtAlarmActivatedCount);
            boardLampMap[BoardSpecInfo.BoardType.TOXIN] = transform.Find("BoardStateLamps").Find("LampToxin").GetComponent<Image>();
            boardLampMap[BoardSpecInfo.BoardType.HNS] = transform.Find("BoardStateLamps").Find("LampHns").GetComponent<Image>();
            boardLampMap[BoardSpecInfo.BoardType.WQ] = transform.Find("BoardStateLamps").Find("LampWq").GetComponent<Image>();

            if (btn == null || alarmTypeLamp == null | txtObsName == null || txtNormalCount == null || txtAlarmActivatedCount == null)
            {
                Debug.LogError("ViewAlarmRealtimeItem: One or more TMP_Text components not found!");
            }

            btn.onClick.AddListener(OnClick);

            isStartCalled = true;
        }


        public void SetValue(ObservatoryInfo obsInfo, List<AlarmInfo> alarms)
        {
            //Debug.Log($"{alarmInfo.alarmIdx} {alarmInfo.obsNameText} {alarmInfo.sensorName} {alarmInfo.occured.valMeas} {alarmInfo.alarmType}");
            this.obsInfo = obsInfo;

            if (btn == null || alarmTypeLamp == null | txtObsName == null || txtNormalCount == null || txtAlarmActivatedCount == null)
                Start();

            //Debug.Log($"obsInfo.sensors : " + obsInfo.sensors.Count);
            List<(int idx, ObservatoryInfo.SensorInfo info)> sensors = obsInfo.sensors.Where(sensor => sensor.info.isUsing && !sensor.info.isMissing).ToList();//.Select(idxSenPair => idxSenPair.info).ToList();

            //Debug.Log($"sensors : " + sensors.Count);
            int normalSensorCount = sensors.Where(sensorInfo => alarms.Select(alarm => alarm.sensorIdx).Contains(sensorInfo.idx) == false).Count();
            //Debug.Log($"normalSensorCount : " + normalSensorCount);
            int alarmCount = alarms.Count();
            Dictionary<BoardSpecInfo.BoardType, AlarmState> boardStates = new() { };
            //TODO

            txtObsName.text = obsInfo.nameText;
            txtNormalCount.text = $"{normalSensorCount:D2}개";
            txtAlarmActivatedCount.text = $"{alarmCount:D2}개";

            //if(alarms.Count == 0)
            Color obsStateColor = Color.green;

            if (alarms.Find(alarm =>
                new[] { AlarmState.TH_LOW, AlarmState.TH_LOW_2, AlarmState.TH_HIGH, AlarmState.TH_HIGH_2, }
                    .Contains(alarm.alarmType)) != null)
                obsStateColor = Color.red;

            if(boardStates.ToList().FindIndex(kvp =>
                new[] { AlarmState.COM_ERROR, AlarmState.LIVE_ERROR, AlarmState.ETC_ERROR}
                    .Contains(kvp.Value)) >= 0)
                obsStateColor = Color.Lerp(Color.red, Color.blue,0.5f);


            alarmTypeLamp.color = obsStateColor;


        }


        private void OnClick()
        {
            if (obsInfo == null)
            {
                Debug.LogError("ViewAlarmRealtimeItem: alarmInfo is null on OnClick!");
                return;
            }


            //선택시 지역 화면으로
            //
            if (modelProvider.GetCurrentObsIdx() == obsInfo.obsIdx) //관측소가 있다면 자동선택
                UiManager.Instance.Invoke(UiEventType.NavigateObs);
            else
                UiManager.Instance.Invoke(UiEventType.SelectObs, obsInfo.obsIdx);
        }

    }
}
