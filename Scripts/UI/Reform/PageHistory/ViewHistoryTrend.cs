using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHistory
{
    internal class ViewHistoryTrend : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;


        UILineRenderer2 trendMeas, trendPred;
        Button btnQuery;

        (TMP_InputField from, TMP_InputField to) txbDatetime;
        DateTime dtFrom, dtTo;
        TMP_Dropdown ddlRegion, ddlObservatory, ddlSensor;



        List<GroupInfo> groups;
        List<ObservatoryInfo> obss;
        List<(int idx, ObservatoryInfo.SensorInfo info)> sensors;

        int groupIdx = -1, obsIdx = -1, sensorIdx = -1;


        #region 툴팁 
        GameObject tooltip;
        TMP_Text txtTime;
        TMP_Text txtValue;
        RectTransform chartArea; // 차트 영역
        List<float> originalValues = new(); // 툴팁용 원본 값들
        bool wasMouseInChartArea = false;   // 마우스 진입/퇴장 추적

        #endregion

        int trendDotCount = 120;


        //Func
        public DateTime DatetimeFrom
        {
            get => dtFrom;
            set
            {
                dtFrom = value;
                txbDatetime.from.SetTextWithoutNotify(dtFrom.ToString("yyyy-MM-dd"));
                dtFrom = dtFrom.AddHours(-dtFrom.Hour);
                dtFrom = dtFrom.AddMinutes(-dtFrom.Minute);
                dtFrom = dtFrom.AddSeconds(-dtFrom.Second);

            }
        }
        public DateTime DateTimeTo
        {
            get => dtTo;
            set
            {
                dtTo = value;
                txbDatetime.to.SetTextWithoutNotify(dtTo.ToString("yyyy-MM-dd"));

                if (dtTo.Date != DateTime.Now.Date)
                {
                    dtTo = dtTo.AddHours(23 - dtTo.Hour);
                    dtTo = dtTo.AddMinutes(59 - dtTo.Minute);
                    dtTo = dtTo.AddSeconds(59 - dtTo.Second);
                }
                else
                {
                    dtTo = DateTime.Now;
                }
            }
        }


        private void Start()
        {
            trendMeas = transform.Find("PanelTrendCharts").Find("TrendChartPv").Find("Chart_Dots").GetComponent<UILineRenderer2>();
            trendPred = transform.Find("PanelTrendCharts").Find("TrendChartAi").Find("Chart_Dots").GetComponent<UILineRenderer2>();
            btnQuery = transform.Find("PanelControl").Find("panelTimeRange").Find("btnQuery").GetComponent<Button>();
            btnQuery.onClick.AddListener(OnClickQuery);

            txbDatetime.from = transform.Find("PanelControl").Find("panelTimeRange").Find("tbxDtFrom").GetComponent<TMP_InputField>();
            txbDatetime.to = transform.Find("PanelControl").Find("panelTimeRange").Find("tbxDtTo").GetComponent<TMP_InputField>();

            ddlRegion = transform.Find("PanelControl").Find("ddlRegion").GetComponent<TMP_Dropdown>();
            ddlObservatory = transform.Find("PanelControl").Find("ddlObservatory").GetComponent<TMP_Dropdown>();
            ddlSensor = transform.Find("PanelControl").Find("ddlSensor").GetComponent<TMP_Dropdown>();

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.UpdateHistoryTimeSeries,OnUpdateHistoryTimeSeries);

            ddlRegion.onValueChanged.AddListener(OnChangeRegion);
            ddlObservatory.onValueChanged.AddListener(OnChangeObservatory);
            ddlSensor.onValueChanged.AddListener(OnChangeSensor);


            txbDatetime.from.onValueChanged.AddListener(value => OnChangeDateTime(true, value));
            txbDatetime.from.onEndEdit.AddListener(value => OnEndEditDateTime(true, value));
            txbDatetime.from.text = DateTime.Now.ToString("yyyy-MM-dd");

            txbDatetime.to.onValueChanged.AddListener(value => OnChangeDateTime(false, value));
            txbDatetime.to.onEndEdit.AddListener(value => OnEndEditDateTime(false, value));
            txbDatetime.to.text = DateTime.Now.ToString("yyyy-MM-dd");

        }


        //이벤트 핸들러
        private void OnUpdateHistoryTimeSeries(object obj)
        {
            List<(DateTime timestamp, (float val, bool isMissing) value)> timeSeries = modelProvider.GetHistoryTimeSeriesInfo();


            List<float> trendDots = new();

            float maxVal = timeSeries.Max(row => row.value.isMissing ? 0f : row.value.val);
            float minVal = timeSeries.Min(row => row.value.isMissing ? 0f : row.value.val);
            minVal = Math.Min(0f, minVal);

            int i = 0, maxC = trendMeas.dotsCount;
            
            if (timeSeries.Count < maxC)
                throw new Exception($"시계열 데이터 개수 부족 - {timeSeries.Count}개 (최소 {maxC}개 필요)");


            while (i < maxC)
            {
                float ratio = (float)(i) / maxC;
                int idx = Mathf.RoundToInt(timeSeries.Count * ratio);
                float val = timeSeries[idx].value.val;
                trendDots.Add(val / (maxVal - minVal));
                i++;
            }

            trendMeas.UpdateControlPoints(trendDots);
            SetTrendTimeRange(dtFrom, dtTo, maxVal, minVal);
        }
        private void OnChangeDateTime(bool isFrom, string value)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //미래 기록 조회 불가
                if (dt.Date > DateTime.Now.Date)
                    dt = DateTime.Now;

                //From < To가 참이게끔 교정
                if (!isFrom && dt < dtFrom)
                    DatetimeFrom = dt;

                if (isFrom && dt > dtTo)
                    DateTimeTo = dt;

                //저장
                if (isFrom) DatetimeFrom = dt; else DateTimeTo = dt;
            }
            catch { }
        }
        private void OnEndEditDateTime(bool isFrom, string value)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //미래 기록 조회 불가
                if (dt.Date > DateTime.Now.Date)
                    dt = DateTime.Now;

                //From < To가 참이게끔 교정
                if (!isFrom && dt < dtFrom)
                    DatetimeFrom = dt;

                if (isFrom && dt > dtTo)
                    DateTimeTo = dt;

                //저장
                if (isFrom) DatetimeFrom = dt; else DateTimeTo = dt;
            }
            finally
            {
                txbDatetime.from.SetTextWithoutNotify(dtFrom.ToString("yyyy-MM-dd"));
                txbDatetime.to.SetTextWithoutNotify(dtTo.ToString("yyyy-MM-dd"));
            }
        }
        private void OnClickQuery()
        {
            Debug.Log($"group {groupIdx} / obsIdx {obsIdx} / sensorIdx {sensorIdx}");
            UiManager.Instance.Invoke(UiEventType.RequestHistoryTimeSeries, (groupIdx, obsIdx, sensorIdx, dtFrom, dtTo));
        }
        private void OnChangeSensor(int arg0)
        {

            string tOption = ddlSensor.options[arg0].text;
            int idx = int.Parse(tOption.Split(':')[0]);
            var tList = sensors.FindAll(g => g.idx == idx);
            
            if (tList.Count ==0) 
            {
                Debug.LogError($"센서 정보 없음 - {idx}");
                return;
            }

            (int idx, ObservatoryInfo.SensorInfo info) info = tList.First();
            sensorIdx = info.idx;
        }
        private void OnChangeObservatory(int arg0)
        {
            string tOption = ddlObservatory.options[arg0].text;
            ObservatoryInfo info = obss.FirstOrDefault(g => g.nameText == tOption);

            if (info == null)
            {
                Debug.LogError($"관측소 정보 없음 - {info}");
                return;
            }

            obsIdx = info.obsIdx;
            sensors = info.sensors;

            ddlSensor.options = info.sensors.Where(sen => sen.info.isUsing).Select(sen => new TMP_Dropdown.OptionData($"{sen.idx}:{sen.info.name}")).ToList();

            ddlSensor.enabled = true;
            if (ddlSensor.options.Count == 0)
            {
                ddlSensor.enabled = false;
                ddlSensor.options.Add(new TMP_Dropdown.OptionData("(비어있음)"));
            }
            else
            {
                ddlSensor.value = 0;

                OnChangeSensor(0);
            }
        }
        private void OnChangeRegion(int arg0)
        {
            string tOption = ddlRegion.options[arg0].text;
            GroupInfo info = groups.FirstOrDefault(g => g.groupName == tOption);

            if(info == null) { 
                Debug.LogError($"지역 정보 없음 - {tOption}");
                return;
            }

            groupIdx = info.groupIdx;

            ddlObservatory.enabled = true;
            ddlObservatory.options = obss.Where(obs => obs.groupIdx == info.groupIdx).Select(obs => new TMP_Dropdown.OptionData(obs.nameText)).ToList();

            if (ddlObservatory.options.Count == 0)
            {
                
                ddlObservatory.enabled = false;
                ddlObservatory.options.Add(new TMP_Dropdown.OptionData("(비어있음)"));
                obsIdx = -1;

                ddlSensor.options.Clear();

                ddlSensor.enabled = false;
                ddlSensor.options.Add(new TMP_Dropdown.OptionData("(비어있음)"));
                sensorIdx = -1;

            }
            else
            {
                ddlObservatory.value = 0;
                OnChangeObservatory(0);
            }
        }
        private void OnInitiate(object obj)
        {
            trendMeas.SetDotAmount(120);
            trendPred.SetDotAmount(120);


            groups = modelProvider.GetGroups();
            obss = modelProvider.GetObss();
    
            //지역 드롭다운 초기화
            ddlRegion.options = groups.Select(group => new TMP_Dropdown.OptionData(group.groupName)).ToList();
            ddlRegion.value = 0;
        }


        // UI 제어
        private void SetTrendTimeRange(DateTime dtFrom, DateTime dtTo, float valMax, float valMin=0)
        {
            List<TMP_Text> txtTimes = trendMeas.transform.parent.Find("Chart_Grid").Find("Text_Horizon").GetComponentsInChildren<TMP_Text>().ToList();
            List<TMP_Text> txtVals = trendMeas.transform.parent.Find("Chart_Grid").Find("Text_Vertical").GetComponentsInChildren<TMP_Text>().ToList();

            for (int i = 0; i < txtTimes.Count; i++)
            {
                TMP_Text txtTime = txtTimes[i];
                float ratio = (float)i / (txtTimes.Count-1);
                DateTime time = dtFrom  + (dtTo - dtFrom) * ratio;
                txtTime.text = (dtTo - dtFrom).TotalDays > 1 ? (dtTo - dtFrom).TotalDays > 7? time.ToString("MM.dd\nHH:mm") : time.ToString("MM.dd\nHH:mm") : time.ToString("HH:mm");
            }
            for (int i = 0; i < txtVals.Count; i++)
            {
                TMP_Text txtVal = txtVals[i];
                float ratio = (float)i / (txtVals.Count-1);
                float val = valMin + (valMax - valMin) * (1f-ratio);
                txtVal.text = $"{val:0.##}";
            }
        }

        // 툴팁 제어

        #region 툴팁 시스템
        /// <summary>
        /// 멀티 디스플레이 환경에서 정확한 마우스 좌표 계산
        /// </summary>
        private bool TryGetPointerOnCanvas(Canvas canvas, out Vector2 screenPos)
        {
#if UNITY_EDITOR
            screenPos = Input.mousePosition;
            return true;
#else
        screenPos = default;
        int target = (canvas != null) ? canvas.targetDisplay : 0;

        if (target < 0 || target >= Display.displays.Length)
            return false;

        Vector3 raw = Input.mousePosition;
        Vector3 rel = Display.RelativeMouseAt(raw);
        
        if (rel != Vector3.zero && (int)rel.z == target)
        {
            screenPos = new Vector2(rel.x, rel.y);
            return true;
        }

        // 폴백 계산
        float x = raw.x;
        int count = Mathf.Min(target, Display.displays.Length - 1);
        for (int i = 0; i < count; i++)
            x -= Display.displays[i].systemWidth;

        Display disp = Display.displays[target];
        float sx = (disp.systemWidth > 0) ? (float)disp.renderingWidth / disp.systemWidth : 1f;
        float sy = (disp.systemHeight > 0) ? (float)disp.renderingHeight / disp.systemHeight : 1f;

        x *= sx;
        float y = raw.y * sy;

        if (x < 0 || y < 0 || x > disp.renderingWidth || y > disp.renderingHeight)
            return false;

        screenPos = new Vector2(x, y);
        return true;
#endif
        }

        void Update()
        {
            bool isInChart = IsMouseInChartArea();

            if (isInChart != wasMouseInChartArea)
            {
                Debug.Log(isInChart ? "🟢 차트 진입!" : "🔴 차트 퇴장!");
                wasMouseInChartArea = isInChart;
            }

            if (isInChart)
            {
                CheckMouseHover();
            }
            else if (tooltip.activeInHierarchy)
            {
                HideTooltip();
            }
        }

        /// <summary>
        /// 마우스가 차트 영역 내에 있는지 확인 (약간 확장된 영역)
        /// </summary>
        private bool IsMouseInChartArea()
        {
            if (chartArea == null) return false;

            var canvas = GetComponentInParent<Canvas>();

            if (!TryGetPointerOnCanvas(canvas, out var screenPos))
                return false;

            Vector2 localMousePos;
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                chartArea, screenPos, null, out localMousePos);

            if (!ok) return false;

            Rect expanded = chartArea.rect;
            expanded.xMax += 30; // 오른쪽 여백 확장
            return expanded.Contains(localMousePos);
        }

        private void CheckMouseHover()
        {
            if (originalValues.Count == 0) return;

            var canvas = GetComponentInParent<Canvas>();
            if (!TryGetPointerOnCanvas(canvas, out var screenPos))
            {
                HideTooltip();
                return;
            }

            Vector2 local;
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                chartArea, screenPos, null, out local);

            Rect expanded = chartArea.rect;
            expanded.xMax += 30;

            if (ok && expanded.Contains(local))
            {
                int idx = FindClosestDataPoint(local);
                if (idx >= 0) ShowTooltip(idx, screenPos);
            }
            else
            {
                HideTooltip();
            }
        }

        /// <summary>
        /// 마우스 위치에서 가장 가까운 데이터 포인트 찾기
        /// </summary>
        private int FindClosestDataPoint(Vector2 mousePos)
        {
            float minDistance = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < originalValues.Count; i++)
            {
                Vector2 pointPos = ConvertChartToLocalPosition(i, originalValues[i]);
                float distance = Vector2.Distance(mousePos, pointPos);

                if (distance < 20f && distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        /// <summary>
        /// 데이터 포인트를 차트 로컬 좌표로 변환
        /// </summary>
        private Vector2 ConvertChartToLocalPosition(int index, float value)
        {
            Rect chartRect = chartArea.rect;

            float normalizedIndex = (originalValues.Count > 1) ?
                (float)index / (originalValues.Count - 1) : 0f;

            float maxValue = originalValues.Max();
            float minValue = Mathf.Min(originalValues.Min(), 0f);

            float normalizedValue;
            if (maxValue > minValue)
            {
                normalizedValue = (value - minValue) / (maxValue - minValue);
            }
            else
            {
                normalizedValue = 0f;
            }

            normalizedValue = Mathf.Clamp01(normalizedValue);

            float xPos = chartRect.xMin + chartRect.width * normalizedIndex;
            float yPos = chartRect.yMin + chartRect.height * normalizedValue;

            return new Vector2(xPos, yPos);
        }

        /// <summary>
        /// 툴팁 표시 - 툴팁의 실제 부모 기준으로 위치 계산
        /// </summary>
        private void ShowTooltip(int index, Vector3 _)
        {
            //if (tooltip == null) return;
            //tooltip.SetActive(true);
            //float value = originalValues[index];
            //DateTime time = GetTimeForIndex(index);
            //if (txtTime != null) txtTime.text = time.ToString("yy.MM.dd HH:mm");
            //if (txtValue != null) txtValue.text = value.ToString("F2");
            //RectTransform tip = tooltip.GetComponent<RectTransform>();
            //if (tip == null) return;
            //// ✅ 노드점 위치 가져오기
            //if (line == null ||
            //    line.controlPointsObjects == null ||
            //    index >= line.controlPointsObjects.Count ||
            //    line.controlPointsObjects[index] == null)
            //{
            //    HideTooltip();
            //    return;
            //}
            //Transform nodePoint = line.controlPointsObjects[index];
            //var canvas = GetComponentInParent<Canvas>();
            //// ✅ 노드점의 월드 좌표 → 툴팁 부모의 로컬 좌표로 변환
            //RectTransform tooltipParent = tip.parent as RectTransform;

            //Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            //    canvas.worldCamera,
            //    nodePoint.position);
            //Vector2 localPosInTooltipParent;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //    tooltipParent,
            //    screenPos,
            //    canvas.worldCamera,
            //    out localPosInTooltipParent);
            //// ✅ 툴팁을 노드점 위쪽에 배치
            //float offsetY = tip.rect.height / 2 + 20;
            //float offsetX = 0;

            //// ✅ 오른쪽 끝 4개 노드점은 왼쪽으로 보정
            //if (index >= originalValues.Count - 4)
            //{
            //    offsetX = -tip.rect.width / 2 - 10;  // 툴팁 너비의 절반 + 여백 10픽셀
            //}

            //tip.anchoredPosition = new Vector2(
            //    localPosInTooltipParent.x + offsetX,
            //    localPosInTooltipParent.y + offsetY
            //);
        }
        private void HideTooltip()
        {
            tooltip.SetActive(false);
        }

        /// <summary>
        /// 인덱스에 해당하는 실제 DB 시간 반환
        /// </summary>
        private DateTime GetTimeForIndex(int index)
        {

            //TODO
            //originalValues



            return DateTime.Now; // 기본값
        }
        #endregion
    }
}
