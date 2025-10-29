using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

/// <summary>
/// 관측소 모니터링 화면의 개별 센서 아이템 - 센서 상태/값 표시 및 트렌드 차트
/// </summary>
public class ObsMonitoringItem : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    #region [UI 컴포넌트들]
    UILineRenderer trdSensor;      // 센서 트렌드 차트
    Image imgSignalLamp;           // 상태 표시 램프
    TMP_Text lblSensorName;        // 센서 이름
    TMP_Text lblUnit;              // 측정 단위
    TMP_Text lblValue;             // 현재 측정값
    Button btnSelectCurrentSensor; // 센서 선택 버튼
    #endregion

    #region [데이터]
    ToxinData toxin;  // 센서 데이터
    int obsId = 0;    // 관측소 ID
    #endregion

    // 상태별 색상 딕셔너리
    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    /// <summary>
    /// 상태별 색상 초기화
    /// </summary>
    static ObsMonitoringItem()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#3EFF00"}, // 정상 - 녹색
            { ToxinStatus.Yellow,   "#FFF600"}, // 경계 - 노랑
            { ToxinStatus.Red,      "#FF0000"}, // 경보 - 빨강
            { ToxinStatus.Purple,   "#6C00E2"}, // 설비이상 - 보라
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
        trdSensor = GetComponentInChildren<UILineRenderer>();
        imgSignalLamp = transform.Find("Icon_SignalLamp").GetComponent<Image>();
        lblSensorName = transform.Find("Text (TMP) List").GetComponent<TMP_Text>();
        lblValue = transform.Find("Text (TMP) List (1)").GetComponent<TMP_Text>();
        lblUnit = transform.Find("Text (TMP) List (2)").GetComponent<TMP_Text>();

        btnSelectCurrentSensor = GetComponent<Button>();
        btnSelectCurrentSensor.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 센서 데이터 설정 - 핵심 메서드
    /// </summary>
    /// <param name="obsId">관측소 ID</param>
    /// <param name="toxin">센서 데이터</param>
    public void SetToxinData(int obsId, ToxinData toxin)
    {
        this.toxin = toxin;
        this.obsId = obsId;

        // UI 텍스트 설정
        lblSensorName.text = toxin.hnsName;                    // 센서명 (예: "용존산소량")
        lblValue.text = toxin.GetLastValue().ToString("F2");   // 현재값 (소수점 2자리)
        lblUnit.text = toxin.unit ?? "";                       // 단위 (예: "mg/L")

        // 센서 활성화 여부에 따른 표시/숨김
        gameObject.SetActive(toxin.on);

        // 상태에 따른 신호등 색상 변경
        imgSignalLamp.color = statusColorDic[toxin.status];
    }

    /// <summary>
    /// 센서 상태 재설정 - 값과 색상만 업데이트
    /// </summary>
    public void ResetToxinStatus()
    {
        if (obsId < 1 || toxin == null) return;

        lblValue.text = toxin.GetLastValue().ToString("F2");  // 최신 측정값 업데이트
        imgSignalLamp.color = statusColorDic[toxin.status];   // 상태 색상 업데이트
    }

    /// <summary>
    /// 센서 선택 버튼 클릭 - 해당 센서를 현재 센서로 선택
    /// </summary>
    private void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (toxin.boardid, toxin.hnsid));
    }

    /// <summary>
    /// 트렌드 라인 업데이트 - 센서 측정값들을 차트로 표시
    /// </summary>
    public void UpdateTrendLine()
    {
        if (toxin == null) return;
        if (toxin.values.Count == 0) return;

        // 최대값과 최소값 모두 구하기
        var max = toxin.values.Max();
        var min = toxin.values.Min();

        // 음수가 있으면 min을 0 아래로 확장
        if (min < 0)
        {
            // 음수 범위 고려 (그대로 사용)
        }
        else
        {
            min = 0; // 양수만 있으면 0부터 시작
        }

        // min-max 범위로 정규화 (0~1 범위)
        float range = max - min;
        if (range <= 0) range = 1; // 0으로 나누기 방지

        List<float> normalizedValues = toxin.values.Select(t => (t - min) / range).ToList();

        // 트렌드 차트 업데이트
        trdSensor.UpdateControlPoints(normalizedValues);

        // 현재값 텍스트 업데이트
        lblValue.text = toxin.GetLastValue().ToString("F2");
    }
}