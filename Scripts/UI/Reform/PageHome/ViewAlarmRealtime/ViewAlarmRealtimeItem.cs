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
    public class ViewAlarmRealtimeItem : MonoBehaviour
    {
        Image alarmTypeLamp;
        TMP_Text txtObsName, txtItemName, txtPv, txtThreshold, txtTimePassed;
        Button btn;

        public AlarmInfo alarmInfo { private set; get; } = null;

        bool isStartCalled = false;

        private void Start()
        {
            if (isStartCalled) return;
            TryGetComponent<Button>(out btn);
            transform.Find("AlarmTypeLamp").TryGetComponent<Image>(out alarmTypeLamp);
            transform.Find("TxtContainer").Find("TxtObsName").TryGetComponent<TMP_Text>(out txtObsName);
            transform.Find("TxtContainer").Find("TxtItemName").TryGetComponent<TMP_Text>(out txtItemName);
            transform.Find("TxtContainer").Find("TxtPv").TryGetComponent<TMP_Text>(out txtPv);
            transform.Find("TxtContainer").Find("TxtThreshold").TryGetComponent<TMP_Text>(out txtThreshold);
            transform.Find("TxtContainer").Find("TxtTimePassed").TryGetComponent<TMP_Text>(out txtTimePassed);

            if (btn == null || alarmTypeLamp==null | txtObsName == null || txtItemName == null || txtPv == null || txtThreshold == null || txtTimePassed == null)
            {
                Debug.LogError("ViewAlarmRealtimeItem: One or more TMP_Text components not found!");
            }

            btn.onClick.AddListener(OnClick);

            isStartCalled = true;
        }


        public void SetValue(AlarmInfo alarmInfo)
        {
            //Debug.Log($"{alarmInfo.alarmIdx} {alarmInfo.obsNameText} {alarmInfo.sensorName} {alarmInfo.occured.valMeas} {alarmInfo.alarmType}");
            this.alarmInfo = alarmInfo;

            if (btn == null || alarmTypeLamp == null | txtObsName == null || txtItemName == null || txtPv == null || txtThreshold == null || txtTimePassed == null)
                Start();


            txtObsName.text = alarmInfo.obsNameText;
            txtItemName.text = alarmInfo.sensorName;
            txtPv.text = alarmInfo.occured.valMeas.ToString("F2");

            switch (alarmInfo.alarmType)
            {
                case AlarmState.TH_HIGH:
                    alarmTypeLamp.color = Color.red;
                    txtThreshold.text = ">" + alarmInfo.occured.thresholdHigh.ToString("F2");
                    break;
                case AlarmState.TH_LOW:
                    alarmTypeLamp.color = Color.red;
                    txtThreshold.text = "<" + alarmInfo.occured.thresholdLow.ToString("F2");
                    break;
            }

            float timePassedSeconds = (float)(DateTime.Now - alarmInfo.occured.timestamp).TotalSeconds;

            if (timePassedSeconds < 60)
            {
                txtTimePassed.text = $"1분 이내";
            }
            else if (timePassedSeconds < 3600 * 2)
            {
                txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 60)}분 전";
            }
            else if (timePassedSeconds < 86400 * 3)
            {
                txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 3600)}시간 전";
            }
            else if (timePassedSeconds < 2592000 * 3)
            {
                txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 86400)}일 전";
            }
            else
            {
                txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 2592000)}개월 전";
            }
        }

        private void Update()
        {
            if (Time.time % 10 != (Time.time + Time.deltaTime) % 10) // Update every 10 seconds
            {

                float timePassedSeconds = (float)(DateTime.Now - alarmInfo.occured.timestamp).TotalSeconds;

                if (timePassedSeconds < 60)
                {
                    txtTimePassed.text = $"1분 이내";
                }
                else if (timePassedSeconds < 3600 * 2)
                {
                    txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 60)}분 전";
                }
                else if (timePassedSeconds < 86400 * 3)
                {
                    txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 3600)}시간 전";
                }
                else if (timePassedSeconds < 2592000 * 3)
                {
                    txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 86400)}일 전";
                }
                else
                {
                    txtTimePassed.text = $"{Math.Floor(timePassedSeconds / 2592000)}개월 전";
                }
            }
        }


        private void OnClick()
        {
            if (alarmInfo == null)
            {
                Debug.LogError("ViewAlarmRealtimeItem: alarmInfo is null on OnClick!");
                return;
            }

            UiManager.Instance.Invoke(UiEventType.NavigateHistory);
            UiManager.Instance.Invoke(UiEventType.SelectAlarm, alarmInfo.alarmIdx);
        }


    }
}
