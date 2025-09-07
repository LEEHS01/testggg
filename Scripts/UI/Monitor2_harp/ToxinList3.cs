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
/// 관측소 센서 실시간 상태를 원형 바(ArcBar)로 표시하는 화면
/// </summary>
internal class ToxinList3 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    public TMP_Text txtName; // 관측소 이름 표시

    // 센서 원형 바 리스트들
    List<ArcBar> allItems = new(), toxinItems = new(), qualityItems = new(), chemicalItems = new();

    int obsId = -1; // 현재 선택된 관측소 ID

    private void Start()
    {
        // 이벤트 등록
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);           // 관측소 선택 시
        UiManager.Instance.Register(UiEventType.ChangeSensorList, OnLoadSetting);     // 센서 리스트 변경 시
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnLoadRecentMeasure); // 트렌드 데이터 변경 시
        UiManager.Instance.Register(UiEventType.ChangeSensorStatus, OnLoadRecentMeasure); // 센서 상태 변경 시
        UiManager.Instance.Register(UiEventType.CommitSensorUsing, OnCommitSensorUsing); // 센서 사용/비사용 변경 시

        Transform scrollContent = transform.Find("Content");

        // 보드별 원형 바 아이템들 찾기
        toxinItems = scrollContent.Find("Grid_Toxin").GetComponentsInChildren<ArcBar>().ToList();           // 독성도 센서
        qualityItems = scrollContent.Find("Grid_Water_Quality").GetComponentsInChildren<ArcBar>().ToList(); // 수질 센서
        chemicalItems = scrollContent.Find("Grid_Chemicals").GetComponentsInChildren<ArcBar>().ToList();   // 화학물질 센서

        // 전체 아이템 리스트 구성
        allItems.AddRange(toxinItems);
        allItems.AddRange(qualityItems);
        allItems.AddRange(chemicalItems);

        // 디버깅: 각 보드별 아이템 개수 확인
        Debug.Log($"toxinItems : {toxinItems.Count}");
        Debug.Log($"qualityItems : {qualityItems.Count}");
        Debug.Log($"chemicalItems : {chemicalItems.Count}");
    }

    /// <summary>
    /// 관측소 선택 시 - 제목 텍스트 업데이트
    /// </summary>
    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        ObsData obs = modelProvider.GetObs(obsId);
        this.obsId = obsId;
        txtName.text = $"{obs.areaName} - {obs.obsName} 실시간 상태";
    }

    /// <summary>
    /// 최신 측정값 로드 - 모든 원형 바의 수치 업데이트
    /// </summary>
    private void OnLoadRecentMeasure(object obj)
    {
        Debug.Log("🔍 OnLoadRecentMeasure 호출됨");
        List<ToxinData> toxinDatas = modelProvider.GetToxins();

        if (toxinDatas.Count == 0) return;

        // 모든 원형 바의 수치와 상태 업데이트
        allItems.ForEach(bar => bar.SetAmount());
    }

    /// <summary>
    /// 센서 리스트 변경 시 - 센서 목록 재구성
    /// </summary>
    private void OnLoadSetting(object data)
    {
        List<ToxinData> toxinDatas = modelProvider.GetToxins();
        ApplySensorList(toxinDatas);
    }

    /// <summary>
    /// 센서 사용/비사용 변경 시 - 해당 센서 표시/숨김
    /// </summary>
    private void OnCommitSensorUsing(object obj)
    {
        if (obj is not (int obsId, int boardId, int sensorId, bool isUsing)) return;

        if (this.obsId != obsId) return; // 현재 관측소가 아니면 무시

        // 보드별 아이템 리스트 선택
        List<ArcBar> tItems = null;
        switch (boardId)
        {
            case 1: tItems = toxinItems; break;    // 독성도
            case 2: tItems = chemicalItems; break; // 화학물질
            case 3: tItems = qualityItems; break;  // 수질
        }
        if (tItems is null) return;

        // 해당 센서 아이템 찾기 (sensorId는 1부터 시작)
        ArcBar tItem = null;
        tItem = tItems[sensorId - 1];
        if (tItem is null) return;

        // 아이템 활성화/비활성화
        tItem.gameObject.SetActive(isUsing);

        // 레이아웃 즉시 갱신 (그리드 재정렬)
        RectTransform rt = tItem.transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    /// <summary>
    /// 센서 리스트를 보드별로 분류하여 적용
    /// </summary>
    void ApplySensorList(List<ToxinData> toxins)
    {
        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;

        // 보드별로 센서 데이터 분류
        toxinBoard = toxins.Where(item => item.boardid == 1).ToList();   // 독성도 보드
        chemicalBoard = toxins.Where(item => item.boardid == 2).ToList(); // 화학물질 보드
        qualityBoard = toxins.Where(item => item.boardid == 3).ToList();  // 수질 보드

        // 각 보드별로 센서 리스트 적용
        ApplySensorListBoard(toxinBoard, toxinItems);
        ApplySensorListBoard(chemicalBoard, chemicalItems);
        ApplySensorListBoard(qualityBoard, qualityItems);
    }

    /// <summary>
    /// 특정 보드의 센서 리스트를 UI 아이템에 적용
    /// </summary>
    void ApplySensorListBoard(List<ToxinData> toxinsInBoard, List<ArcBar> items)
    {
        if (items.Count == 0)
            throw new Exception("ObsMonitoring - ApplySensorListBoard : 발견한 요소의 수가 0입니다.");

        // 센서 개수가 UI 아이템보다 많으면 동적으로 아이템 추가
        if (toxinsInBoard.Count > items.Count)
        {
            int needToAddCount = toxinsInBoard.Count - items.Count;
            Transform itemsParent = items[0].transform.parent;
            GameObject itemPrefab = items[0].gameObject;

            // 부족한 만큼 아이템 생성
            for (int i = 0; i < needToAddCount; i++)
            {
                GameObject obj = Instantiate(itemPrefab, itemsParent);
                ArcBar newItem = obj.GetComponent<ArcBar>();
                items.Add(newItem);
            }
        }

        // 전체 아이템 순회하여 센서 데이터 설정
        for (int i = 0; i < items.Count; i++)
        {
            ArcBar item = items[i];

            // 센서 데이터 범위 내라면 활성화
            if (i < toxinsInBoard.Count)
            {
                ToxinData toxin = toxinsInBoard[i];
                item.SetValue(toxin); // 센서 데이터 설정 (내부에서 toxin.on에 따라 활성화 결정)
            }
            else
            {
                // 센서 데이터 범위를 벗어나면 비활성화
                item.gameObject.SetActive(false);
            }
        }

        // 레이아웃 즉시 갱신 (그리드 크기 재계산)
        RectTransform rt = items[0].transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}