using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// 알람 센서 클릭 시 표시되는 센서 상세 정보 팝업
/// 센서의 현재값, 임계값, 상태, 12시간 트렌드 차트를 표시
/// </summary>
internal class PopupDetailToxin2 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private ToxinData data; // 현재 표시 중인 센서 데이터

    #region [UI 컴포넌트]
    [Header("센서 정보 텍스트")]
    public TMP_Text txtName;     // 센서명
    public TMP_Text txtCurrent;  // 현재 측정값
    public TMP_Text txtTotal;    // 경계 임계값

    [Header("차트 관련")]
    public UILineRenderer2 line; // 트렌드 라인 차트
    public List<TMP_Text> hours; // 시간축 라벨들
    public List<TMP_Text> verticals; // 세로축 라벨들

    [Header("상태 아이콘들")]
    public GameObject statusGreen;   // 정상 상태 아이콘
    public GameObject statusRed;     // 경보 상태 아이콘  
    public GameObject statusYellow;  // 경계 상태 아이콘
    public GameObject statusPurple;  // 설비이상 상태 아이콘
    #endregion

    private void Start()
    {
        // 알람 센서 선택 이벤트 등록
        UiManager.Instance.Register(UiEventType.SelectAlarmSensor, OnSelectCurrentToxin);

        // 초기에는 팝업 숨김
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 알람 센서 선택 시 - 해당 센서의 상세 정보 표시
    /// </summary>
    /// <param name="data">선택된 센서의 (boardId, hnsId) 튜플</param>
    public void OnSelectCurrentToxin(object data)
    {
        // 팝업 활성화
        gameObject.SetActive(true);

        // 데이터 검증
        if (data is not (int boardId, int hnsId)) return;

        // 해당 센서 데이터 가져오기
        ToxinData toxin = modelProvider.GetToxin(boardId, hnsId);
        if (toxin == null) return;

        this.data = toxin;

        // 센서 기본 정보 업데이트
        UpdateSensorInfo();

        // 트렌드 차트 업데이트
        UpdateTrendChart();

        // 상태 아이콘 설정
        SetStatusIcon(this.data.status);

        // 차트 축 설정
        SetMins(DateTime.Now);
        SetVertical(this.data.values.Max());
    }

    /// <summary>
    /// 센서 기본 정보 업데이트
    /// </summary>
    private void UpdateSensorInfo()
    {
        txtName.text = data.hnsName;                                    // 센서명
        txtCurrent.text = Math.Round(data.GetLastValue(), 2).ToString(); // 현재 측정값 (소수점 2자리)
        txtTotal.text = Math.Round(data.warning, 2).ToString();         // 경계 임계값 (소수점 2자리)
    }

    /// <summary>
    /// 트렌드 차트 업데이트 (12시간 데이터)
    /// </summary>
    private void UpdateTrendChart()
    {
        if (data.values.Count == 0) return;

        // 최대값 기준으로 정규화 (0~1 범위)
        var max = data.values.Max();
        if (max <= 0) max = 1; // 0으로 나누기 방지

        var normalizedChart = data.values.Select(t => t / max).ToList();

        // 라인 차트 업데이트
        line.UpdateControlPoints(normalizedChart);
    }

    /// <summary>
    /// 상태 아이콘 설정 - 현재 상태에 맞는 아이콘만 활성화
    /// </summary>
    /// <param name="status">센서 상태</param>
    private void SetStatusIcon(ToxinStatus status)
    {
        statusGreen.SetActive(status == ToxinStatus.Green);   // 정상
        statusRed.SetActive(status == ToxinStatus.Red);       // 경보
        statusYellow.SetActive(status == ToxinStatus.Yellow); // 경계
        statusPurple.SetActive(status == ToxinStatus.Purple); // 설비이상
    }

    /// <summary>
    /// 시간축 설정 - 12시간 구간을 시간 라벨들에 분배
    /// </summary>
    /// <param name="dt">기준 시간 (현재 시간)</param>
    private void SetMins(DateTime dt)
    {
        DateTime startDt = dt.AddHours(-12); // 12시간 전부터

        // 시간 간격 계산 (전체 12시간을 라벨 개수로 나눔)
        var timeInterval = (dt - startDt).TotalMinutes / (this.hours.Count - 1);

        // 각 라벨에 시간 설정 (현재 시간부터 역순으로)
        for (int i = 0; i < this.hours.Count; i++)
        {
            var time = dt.AddMinutes(-(timeInterval * i));
            this.hours[i].text = time.ToString("HH:mm"); // 시:분 형태로 표시
        }
    }

    /// <summary>
    /// 세로축 설정 - 측정값 범위를 세로 라벨들에 분배
    /// </summary>
    /// <param name="max">데이터 최대값</param>
    private void SetVertical(float max)
    {
        // 세로축 간격 계산 (최대값+1을 라벨 개수로 나눔)
        var verticalInterval = ((max + 1) / (verticals.Count - 1));

        // 각 라벨에 측정값 범위 설정 (0부터 최대값까지)
        for (int i = 0; i < verticals.Count; i++)
        {
            var value = verticalInterval * i;
            verticals[i].text = Math.Round(value, 2).ToString(); // 소수점 2자리로 표시
        }
    }
}