using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.ModelsReform;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewAlarmRealtime : MonoBehaviour
    {
        private bool isInitiated = false;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        //GameObject prefab => Resources.Load<GameObject>("Reform/PageHome/ViewAlarmRealtimeItem");

        //#CurrentSituation
        private List<Image> imgRingCharts = new();
        TMP_Text txtAllAlarm, txtCautionAlarm, txtWarningAlarm, txtMalfunctionAlarm;


        //#ComparePrevMonth
        UILineRenderer2 trendPrevious;
        UILineRenderer2 trendCurrent;
        TMP_Text txtMonthPrev, txtMonthCur;
        TMP_Text txtDayFirst, txtDayLast;



        private RectTransform rectTransform;
        private Vector2 startAnchoredPos;
        private bool hasInitPosition = false;

        private Tween moveTween;

        private void Start()
        {
            var charts = transform.Find("PnlCurrentSituation").Find("Doughnut Chart").GetComponentsInChildren<Image>();
            imgRingCharts.AddRange(charts);

            txtAllAlarm = transform.Find("PnlCurrentSituation").Find("Middle Circle").GetComponentInChildren<TMP_Text>();
            txtCautionAlarm = transform.Find("PnlCurrentSituation").Find("imgLampCaution").GetComponentInChildren<TMP_Text>();
            txtWarningAlarm = transform.Find("PnlCurrentSituation").Find("imgLampWarning").GetComponentInChildren<TMP_Text>();
            txtMalfunctionAlarm = transform.Find("PnlCurrentSituation").Find("imgLampMalfunction").GetComponentInChildren<TMP_Text>();


            trendPrevious = transform.Find("PnlComparePrevMonth").Find("conTrend").Find("TrendChartPrevious").GetComponentInChildren<UILineRenderer2>();
            trendCurrent = transform.Find("PnlComparePrevMonth").Find("conTrend").Find("TrendChartCurrent").GetComponentInChildren<UILineRenderer2>();

            txtMonthPrev = transform.Find("PnlComparePrevMonth").Find("imgLampPrevious").GetComponentInChildren<TMP_Text>();
            txtMonthCur = transform.Find("PnlComparePrevMonth").Find("imgLampCurrent").GetComponentInChildren<TMP_Text>();
            txtDayFirst = transform.Find("PnlComparePrevMonth").Find("conTrend").Find("txtDayFirst").GetComponentInChildren<TMP_Text>();
            txtDayLast = transform.Find("PnlComparePrevMonth").Find("conTrend").Find("txtDayLast").GetComponentInChildren<TMP_Text>();



            rectTransform = GetComponent<RectTransform>();
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);

            UiManager.Instance.Register(UiEventType.NavigateRegion, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateCctv, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateHistory, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateSetting, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);

        }

        private void OnInitiate(object obj)
        {
            if (rectTransform != null && !hasInitPosition)
            {
                startAnchoredPos = rectTransform.anchoredPosition;
                hasInitPosition = true;
            }

            isInitiated = true;
        }

        public void Update()
        {
            if (Time.time % 10 != (Time.time + Time.deltaTime) % 10 && isInitiated) // Update every 10 seconds
            {
                OnChangeAlarmList(null);
            }
        }
        private void OnDestroy()
        {
            moveTween?.Kill();
        }

        // 애니메이션
        private void OnNavigateHome(object obj)
        {
            if (rectTransform == null || !hasInitPosition)
                return;

            moveTween?.Kill();
            moveTween = rectTransform
                .DOAnchorPos(startAnchoredPos, 0.5f)
                .SetEase(Ease.OutCubic);
        }

        // 애니메이션
        private void OnNavigateOut(object obj)
        {
            if (rectTransform == null)
                return;

            moveTween?.Kill();

            Vector2 targetPos = rectTransform.anchoredPosition + new Vector2(400f, 0f);

            moveTween = rectTransform
                .DOAnchorPos(targetPos, 0.5f)
                .SetEase(Ease.OutCubic);
        }


        //  이벤트 현황
        void OnChangeAlarmList(object obj)
        {
            List<AlarmInfo> alarmsNew = modelProvider.GetAlarmsActivated();
            List<ObservatoryInfo> obss = modelProvider.GetObss();

            Dictionary<string, int> alarmCnts = new() { { "caution", 0 }, { "warning", 0 }, { "malfunction", 0 } };
            alarmsNew.ForEach(alarm =>
            {
                //기능장애 관련 알람 판단...
                if ( //설비이상 관측소 확인
                    new int[] {
                        (int)AlarmState.COM_ERROR,
                        (int)AlarmState.ETC_ERROR,
                        (int)AlarmState.LIVE_ERROR,
                    }
                    .Contains<int>((int)alarm.alarmType)
                    )
                    alarmCnts["malfunction"]++;

                else if ( //경보 관측소 확인
                    new int[] {
                        (int)AlarmState.TH_LOW_2,
                        (int)AlarmState.TH_HIGH_2,
                    }
                    .Contains<int>((int)alarm.alarmType)
                    )
                    alarmCnts["warning"]++;

                else if ( //경계 관측소 확인
                    new int[] {
                        (int)AlarmState.TH_LOW,
                        (int)AlarmState.TH_HIGH,
                    }
                    .Contains<int>((int)alarm.alarmType)
                    )
                    alarmCnts["caution"]++;
            });

            UpdateCircleGraph(alarmCnts["caution"], alarmCnts["warning"], alarmCnts["malfunction"]);
            UpdateLegendText(alarmCnts["caution"], alarmCnts["warning"], alarmCnts["malfunction"]);

            // 트렌드 그래프 업데이트
            var (prev, cur) = modelProvider.GetEventsComparisonInfo();
            UpdateTrendGraph(prev, cur);

        }

        void UpdateCircleGraph(int cau, int warn, int malf)
        {

            // RingChart 업데이트
            const float fillRatioMin = 0.01f;
            var duration = 1f;
            var rotation = fillRatioMin;

            float sum = cau + warn + malf;
            List<int> alarmCounts = new List<int>() { cau, warn, malf }; // 예시 데이터

            for (int i = 0; i < imgRingCharts.Count; i++)
            {
                float p = (float)alarmCounts[i] / sum;

                var setPercent = p < fillRatioMin ? fillRatioMin : p;
                imgRingCharts[i].DOFillAmount(setPercent, duration);
                imgRingCharts[i].transform.DOLocalRotate(new Vector3(0, 0, rotation), duration);

                rotation -= (360 * setPercent);
            }
        }

        void UpdateLegendText(int cau, int warn, int malf)
        {
            txtAllAlarm.text = $"{cau + warn + malf}건";
            txtCautionAlarm.text = $"경계 {cau}건";
            txtWarningAlarm.text = $"경보 {warn}건";
            txtMalfunctionAlarm.text = $"상태이상 {malf}건";
        }

        // 이벤트 내역 전월 비교 트렌드 업데이트
        void UpdateTrendGraph((int year, int month, List<int> cnts) prev, (int year, int month, List<int> cnts) cur)
        {
            if (prev.cnts.Count == 0 || cur.cnts.Count == 0) return;

            //trendPrevious.Points = prev.Select((value, index) => new Vector2(index, value)).ToArray();
            //trendCurrent.Points = cur.Select((value, index) => new Vector2(index, value)).ToArray();

            trendPrevious.SetDotAmount(prev.cnts.Count);
            trendCurrent.SetDotAmount(cur.cnts.Count);

            int maxCount = Math.Max(prev.cnts.Max(), cur.cnts.Max());
            trendPrevious.UpdateControlPoints(prev.cnts.Select(cnt => (float)cnt / maxCount).ToList());
            trendCurrent.UpdateControlPoints(cur.cnts.Select(cnt => (float)cnt / maxCount).ToList());
            

            txtMonthPrev.text = $"{prev.year}/{prev.month}";
            txtMonthCur.text = $"{cur.year}/{cur.month}";
            txtDayFirst.text = $"1일";
            txtDayLast.text = $"말일";


        }
    }
}
