using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHistory
{
    internal class PopupAlarmInspect : MonoBehaviour
    {
        Button btnClose;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiated);
            UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectAlarm);
            btnClose = transform.Find("btnClose").GetComponent<Button>();
            btnClose.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            gameObject.SetActive(false);
        }

        private void OnSelectAlarm(object obj)
        {
            if(obj is not int alarmIdx) throw new ArgumentException("Invalid argument for SelectAlarm event. Expected int alarmIdx.");

            gameObject.SetActive(true);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            AlarmInfo alarm =  modelProvider.GetAlarmByIdx(alarmIdx);



            //TODO 알람 정보 표시 
        }

        private void OnInitiated(object obj)
        {
            gameObject.SetActive(false);
        }
    }
}
