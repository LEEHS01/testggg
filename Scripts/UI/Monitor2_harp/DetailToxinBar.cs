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


internal class DetailToxinBar : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private ToxinData toxinData;

    private DateTime aladt;
    public TMP_Text txtName;
    public UILineRenderer2 line;
    public List<TMP_Text> hours;
    public List<TMP_Text> verticals;
    public GameObject btnDetail;

    //private int periodDays = 1;
    public TMP_Dropdown periodDropdown;

    //툴팁
    [Header("Tooltip Components")]
    public GameObject tooltip;
    public TMP_Text txtTime;
    public TMP_Text txtValue;
    public RectTransform chartArea;

    private List<float> originalValues = new();



    void Start()
    {
        Initialize();
        InitializeDropdown();

        UiManager.Instance.Register(UiEventType.SelectCurrentSensor, OnSelectCurrentSensor);
        //UiManager.Instance.Register(UiEventType.SelectCurrentSensor, OnSelectLog);
        //UiManager.Instance.Register(UiEventType.SelectAlarmSensor, OnSelectToxin);
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine); // 이벤트 등록
        btnDetail.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);

        /*tooltip = transform.Find("Tooltip").gameObject;
        txtTime = tooltip.transform.Find("txtTime").GetComponent<TMP_Text>();
        txtValue = tooltip.transform.Find("txtValue").GetComponent<TMP_Text>();
        chartArea = transform.Find("Chart_Grid").GetComponent<RectTransform>();*/

        tooltip.SetActive(false);

        //UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (1, 0)); // 디폴트 센서 강제 트리거
    }

    private void OnClick()
    {
        if (toxinData == null) return;

        // 로그 데이터 확인
        List<ToxinData> logToxins = modelProvider.GetToxinsInLog();

        // 로그 데이터에서 현재 센서와 매칭되는 데이터 찾기
        foreach (var logToxin in logToxins)
        {
            // 현재 센서와 매칭되는지 확인
            if (logToxin.boardid == toxinData.boardid && logToxin.hnsid == toxinData.hnsid)
            {
                if (logToxin.aiValues != null && logToxin.aiValues.Count > 0)
                {
                    // AI값이 존재하면 상세 팝업 열기
                    UiManager.Instance.Invoke(UiEventType.Popup_AiTrend, (toxinData.boardid, toxinData.hnsid));
                    return;
                }
            }
        }

        // AI값이 없으면 에러 팝업 표시
        UiManager.Instance.Invoke(UiEventType.PopupErrorMonitorB,
            new Exception("AI 분석값이 없습니다. 알람 로그를 먼저 선택해주세요."));
        //if (toxinData == null) return;
        //UiManager.Instance.Invoke(UiEventType.Popup_AiTrend, (toxinData.boardid, toxinData.hnsid));
    }

    private void OnChangeTrendLine(object obj)
    {
        if (toxinData == null) return;

        if (toxinData.values.Count == 0)
            toxinData = modelProvider.GetToxin(toxinData.boardid, toxinData.hnsid);


        List<float> normalizedValues = new();
        float max = Math.Max(toxinData.values.Max(), toxinData.warning);
        toxinData.values.ForEach(val => normalizedValues.Add(val / max));

        line.UpdateControlPoints(normalizedValues);

        originalValues.Clear();
        originalValues.AddRange(toxinData.values);

    }
    /*
    private void OnSelectCurrentSensor(object obj)
    {
        if (obj is not (int boardId, int hnsId)) return;

        toxinData = modelProvider.GetToxin(toxinData.boardid, toxinData.hnsid);

        txtName.text = toxinData.hnsName;

        List<float> normalizedValues = new();
        float max = Math.Max(toxinData.values.Max(), toxinData.warning);
        toxinData.values.ForEach(val => normalizedValues.Add(val / max));
        SetVertical(max);
        SetDynamicHours(1);
        line.UpdateControlPoints(normalizedValues);
    }
    */
    private void Initialize()
    {
        SetDynamicHours(1);
        this.txtName.text = "";
        this.btnDetail.SetActive(true);
    }

    

    private void OnSelectCurrentSensor(object obj)
    {
        // 전달된 값이 튜플 형식이 아닌 경우
        if (obj is not (int boardId, int hnsId))
        {
            Debug.LogError("[OnSelectCurrentSensor] 잘못된 파라미터 전달됨 (expected: (int, int))");
            return;
        }

        // ToxinData 조회
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
        Debug.Log($"toxinData.values 개수: {toxinData.values.Count}");
        Debug.Log($"toxinData.warning: {toxinData.warning}");

        // 이름 표시
        txtName.text = toxinData.hnsName;

        // 정규화 후 라인 그래프 업데이트
        List<float> normalizedValues = new();
        float max = Math.Max(toxinData.values.Max(), toxinData.warning);
        toxinData.values.ForEach(val => normalizedValues.Add(val / max));

        SetVertical(max);
        SetDynamicHours(1);
        line.UpdateControlPoints(normalizedValues);

        Debug.Log($"[OnSelectCurrentSensor] 그래프 정상 업데이트 완료 (boardId={boardId}, hnsId={hnsId})");
        originalValues.Clear();
        originalValues.AddRange(toxinData.values);
    }

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
            case 0:
                periodDays = 1;  // 1일
                break;
            case 1:
                periodDays = 7;  // 7일
                break;
            case 2:
                periodDays = 30; // 30일
                break;
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
        SetVertical(Mathf.Max( convertedData.Max(), toxinData.warning));
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

    private void SetDynamicHours(int periodDays)
    {
        DateTime endDt = DateTime.Now;
        //DateTime startDt = endDt.AddDays(-periodDays);
        DateTime startDt = endDt.AddHours(-12); // 직접 12시간
        var interval = (endDt - startDt).TotalMinutes / (this.hours.Count-1);

        for (int i = 0; i < this.hours.Count; i++)
        {
            var t = startDt.AddMinutes(interval * i);
            this.hours[i].text = t.ToString("MM-dd HH:mm");
        }
    }

    private void SetVertical(float max)
    {
        var verticalMax = max + 1;

        for (int i = 0; i < this.verticals.Count; i++)
        {
            float ratio = ((float)this.verticals.Count - i-1) / (verticals.Count-1);
            //Debug.Log("ratio : " + ratio);
            //Debug.Log("Math.Round((verticalMax * ratio),2) : " + Math.Round((verticalMax * ratio),2));
            this.verticals[i].text = Math.Round((verticalMax * ratio), 2).ToString();
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

    #region 툴팁
    private Vector3 GetDisplay2MousePosition()
    {
        Vector3 mousePos = Input.mousePosition;

        // Input.mousePosition은 항상 Display 1 기준이므로
        // Display 2로 변환하려면 Display 1의 너비만큼 빼기
        if (Display.displays.Length > 1)
        {
            mousePos.x -= Display.displays[0].systemWidth;

            // 마우스가 Display 1에 있으면 음수가 됨
            if (mousePos.x < 0 || mousePos.x > Display.displays[1].systemWidth)
            {
                return Vector3.negativeInfinity; // 범위 밖임을 표시
            }
        }

        return mousePos;
    }

    void Update()
    {
        //Debug.Log("DetailToxinBar Update() 실행됨!");
        CheckMouseHover();
    }
    private void CheckMouseHover()
    {
        if (toxinData == null || originalValues.Count == 0) return;

        // Display 2 기준 마우스 좌표 가져오기
        Vector3 display2MousePos = GetDisplay2MousePosition();

        // 마우스가 Display 2 범위에 없으면 툴팁 숨기기
        if (display2MousePos == Vector3.negativeInfinity)
        {
            HideTooltip();
            return;
        }

        Vector2 mousePos;
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            chartArea, display2MousePos, null, out mousePos);

        /*// *** 핵심 변경: 확장 영역 제거, 순수 chartArea.rect만 사용 ***
        if (isInside && chartArea.rect.Contains(mousePos))
        {
            int closestIndex = FindClosestDataPoint(mousePos);
            if (closestIndex >= 0)
            {
                ShowTooltip(closestIndex, display2MousePos);
            }
        }
        else
        {
            HideTooltip();
        }*/

        Rect expandedRect = chartArea.rect;
        expandedRect.xMax += 30;

        if (isInside && expandedRect.Contains(mousePos))
        {
            int closestIndex = FindClosestDataPoint(mousePos);
            if (closestIndex >= 0)
            {
                ShowTooltip(closestIndex, display2MousePos);
            }
        }
        else
        {
            HideTooltip();
        }
    }

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

    private Vector2 ConvertChartToLocalPosition(int index, float value)
    {
        Rect chartRect = chartArea.rect;

        // 인덱스를 0~1 범위로 정규화
        float normalizedIndex = (originalValues.Count > 1) ?
            (float)index / (originalValues.Count - 1) : 0f;

        // 값을 0~1 범위로 정규화 (음수 방지)
        float maxValue = Mathf.Max(originalValues.Max(), toxinData.warning);
        float minValue = Mathf.Min(originalValues.Min(), 0f); // 최소값도 고려

        float normalizedValue;
        if (maxValue > minValue)
        {
            normalizedValue = (value - minValue) / (maxValue - minValue);
        }
        else
        {
            normalizedValue = 0f;
        }

        // 0~1 범위로 클램핑
        normalizedValue = Mathf.Clamp01(normalizedValue);

        // 실제 픽셀 위치 계산
        float xPos = chartRect.xMin + chartRect.width * normalizedIndex;
        float yPos = chartRect.yMin + chartRect.height * normalizedValue;

        // 디버깅 로그 추가
        Debug.Log($"노드 {index}: value={value:F2}, normalizedValue={normalizedValue:F3}, " +
                 $"maxValue={maxValue:F2}, minValue={minValue:F2}, yPos={yPos:F2}");

        Vector2 result = new Vector2(xPos, yPos);
        return result;
    }

    private void ShowTooltip(int index, Vector3 screenPosition)
    {
        if (tooltip == null) return;

        tooltip.SetActive(true);

        float value = originalValues[index];
        DateTime time = GetTimeForIndex(index);

        if (txtTime != null) txtTime.text = time.ToString("yyMMdd - HH:mm");
        if (txtValue != null) txtValue.text = value.ToString("F2");

        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        if (tooltipRect == null) return;

        // Display 2 기준 마우스 좌표 사용
        Vector3 display2MousePos = GetDisplay2MousePosition();

        if (display2MousePos == Vector3.negativeInfinity)
        {
            HideTooltip();
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 localPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            display2MousePos,
            canvas.worldCamera,
            out localPos))
        {
            Vector2 tooltipSize = tooltipRect.sizeDelta;

            // 위치 조정
            localPos.y += tooltipSize.y / 2 + 60 ;

            if (index >= originalValues.Count - 4)
            {
                localPos.x -= 400;
                /* localPos.x -= tooltipSize.x + 0;*/
            }
            else
            {
                localPos.x -= 350;
            }

            tooltipRect.anchoredPosition = localPos;
        }
    }

    private void HideTooltip()
    {
        tooltip.SetActive(false);
    }

    private DateTime GetTimeForIndex(int index)
    {
        DateTime endTime = DateTime.Now;
        DateTime startTime = endTime.AddHours(-12);
        double intervalMinutes = (endTime - startTime).TotalMinutes / (originalValues.Count - 1);

        return startTime.AddMinutes(intervalMinutes * index);
    }
    #endregion


}

