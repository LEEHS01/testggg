using Assets.Scripts.Manager;
using Newtonsoft.Json;
using NUnit.Framework;
using Onthesys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Manager
{
    public class UiManager : MonoBehaviour
    {
        public ModelProvider modelProvider => ModelManager.Instance;

        public static UiManager Instance = null;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
        }

        private Dictionary<UiEventType, Action<object>> eventHandlers = new();
        public void Register(UiEventType eventType, Action<object> handler)
        {
            Debug.Log($"UiManager - Register 이벤트 구독: {eventType}");
            if (!eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = handler;
            }
            else
            {
                eventHandlers[eventType] += handler;
            }
        }

        public void Unregister(UiEventType eventType, Action<object> handler)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] -= handler;
            }
        }

        public void Invoke(UiEventType eventType, object payload = null)
        {
            Debug.Log($"UiManager - Invoke 이벤트 발생: {eventType}, 페이로드: {payload}");
            /*if (eventType == UiEventType.ChangeTrendLine)
            {
                Debug.Log($"ChangeTrendLine 호출! 스택트레이스:\n{System.Environment.StackTrace}");
            }*/
            if (eventHandlers.ContainsKey(eventType))
            {
                List<Delegate> delegates = eventHandlers[eventType]?.GetInvocationList().ToList();

                delegates.ForEach(del =>
                {
                    try
                    {
                        del.DynamicInvoke(payload);
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError($"UiManager - Invoke {ex.GetType()} : {ex.Message}");

                        while (ex is TargetInvocationException tex)
                            ex = tex.InnerException;

                        //재귀 방지
                        if (eventType == UiEventType.PopupError)
                        {
                            Debug.LogError($"UiManager - Invoke 내부 오류 : eventType({eventType}) ({ex.GetType()}) : {ex.Message}");
                            return;
                        }
                        Invoke(UiEventType.PopupError, ex);
                    }
                });

                //eventHandlers[eventType]?.Invoke(payload);
            }
        }

    }



    public enum UiEventType
    {
        Initiate,       //주요 컨트롤 객체들의 초기화 완료

        NavigateHome,   //시작 화면으로 이동
        NavigateRegion,   //지역 화면으로 이동
        NavigateObs,    //관측소 화면으로 이동
        NavigateCctv,   //Cctv 화면으로 이동
        NavigateHistory, //알람 및 히스토리 화면으로 이동
        NavigateSetting,    //환경설정 화면으로 이동

        SelectObs,     //관측소 선택
        SelectAlarm,   //알람 선택

        PopupError,
        SelectSensor,
        PopupCctv,
        PopupDbSet,
        RequestResetThreshold,
        RequestResetInspect,



        SelectTimeSeries,   //UiManager.Instance.Invoke(UiEventType.SelectTimeSeries, (0,0,DateTime.Now.AddDays(-1), DateTime.Now)) //관측소, 센서, 시작시간, 끝시간
        ObsUpdate,      //pv 값 / 보드 상태값...
        ChangeAlarmList,    //알람 리스트 변경 (알람 발생, 해제 등)
        RequestHistoryTimeSeries,
        UpdateRealtimeTimeSeries,
        UpdateHistoryTimeSeries,
        UpdateWeather,
    }

}