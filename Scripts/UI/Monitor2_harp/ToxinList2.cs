using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 알람 선택 시 해당 시점의 센서 상태를 막대 바(ToxinBar2)로 표시하는 화면
/// 과거 알람 발생 시점의 센서 데이터 조회용
/// </summary>
internal class ToxinList2 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private LogData log;                      // 선택된 알람 로그 데이터
    private List<ToxinBar2> bars = new();     // 센서 상태 막대 바 목록

    [Header("UI 컴포넌트")]
    public RectTransform scrollContainer;     // 스크롤 컨테이너 (동적 크기 조정용)
    public Transform barParent;               // Bar가 추가될 부모 객체
    public TMP_Text txtName;                  // 관측소 이름 텍스트
    public SetTime txtTime;                   // 알람 발생 시간 텍스트 1
    public SetTime txtTime2;                  // 알람 발생 시간 텍스트 2 (중복 표시용)

    int obsId;          // 현재 관측소 ID
    Vector3 defaultPos; // 기본 위치 (애니메이션용)

    void Start()
    {
        // 이벤트 등록
        UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectLog);           // 알람 선택 시
        UiManager.Instance.Register(UiEventType.ChangeAlarmSensorList, OnLoadAlarmData); // 알람 센서 리스트 변경 시

        // 기존 ToxinBar2 컴포넌트들 수집
        var barComps = scrollContainer.GetComponentsInChildren<ToxinBar2>();
        bars.AddRange(barComps);

        // 초기에는 모든 바 비활성화
        bars.ForEach(bar => bar.gameObject.SetActive(false));

        defaultPos = transform.position;
        gameObject.SetActive(false); // 초기에는 화면 숨김
    }

    /// <summary>
    /// 알람 선택 시 - 해당 알람 정보 표시 및 화면 활성화
    /// </summary>
    private void OnSelectLog(object data)
    {
        if (data is not int logIdx) return;

        // 선택된 알람 데이터 로드
        log = modelProvider.GetAlarm(logIdx);
        this.obsId = log.obsId;

        // UI 텍스트 업데이트
        txtName.text = $"{log.areaName} - {log.obsName}";
        txtTime.SetText(log.time);        // 알람 발생 시간 설정
        txtTime2.SetText(log.time);       // 중복 시간 표시

        // 화면 위치 초기화 및 활성화
        transform.position = defaultPos;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 알람 센서 데이터 로드 - 해당 시점의 모든 센서 상태 표시
    /// </summary>
    private void OnLoadAlarmData(object obj)
    {
        // 알람 발생 시점의 센서 데이터 가져오기
        List<ToxinData> toxinDatas = modelProvider.GetToxinsInLog();

        // 보드별로 센서 데이터 분류
        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;
        toxinBoard = toxinDatas.Where(item => item.boardid == 1).ToList();   // 독성도 보드
        chemicalBoard = toxinDatas.Where(item => item.boardid == 2).ToList(); // 화학물질 보드
        qualityBoard = toxinDatas.Where(item => item.boardid == 3).ToList();  // 수질 보드

        // 표시 순서: 독성도 → 수질 → 화학물질
        List<ToxinData> sortedToxins = new();
        sortedToxins.AddRange(toxinBoard);
        sortedToxins.AddRange(qualityBoard);
        sortedToxins.AddRange(chemicalBoard);

        float scrollHeight = 0; // 스크롤 컨테이너 높이 계산용

        // 각 센서 데이터를 막대 바에 적용
        for (int i = 0; i < sortedToxins.Count; i++)
        {
            // 바 개수보다 센서가 많으면 경고 후 건너뛰기
            if (i >= bars.Count)
            {
                Debug.LogWarning($"[ToxinList2] Bar index {i} is out of range");
                continue;
            }

            ToxinData toxin = sortedToxins[i];
            ToxinBar2 bar = bars[i];

            // 센서 데이터를 바에 설정 (관측소ID, 센서데이터, 상태)
            bar.SetToxinData(obsId, toxin, toxin.status);

            // 센서가 활성화된 경우만 표시
            bar.gameObject.SetActive(toxin.on);

            // 활성화된 바의 높이를 스크롤 높이에 누적
            if (toxin.on)
                scrollHeight += bar.GetComponent<RectTransform>().rect.height;
        }

        // 스크롤 컨테이너 크기 동적 조정 (내용물 높이 + 여백 76)
        RectTransform rect = this.scrollContainer.GetComponent<RectTransform>();
        rect.DOSizeDelta(new Vector2(rect.rect.width, scrollHeight + 76), 0);

        // 레이아웃 즉시 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollContainer);
    }
}