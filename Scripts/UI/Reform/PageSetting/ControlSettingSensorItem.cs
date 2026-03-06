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

namespace Assets.Scripts.UI.Reform.PageSetting
{
    internal class ControlSettingSensorItem : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        TMP_Text txtSensorName;
        Toggle tglIsInspect;
        TMP_InputField inputThLo, inputThHi;

        ObservatoryInfo obs;
        public SensorInfo sensor;

        bool isStarted = false;

        private void Start()
        {
            if (!isStarted) 
            {
                tglIsInspect = transform.Find("tglIsInspect").GetComponent<Toggle>();
                inputThLo = transform.Find("txtBoxLo").GetComponent<TMP_InputField>();
                inputThHi = transform.Find("txtBoxHi").GetComponent<TMP_InputField>();
                txtSensorName = transform.Find("txtSensorName").GetComponent<TMP_Text>();

                inputThLo.onValueChanged.AddListener(OnChangeValue);
                inputThHi.onValueChanged.AddListener(OnChangeValue);

                isStarted = true;
            }
        }

        private void OnChangeValue(string arg0)
        {
            //arg0 <<<< 이게 뭐임????
            // 왜 Action string 으로 받는거야???

            string txtInputThLo = inputThLo.text;
            string txtInputThHi = inputThHi.text;
            //TODO 



        }

        internal void SetValue(ObservatoryInfo obs, ObservatoryInfo.SensorInfo sensorInfo)
        {
            if (!isStarted) Start();
            this.obs = obs;
            this.sensor = sensorInfo;

            txtSensorName.text = sensorInfo.name;
            tglIsInspect.isOn = sensorInfo.isUsing;
            inputThLo.text = sensorInfo.thresholdLow.ToString();
            inputThHi.text = sensorInfo.thresholdHigh.ToString();
    
            this.obs = obs;
            this.sensor = sensorInfo;
        }
    }
}
