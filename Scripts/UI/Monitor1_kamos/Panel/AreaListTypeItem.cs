using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 지역 타입 리스트의 개별 아이템 - 지역명과 상태별 관측소 개수 표시
/// </summary>
public class AreaListTypeItem : MonoBehaviour
{
    public int areaId;      // Inspector 설정용 지역 ID
    public string areaName; // Inspector 설정용 지역명

    TMP_Text lblAreaName; // 지역명 텍스트
    Dictionary<ToxinStatus, TMP_Text> lblAlarmCounts; // 상태별 관측소 개수 텍스트들
    Button btnNavigateArea; // 지역 이동 버튼

    /// <summary>
    /// Inspector에서 areaName 변경시 즉시 반영
    /// </summary>
    private void OnValidate()
    {
        lblAreaName = transform.Find("TitleName_Button").GetComponentInChildren<TMP_Text>();
        lblAreaName.text = areaName;
    }

    private void Start()
    {
        lblAreaName = transform.Find("TitleName_Button").GetComponentInChildren<TMP_Text>();

        // 상태별 신호등 텍스트들을 딕셔너리로 매핑
        var signalLamps = transform.Find("SignalLamps");
        lblAlarmCounts = new() {
            {ToxinStatus.Green  , signalLamps.Find("SignalLamp_Green").GetComponentInChildren<TMP_Text>() },   // 정상
            {ToxinStatus.Yellow , signalLamps.Find("SignalLamp_Yellow").GetComponentInChildren<TMP_Text>() }, // 경계
            {ToxinStatus.Red    , signalLamps.Find("SignalLamp_Red").GetComponentInChildren<TMP_Text>() },    // 경보
            {ToxinStatus.Purple , signalLamps.Find("SignalLamp_Purple").GetComponentInChildren<TMP_Text>() }, // 설비이상
        };

        btnNavigateArea = GetComponent<Button>();
        btnNavigateArea.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 지역 화면으로 이동
    /// </summary>
    private void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateArea, areaId);
    }

    /// <summary>
    /// AlarmCount 구조체로 데이터 설정
    /// </summary>
    public void SetAreaData(int areaId, string areaName, AlarmCount alarmCount)
        => SetAreaData(areaId, areaName, alarmCount.green, alarmCount.yellow, alarmCount.red, alarmCount.purple);

    /// <summary>
    /// Dictionary로 상태별 관측소 개수 설정
    /// </summary>
    public void SetAreaData(int areaId, string areaName, Dictionary<ToxinStatus, int> obsStatus)
    {
        this.areaId = areaId;
        lblAreaName.text = areaName;

        // 각 상태별 관측소 개수를 해당 텍스트에 표시
        foreach (var pair in obsStatus)
            lblAlarmCounts[pair.Key].text = pair.Value.ToString();
    }

    /// <summary>
    /// 개별 파라미터로 상태별 관측소 개수 설정 (내부적으로 Dictionary 생성)
    /// </summary>
    public void SetAreaData(int areaId, string areaName, int greenObs, int yellowObs, int redObs, int purpleObs)
        => SetAreaData(areaId, areaName, new Dictionary<ToxinStatus, int>() {
            { ToxinStatus.Green, greenObs },   
            { ToxinStatus.Yellow, yellowObs }, 
            { ToxinStatus.Red, redObs },       
            { ToxinStatus.Purple, purpleObs }, 
        });
}