using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 알람 시점 센서 상태를 막대 형태로 표시하는 UI 컴포넌트
/// 센서명, 측정값, 상태등, 트렌드 그래프를 포함
/// </summary>
internal class ToxinBar2 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    #region [UI 컴포넌트]
    UILineRenderer trdSensor;       // 트렌드 그래프 (작은 라인 차트)
    Image imgSignalLamp;            // 상태 표시등 (색상으로 상태 구분)
    TMP_Text lblSensorName;         // 센서 이름 라벨
    TMP_Text lblUnit;               // 측정 단위 라벨 
    TMP_Text lblValue;              // 측정값 라벨
    Button btnSelectCurrentSensor;  // 센서 선택 버튼
    #endregion

    #region [데이터]
    ToxinData toxin;        // 센서 데이터
    int obsId = 0;          // 관측소 ID
    ToxinStatus sensorStatus; // 센서 상태 (정상/경계/경보/설비이상)
    #endregion

    // 상태별 색상 딕셔너리
    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    /// <summary>
    /// 상태별 색상 초기화
    /// </summary>
    static ToxinBar2()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#3EFF00"}, 
            { ToxinStatus.Yellow,   "#FFF600"}, 
            { ToxinStatus.Red,      "#FF0000"}, 
            { ToxinStatus.Purple,   "#6C00E2"}, 
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                statusColorDic[pair.Key] = color;
    }

    /// <summary>
    /// UI 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        // 자식 컴포넌트들 찾기
        trdSensor = GetComponentInChildren<UILineRenderer>();                           // 트렌드 그래프
        imgSignalLamp = transform.Find("Icon_SignalLamp").GetComponent<Image>();       // 상태등
        lblSensorName = transform.Find("Text (TMP) List").GetComponent<TMP_Text>();    // 센서명
        lblValue = transform.Find("Text (TMP) List (1)").GetComponent<TMP_Text>();     // 측정값
        lblUnit = transform.Find("Text (TMP) List (2)").GetComponent<TMP_Text>();      // 단위 (기존에는 임계값이었음)

        // 버튼 클릭 이벤트 등록
        btnSelectCurrentSensor = GetComponent<Button>();
        btnSelectCurrentSensor.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 센서 데이터 설정 - 핵심 메서드
    /// </summary>
    /// <param name="obsId">관측소 ID</param>
    /// <param name="toxin">센서 데이터</param>
    /// <param name="status">센서 상태</param>
    public void SetToxinData(int obsId, ToxinData toxin, ToxinStatus status)
    {
        this.toxin = toxin;
        this.sensorStatus = status;
        this.obsId = obsId;

        // UI 라벨 업데이트
        lblSensorName.text = toxin.hnsName;                                // 센서명
        lblValue.text = "" + toxin.GetLastValue().ToString("F2");          // 최신 측정값 (소수점 2자리)
        lblUnit.text = toxin.unit ?? "";                                   // 측정 단위

        // 센서 활성화 여부에 따라 표시/숨김
        gameObject.SetActive(toxin.on);

        // 상태에 따른 신호등 색상 변경
        imgSignalLamp.color = statusColorDic[sensorStatus];

        // 트렌드 그래프 업데이트
        UpdateTrendGraph();
    }

    /// <summary>
    /// 트렌드 그래프 업데이트
    /// </summary>
    private void UpdateTrendGraph()
    {
        // 측정값이 없으면 그래프 그리지 않음
        if (toxin.values.Count == 0) return;

        List<float> normalizedValues = new();

        // 최대값 + 1을 기준으로 정규화 (0으로 나누기 방지)
        float max = toxin.values.Max() + 1;

        // 모든 값을 0~1 범위로 정규화
        toxin.values.ForEach(val => normalizedValues.Add(val / max));

        // 트렌드 그래프 업데이트 (전체 데이터 사용)
        trdSensor.UpdateControlPoints(normalizedValues);
    }

    /// <summary>
    /// 센서 바 클릭 시 - 해당 센서 상세 정보로 이동
    /// </summary>
    private void OnClick()
    {
        Debug.Log($"ToxinBar2 클릭됨: {toxin.hnsName}, boardId={toxin.boardid}, hnsId={toxin.hnsid}");

        int boardId = toxin.boardid;
        int hnsId = toxin.hnsid;

        // 알람 센서 선택 이벤트 발생 (센서 상세 화면으로 이동)
        UiManager.Instance.Invoke(UiEventType.SelectAlarmSensor, (boardId, hnsId));
    }
}