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
using static Assets.Scripts.Info.ObservatoryInfo;

namespace Assets.Scripts.UI.Reform.PageHistory
{
    internal class ViewAlarmItem : MonoBehaviour
    {

        Button btn;

        TMP_Text txtObsName, txtAlarmType, txtOccuredDt, txtSolvedDt, txtOccuredMeas, txtSolvedMeas, txtOccuredCond, txtSolvedCond;


        AlarmInfo alarmInfo;
        SensorInfo sensorInfo;


        bool isStarted = false;
        private void Start()
        {
            if (!isStarted)
            {
                txtObsName = transform.Find("layoutAlarm").Find("txtObsName").GetComponent<TMP_Text>();
                txtAlarmType = transform.Find("layoutAlarm").Find("txtAlarmType").GetComponent<TMP_Text>();
                
                txtOccuredDt = transform.Find("layoutOccured").Find("txtOccuredDt").GetComponent<TMP_Text>();
                txtOccuredMeas = transform.Find("layoutOccured").Find("txtOccuredMeas").GetComponent<TMP_Text>();
                txtOccuredCond = transform.Find("layoutOccured").Find("txtOccuredCond").GetComponent<TMP_Text>();

                txtSolvedDt = transform.Find("layoutSolved").Find("txtSolvedDt").GetComponent<TMP_Text>();
                txtSolvedMeas = transform.Find("layoutSolved").Find("txtSolvedMeas").GetComponent<TMP_Text>();
                txtSolvedCond = transform.Find("layoutSolved").Find("txtSolvedCond").GetComponent<TMP_Text>();

                btn = GetComponent<Button>();
                btn.onClick.AddListener(OnClickItem);

                isStarted = true;
            }
        }

        internal void SetValue(AlarmInfo alarmInfo, SensorInfo sensorInfo)
        {
            if (!isStarted) Start();

            this.alarmInfo = alarmInfo ?? throw new ArgumentNullException(nameof(alarmInfo));
            this.sensorInfo = sensorInfo ?? throw new ArgumentNullException(nameof(sensorInfo));

            // txtObsName
            txtObsName.text = alarmInfo.obsNameText;

            // txtAlarmType
            var alarmType = alarmInfo.alarmType;
            switch (alarmType) 
            {
                case ModelsReform.AlarmState.TH_HIGH:
                    txtAlarmType.text = "정상 범위 초과";
                    break;
                case ModelsReform.AlarmState.TH_LOW:
                    txtAlarmType.text = "정상 범위 미달";
                    break;
            }
            

            // <Occured>
            {
                // txtOccuredDt
                txtOccuredDt.text = alarmInfo.occured.timestamp.ToString("yy.MM.dd HH:mm");

                //txtOccuredMeas
                txtOccuredMeas.text = $"{alarmInfo.occured.valMeas.ToString("N" + (alarmInfo.occured.valMeas < 1000 ? alarmInfo.occured.valMeas < 100 ? "2" : "1" : "0"))} {sensorInfo.unit}";

                // txtOccuredCond
                string condSymbol =
                    alarmType == ModelsReform.AlarmState.TH_HIGH ? ">" :
                    alarmType == ModelsReform.AlarmState.TH_LOW ? "<" :
                    "?";
                string condThreshold =
                    alarmType == ModelsReform.AlarmState.TH_HIGH ? alarmInfo.occured.thresholdHigh.ToString("N" + (alarmInfo.occured.thresholdHigh < 1000 ? alarmInfo.occured.thresholdHigh < 100 ? "2" : "1" : "0")) :
                    alarmType == ModelsReform.AlarmState.TH_LOW ? alarmInfo.occured.thresholdLow.ToString("N" + (alarmInfo.occured.thresholdLow < 1000 ? alarmInfo.occured.thresholdLow < 100 ? "2" : "1" : "0")) : "?";

                txtOccuredCond.text = $"{condSymbol} {condThreshold}";
            }

            // <solved> 종료됨
            if (alarmInfo.solved.HasValue)// 해결 정보가 있는 경우에만 표시
            {
                //txtSolvedDt
                txtSolvedDt.text = alarmInfo.solved.Value.timestamp.ToString("yy.MM.dd HH:mm");

                //txtSolvedMeas
                txtSolvedMeas.text =$"{sensorInfo.pv.ToString("N" + (sensorInfo.pv < 1000 ? sensorInfo.pv < 100 ? "2" : "1" : "0"))} {sensorInfo.unit}";


                // txtSolvedCond
                string condSymbol =
                    alarmType == ModelsReform.AlarmState.TH_HIGH ? ">" :
                    alarmType == ModelsReform.AlarmState.TH_LOW ? "<" :
                    "?";
                string condThreshold =
                    alarmType == ModelsReform.AlarmState.TH_HIGH ? alarmInfo.solved.Value.thresholdHigh.ToString("N" + (alarmInfo.solved.Value.thresholdHigh < 1000 ? alarmInfo.solved.Value.thresholdHigh < 100 ? "2" : "1" : "0")) :
                    alarmType == ModelsReform.AlarmState.TH_LOW ? alarmInfo.solved.Value.thresholdLow.ToString("N" + (alarmInfo.solved.Value.thresholdLow < 1000 ? alarmInfo.solved.Value.thresholdLow < 100 ? "2" : "1" : "0")) :
                    "?";

                txtSolvedCond.text = $"{condSymbol} {condThreshold}";
            }

            // <solved> 현재 진행형
            else // 없으면 "진행중" 등으로 표시+현재값들
            {
                txtSolvedDt.text = "진행중";       // 해결 날짜는 solvedTime이 null인지 여부로 판단하여 "진행중" 또는 실제 날짜 표시

                //txtSolvedMeas
                txtSolvedMeas.text = $"{sensorInfo.pv.ToString("N" + (sensorInfo.pv < 1000 ? sensorInfo.pv < 100 ? "2" : "1" : "0"))} {sensorInfo.unit}";


                // txtSolvedCond
                string condSymbol =
                    alarmType == ModelsReform.AlarmState.TH_HIGH ? ">" :
                    alarmType == ModelsReform.AlarmState.TH_LOW ? "<" :
                    "?";
                string condThreshold =
                    alarmType == ModelsReform.AlarmState.TH_HIGH ? sensorInfo.thresholdHigh.ToString("N" + (sensorInfo.thresholdHigh < 1000 ? sensorInfo.thresholdHigh < 100 ? "2" : "1" : "0")) :
                    alarmType == ModelsReform.AlarmState.TH_LOW ? sensorInfo.thresholdLow.ToString("N" + (sensorInfo.thresholdLow < 1000 ? sensorInfo.thresholdLow < 100 ? "2" : "1" : "0")) :
                    "?";

                txtSolvedCond.text = $"{condSymbol} {condThreshold}";
            }

            transform.GetComponentsInChildren<TMP_Text>().ToList().ForEach(txt => txt.color = alarmInfo.solved.HasValue? Color.gray : Color.white);

        }

        private void OnClickItem()
        {
            UiManager.Instance.Invoke(UiEventType.SelectAlarm, alarmInfo.alarmIdx);
        }
    }
}
