using Assets.Scripts.Info;
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
    public class ViewObsSensorSplitter : MonoBehaviour
    {
        TMP_Text txtTitle;

        Image imgAlarmLamp;


        public bool isStarted = false;
        private void Start()
        {
            if (isStarted) return;
            {
                txtTitle = transform.Find("TxtTitle").GetComponent<TMP_Text>();
                imgAlarmLamp = transform.Find("titleCircle").Find("imgAlarmLamp").GetComponent<Image>();
            }
            isStarted = true;
        }
        public void SetValue(string boardName, ObservatoryInfo.BoardInfo boardInfo)
        {
            if (!isStarted) Start();
            txtTitle.text = $"{boardName} 계측 현황";

            //TODO
            //imgAlarmLamp

        }
    }
}
