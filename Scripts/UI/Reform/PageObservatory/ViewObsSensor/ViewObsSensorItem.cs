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

namespace Assets.Scripts.UI.Reform.PageObservatory
{
    public class ViewObsSensorItem : MonoBehaviour
    {
        TMP_Text txtName, txtPV, txtThLo, txtThHi, txtUnit;
        Image imgAlarmLamp;
        RectTransform trendSimple;
        Button btn;


        SensorInfo sensorInfo;
        TimeSeriesInfo timeSeries;
        AlarmInfo alarmActivated;

        public (int obsIdx, int sensorIdx) sensorAddress;

        bool isStarted = false;
        private void Start()
        {
            if (isStarted) return;
            {
                txtName = transform.Find("TxtName").GetComponent<TMP_Text>();
                txtPV = transform.Find("TxtPv").GetComponent<TMP_Text>();
                txtThLo = transform.Find("TxtThLo").GetComponent<TMP_Text>();
                txtThHi = transform.Find("TxtThHi").GetComponent<TMP_Text>();
                txtUnit = transform.Find("TxtUnit").GetComponent<TMP_Text>();
                imgAlarmLamp = transform.Find("ImgAlarmLamp").GetComponent<Image>();
                trendSimple = transform.Find("LineChart").GetComponent<RectTransform>();
                btn = GetComponent<Button>();

                btn.onClick.AddListener(OnClick);
            }
            isStarted = true;
        }
        public void SetValue((int obsIdx, int sensorIdx) sensorAddress , SensorInfo sensorInfo, TimeSeriesInfo timeSeries, AlarmInfo alarmActivated)
        {
            if (!isStarted) Start();

            this.sensorAddress = sensorAddress;
            this.sensorInfo = sensorInfo;
            this.timeSeries = timeSeries;
            this.alarmActivated = alarmActivated;

            txtName.text = $"{sensorInfo.name}";
            txtPV.text = $"{sensorInfo.pv.ToString("N" + (sensorInfo.pv < 1000 ? sensorInfo.pv < 100 ? "2" : "1" : "0"))}";
            txtThHi.text = $"{sensorInfo.thresholdHigh.ToString("N" + (sensorInfo.pv < 1000 ? sensorInfo.pv < 100 ? "2" : "1" : "0"))}";
            txtThLo.text = $"{sensorInfo.thresholdLow.ToString("N" + (sensorInfo.pv < 1000 ? sensorInfo.pv < 100 ? "2" : "1" : "0"))}";
            txtUnit.text = $"{sensorInfo.unit}";

            //Image?
            //@TODO

            //Trend?
            //@TODO

        }


        void OnClick() 
        {
            UiManager.Instance.Invoke(UiEventType.SelectSensor, sensorAddress);
        }
    }
}
