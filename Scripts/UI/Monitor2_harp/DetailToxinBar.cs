using Newtonsoft.Json.Linq;
using Onthesys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;

/// <summary>
/// 상세 독성 차트 바 - 개별 센서의 시계열 데이터 차트 + 툴팁 + 기간 선택
/// </summary>
internal class DetailToxinBar : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private ToxinData toxinData;

    private DateTime aladt;
    public TMP_Text txtName;
    public TMP_Text txtQueryTime;
    public UILineRenderer2 line;
    public List<TMP_Text> hours;      // 시간축 라벨
    public List<TMP_Text> verticals;  // 값축 라벨
    public GameObject btnDetail;

    public TMP_Dropdown periodDropdown; // 기간 선택 (1일/7일/30일)

    [Header("Table Popup")]
    public UnityEngine.UI.Button btnShowTable;              // 표 보기 버튼
    public PopupTableData popupTableData;    // 팝업 참조


    #region 툴팁 컴포넌트
    [Header("Tooltip Components")]
    public GameObject tooltip;
    public TMP_Text txtTime;
    public TMP_Text txtValue;
    public RectTransform chartArea; // 차트 영역
    #endregion

    private List<float> originalValues = new(); // 툴팁용 원본 값들
    private bool wasMouseInChartArea = false;   // 마우스 진입/퇴장 추적

    void Start()
    {
        Initialize();
        InitializeDropdown();

        UiManager.Instance.Register(UiEventType.SelectCurrentSensor, OnSelectCurrentSensor);
        UiManager.Instance.Register(UiEventType.RefreshDetailChart, OnRefreshDetailChart);
        btnDetail.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);

        if (btnShowTable != null)
            btnShowTable.onClick.AddListener(OnClickShowTable);

        tooltip.SetActive(false);
    }

    /// <summary>
    /// 표 보기 버튼 클릭
    /// </summary>
    private void OnClickShowTable()
    {
        // 센서 데이터 확인
        if (toxinData == null)
        {
            Debug.LogWarning("[DetailToxinBar] 센서를 먼저 선택해주세요.");
            UiManager.Instance.Invoke(UiEventType.PopupErrorMonitorB,
                new Exception("센서를 먼저 선택해주세요."));
            return;
        }
        if (toxinData == null)
        {
            Debug.LogWarning("표시할 데이터가 없습니다.");
            return;
        }

        if (popupTableData != null)
        {
            popupTableData.ShowPopup(toxinData);
        }
        else
        {
            Debug.LogError("PopupTableData 참조가 연결되지 않았습니다!");
        }
    }

    /// <summary>
    /// AI 상세 분석 팝업 열기
    /// </summary>
    private void OnClick()
    {
        if (toxinData == null) return;

        if (toxinData.aiValues != null && toxinData.aiValues.Count > 0)
        {
            UiManager.Instance.Invoke(UiEventType.Popup_AiTrend, (toxinData.boardid, toxinData.hnsid));
        }
        else
        {
            UiManager.Instance.Invoke(UiEventType.PopupErrorMonitorB,
                new Exception("AI 분석값이 없습니다."));
        }
    }

    /// <summary>
    /// 트렌드 라인 업데이트 시 차트 다시 그리기
    /// </summary>
    private void OnRefreshDetailChart(object obj)
    {
        if (toxinData == null) return;

        if (toxinData.values.Count == 0)
            toxinData = modelProvider.GetToxin(toxinData.boardid, toxinData.hnsid);

        UpdateQueryTime();
        SetDynamicHours(1);

        List<float> normalizedValues = new();
        float max = toxinData.values.Max();

        float normalizeMax = max + 1;

        if (max <= 0)
        {
            toxinData.values.ForEach(val => normalizedValues.Add(0f));
        }
        else
        {
            toxinData.values.ForEach(val => normalizedValues.Add(val / normalizeMax));
        }

        line.UpdateControlPoints(normalizedValues);

        originalValues.Clear();
        originalValues.AddRange(toxinData.values);
    }

    private void Initialize()
    {
        SetDynamicHours(1);
        this.txtName.text = "";
        this.btnDetail.SetActive(true);
    }

    /// <summary>
    /// 센서 선택 시 해당 센서의 차트 표시
    /// </summary>
    private void OnSelectCurrentSensor(object obj)
    {
        if (obj is not (int boardId, int hnsId))
        {
            Debug.LogError("[OnSelectCurrentSensor] 잘못된 파라미터 전달됨 (expected: (int, int))");
            return;
        }

        toxinData = modelProvider.GetToxin(boardId, hnsId);

        if (toxinData == null)
        {
            Debug.LogError($"[OnSelectCurrentSensor] 해당 센서 데이터 없음 (boardId={boardId}, hnsId={hnsId})");
            return;
        }

        if (toxinData.values == null || toxinData.values.Count == 0)
        {
            Debug.LogWarning($"[OnSelectCurrentSensor] 센서 값이 비어 있음 (boardId={boardId}, hnsId={hnsId})");
            return;
        }

        txtName.text = toxinData.hnsName;
        UpdateQueryTime();

        Debug.Log($"Min: {toxinData.values.Min()}, Max: {toxinData.values.Max()}, Average: {toxinData.values.Average()}");

        List<float> normalizedValues = new();
        float max = toxinData.values.Max();

        // ✅ +1 추가!
        float normalizeMax = max + 1;

        if (max <= 0)
        {
            toxinData.values.ForEach(val => normalizedValues.Add(0f));
        }
        else
        {
            toxinData.values.ForEach(val => normalizedValues.Add(val / normalizeMax));
        }

        SetVertical(max);
        SetDynamicHours(1);
        line.UpdateControlPoints(normalizedValues);

        Debug.Log($"[OnSelectCurrentSensor] 그래프 정상 업데이트 완료 (boardId={boardId}, hnsId={hnsId})");
        originalValues.Clear();
        originalValues.AddRange(toxinData.values);
    }

    /// <summary>
    /// 새 메서드: 조회 시점 업데이트
    /// </summary>
    private void UpdateQueryTime()
    {
        if (txtQueryTime == null) return;

        DateTime queryTime = modelProvider.GetCurrentChartEndTime();

        // 데이터가 있으면 마지막 시간 사용
        if (toxinData?.dateTimes != null && toxinData.dateTimes.Count > 0)
        {
            queryTime = toxinData.dateTimes.Last();
        }

        txtQueryTime.text = $"조회시점: {queryTime:yyyy.MM.dd HH:mm}";
    }

    /// <summary>
    /// 기간 선택 드롭다운 초기화 (1일/7일/30일)
    /// </summary>
    private void InitializeDropdown()
    {
        if (periodDropdown == null)
        {
            Debug.LogError("Dropdown 객체가 할당x");
            return;
        }

        var options = new List<string> { "1일", "7일", "30일" };
        periodDropdown.ClearOptions();
        periodDropdown.AddOptions(options);

        periodDropdown.onValueChanged.AddListener(value =>
        {
            int periodDays = GetSelectedPeriodDays();
            Debug.Log($"기간 선택: {periodDays}일");
            //TODO Set Period
        });

        Debug.Log("Dropdown 초기화 완료");
    }

    public void OnSelectLog(object data)
    {
        if (data is LogData logData)
        {
            this.aladt = logData.time;
            UpdateChartData(1);
        }
    }

    public void OnSelectToxin(object data)
    {
        if (data is ToxinData toxinData)
        {
            this.toxinData = toxinData;
            this.txtName.text = toxinData.hnsName;
            this.btnDetail.SetActive(true);

            int periodDays = GetSelectedPeriodDays();
            UpdateChartData(periodDays);
        }
    }

    private int GetSelectedPeriodDays()
    {
        int periodDays = 1;
        switch (periodDropdown.value)
        {
            case 0: periodDays = 1; break; // 1일
            case 1: periodDays = 7; break; // 7일
            case 2: periodDays = 30; break; // 30일
        }
        return periodDays;
    }

    private void UpdateChartData(int periodDays)
    {
        if (toxinData == null)
        {
            Debug.LogWarning("ToxinData is null. 랜덤 데이터로 그래프를 초기화합니다.");
            return;
        }

        var convertedData = ConvertToChartData(toxinData);
        line.UpdateControlPoints(convertedData);
        SetVertical(Mathf.Max(convertedData.Max()));
        SetDynamicHours(periodDays);
    }

    private List<float> ConvertToChartData(ToxinData toxin)
    {
        if (toxin == null) throw new Exception("DetailToxinBar.toxinData is null. Cannot draw the graph!");

        var max = toxin.values.Max();
        var lchart = new List<float>();
        if (max > toxin.warning)
        {
            lchart = toxin.values.Select(t => t / max).ToList();
        }
        else
        {
            max = toxin.warning;
            lchart = toxin.values.Select(t => t / toxin.warning).ToList();
        }
        SetVertical(max);
        return lchart;
    }

    /// <summary>
    /// 실제 DB 시간 데이터로 시간축 라벨 설정
    /// </summary>
    private void SetDynamicHours(int periodDays)
    {
        if (toxinData?.dateTimes != null && toxinData.dateTimes.Count > 0)
        {
            DateTime actualStartTime = toxinData.dateTimes.First();
            DateTime actualEndTime = toxinData.dateTimes.Last();

            var interval = (actualEndTime - actualStartTime).TotalMinutes / (this.hours.Count - 1);

            for (int i = 0; i < this.hours.Count; i++)
            {
                var t = actualStartTime.AddMinutes(interval * i);
                this.hours[i].text = t.ToString("MM-dd\nHH:mm");
            }
        }
    }

    /// <summary>
    /// 값축 라벨 설정 - 역순으로 표시 (위쪽이 큰 값)
    /// </summary>
    private void SetVertical(float max)
    {
        var verticalMax = max + 1;

        for (int i = 0; i < this.verticals.Count; i++)
        {
            float ratio = ((float)this.verticals.Count - i - 1) / (verticals.Count - 1);
            this.verticals[i].text = Math.Round((verticalMax * ratio)).ToString();
        }
    }

    public void OnDetailSelect()
    {
        if (toxinData == null)
        {
            Debug.LogWarning("No toxin data available.");
            return;
        }

        UiManager.Instance.Invoke(UiEventType.SelectAlarmSensor, (this.aladt, this.toxinData.boardid, this.toxinData.hnsid));
    }

    private void STORED_OnChangeTrendLine(object data)
    {
        if (data is int periodDays)
        {
            Debug.Log($"ChangeTrendLine 이벤트 수신: {periodDays}일");
            SetDynamicHours(periodDays);
            UpdateChartData(periodDays);
        }
    }

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
        if (toxinData == null || originalValues.Count == 0) return;

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
        if (tooltip == null) return;
        tooltip.SetActive(true);
        float value = originalValues[index];
        DateTime time = GetTimeForIndex(index);
        if (txtTime != null) txtTime.text = time.ToString("yy.MM.dd HH:mm");
        if (txtValue != null) txtValue.text = value.ToString("F2");
        RectTransform tip = tooltip.GetComponent<RectTransform>();
        if (tip == null) return;
        // ✅ 노드점 위치 가져오기
        if (line == null ||
            line.controlPointsObjects == null ||
            index >= line.controlPointsObjects.Count ||
            line.controlPointsObjects[index] == null)
        {
            HideTooltip();
            return;
        }
        Transform nodePoint = line.controlPointsObjects[index];
        var canvas = GetComponentInParent<Canvas>();
        // ✅ 노드점의 월드 좌표 → 툴팁 부모의 로컬 좌표로 변환
        RectTransform tooltipParent = tip.parent as RectTransform;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera,
            nodePoint.position);
        Vector2 localPosInTooltipParent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParent,
            screenPos,
            canvas.worldCamera,
            out localPosInTooltipParent);
        // ✅ 툴팁을 노드점 위쪽에 배치
        float offsetY = tip.rect.height / 2 + 20;
        float offsetX = 0;

        // ✅ 오른쪽 끝 4개 노드점은 왼쪽으로 보정
        if (index >= originalValues.Count - 4)
        {
            offsetX = -tip.rect.width / 2 - 10;  // 툴팁 너비의 절반 + 여백 10픽셀
        }

        tip.anchoredPosition = new Vector2(
            localPosInTooltipParent.x + offsetX,
            localPosInTooltipParent.y + offsetY
        );
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
        if (toxinData?.dateTimes != null &&
            index >= 0 && index < toxinData.dateTimes.Count)
        {
            return toxinData.dateTimes[index];
        }

        return DateTime.Now; // 기본값
    }
    #endregion
}