using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// 관측소 모니터링 화면 - 센서 상태 표시 및 설정
/// </summary>
public class ObsMonitoring : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    Vector3 defaultPos;
    Button btnSetting;

    // UI 컴포넌트들
    TMP_Text lblStatus;
    Image imgSingalLamp;
    List<ObsMonitoringItem> allItems = new(), toxinItems = new(), qualityItems = new(), chemicalItems = new();

    int obsId; // 현재 선택된 관측소 ID

    #region [보드 상태 표시 관련 변수]
    [Header("보드 범례 이미지")]
    public Image lblToxin;      // 독성도 범례 이미지
    public Image lblQuality;    // 수질 범례 이미지  
    public Image lblChemical;   // 화학물질 범례 이미지

    private Color originalImageColor = Color.white;
    private bool[] lastBoardErrors = new bool[4]; // 성능 최적화용 - 변경된 보드만 업데이트
    #endregion

    // 상태별 색상 딕셔너리
    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    /// <summary>
    /// 상태별 색상 초기화 (정적 생성자)
    /// </summary>
    static ObsMonitoring()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#3EFF00"}, // 정상
            { ToxinStatus.Yellow,   "#FFF600"}, // 경계
            { ToxinStatus.Red,      "#FF0000"}, // 경보
            { ToxinStatus.Purple,   "#6C00E2"}, // 설비이상
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                statusColorDic[pair.Key] = color;
    }

    private void Start()
    {
        // 이벤트 등록
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);

        UiManager.Instance.Register(UiEventType.ChangeSensorList, OnChangeSensorList);
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine);
        UiManager.Instance.Register(UiEventType.ChangeSensorStatus, OnChangeSensorStatus);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
        UiManager.Instance.Register(UiEventType.CommitSensorUsing, OnCommitSensorUsing);

        // UI 컴포넌트 초기화
        Transform scrollContent = transform.Find("Scroll").Find("Content");

        toxinItems = scrollContent.Find("lstToxin").GetComponentsInChildren<ObsMonitoringItem>().ToList();
        qualityItems = scrollContent.Find("lstQuality").GetComponentsInChildren<ObsMonitoringItem>().ToList();
        chemicalItems = scrollContent.Find("lstChemical").GetComponentsInChildren<ObsMonitoringItem>().ToList();

        btnSetting = transform.Find("Btn_Setting").GetComponent<Button>();
        btnSetting.onClick.AddListener(OnClickSetting);

        lblStatus = transform.Find("lblStatus").GetComponent<TMP_Text>();
        imgSingalLamp = transform.Find("Icon_EventPanel_TitleCircle").Find("Icon_SignalLamp").GetComponent<Image>();

        // 원본 이미지 색상 저장 (복원용)
        if (lblToxin != null)
            originalImageColor = lblToxin.color;

        defaultPos = transform.position;

        // 전체 아이템 리스트 구성
        allItems.AddRange(toxinItems);
        allItems.AddRange(qualityItems);
        allItems.AddRange(chemicalItems);
        allItems.ForEach(item => item.gameObject.SetActive(false));
    }

    /// <summary>
    /// 알람 변경 시 - 관측소 상태 업데이트
    /// </summary>
    private void OnChangeAlarmList(object obj)
    {
        ToxinStatus status = modelProvider.GetObsStatus(obsId);
        SetTitleStatus(status);
    }

    /// <summary>
    /// 지역 화면으로 이동
    /// </summary>
    private void OnNavigateArea(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }

    /// <summary>
    /// 홈 화면으로 이동
    /// </summary>
    private void OnNavigateHome(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }

    /// <summary>
    /// 관측소 화면으로 이동 - 핵심 메서드
    /// </summary>
    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        this.obsId = obsId;
        SetAnimation(defaultPos + new Vector3(-575f, 0f), 1f); // 왼쪽으로 이동

        ToxinStatus status = modelProvider.GetObsStatus(obsId);
        SetTitleStatus(status);
    }

    /// <summary>
    /// 센서 사용 여부 변경 시
    /// </summary>
    private void OnCommitSensorUsing(object obj)
    {
        if (obj is not (int obsId, int boardId, int sensorId, bool isUsing)) return;
        if (this.obsId != obsId) return;

        // 보드별 아이템 리스트 선택
        List<ObsMonitoringItem> tItems = null;
        switch (boardId)
        {
            case 1: tItems = toxinItems; break;      // 독성도
            case 2: tItems = chemicalItems; break;  // 화학물질
            case 3: tItems = qualityItems; break;   // 수질
        }
        if (tItems is null) return;

        // 수질 센서는 역순 정렬되어 있어서 특별 처리
        ObsMonitoringItem tItem = null;
        if (boardId == 3) // 수질 센서만 역순 정렬되어 있음
        {
            List<ToxinData> qualityToxins = modelProvider.GetToxins()
                .Where(item => item.boardid == 3)
                .OrderByDescending(item => item.hnsid)
                .ToList();

            int itemIndex = qualityToxins.FindIndex(toxin => toxin.hnsid == sensorId);
            if (itemIndex >= 0 && itemIndex < tItems.Count)
                tItem = tItems[itemIndex];
        }
        else // 독성도, 화학물질은 순서대로
        {
            tItem = tItems[sensorId - 1];
        }

        if (tItem is null) return;

        // 센서 표시/숨김 처리
        tItem.gameObject.SetActive(isUsing);
        RectTransform rt = tItem.transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt); // 레이아웃 즉시 재계산
    }

    /// <summary>
    /// 센서 상태 변경 시 - 보드 이미지 색상 업데이트
    /// </summary>
    private void OnChangeSensorStatus(object obj)
    {
        allItems.ForEach(item => item.ResetToxinStatus());

        // 보드별 설비이상 상태를 이미지 색상으로 표시
        UpdateBoardImageStatusOptimized();
    }

    /// <summary>
    /// 트렌드 라인 변경 시
    /// </summary>
    private void OnChangeTrendLine(object obj) => allItems.ForEach(item => item.UpdateTrendLine());

    /// <summary>
    /// 센서 리스트 변경 시
    /// </summary>
    private void OnChangeSensorList(object obj) => ApplySensorList(modelProvider.GetToxins());

    /// <summary>
    /// 설정 버튼 클릭
    /// </summary>
    private void OnClickSetting()
    {
        UiManager.Instance.Invoke(UiEventType.PopupSetting, obsId);
    }

    /// <summary>
    /// 센서 리스트를 보드별로 분류하여 적용
    /// </summary>
    void ApplySensorList(List<ToxinData> toxins)
    {
        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;

        toxinBoard = toxins.Where(item => item.boardid == 1).ToList();
        chemicalBoard = toxins.Where(item => item.boardid == 2).ToList();
        qualityBoard = toxins.Where(item => item.boardid == 3)
                            .OrderByDescending(item => item.hnsid) // 수질은 역순
                            .ToList();

        ApplySensorListBoard(toxinBoard, toxinItems);
        ApplySensorListBoard(chemicalBoard, chemicalItems);
        ApplySensorListBoard(qualityBoard, qualityItems);
    }

    /// <summary>
    /// 특정 보드의 센서 리스트를 UI 아이템에 적용
    /// </summary>
    void ApplySensorListBoard(List<ToxinData> toxinsInBoard, List<ObsMonitoringItem> items)
    {
        if (items.Count == 0) throw new Exception("ObsMonitoring - ApplySensorListBoard : 발견한 요소의 수가 0입니다.");

        // 센서가 UI 아이템보다 많으면 동적으로 추가
        if (toxinsInBoard.Count > items.Count)
        {
            int needToAddCount = toxinsInBoard.Count - items.Count;
            Transform itemsParent = items[0].transform.parent;
            GameObject itemPrefab = items[0].gameObject;

            for (int i = 0; i < needToAddCount; i++)
            {
                GameObject obj = Instantiate(itemPrefab, itemsParent);
                ObsMonitoringItem newItem = obj.GetComponent<ObsMonitoringItem>();
                items.Add(newItem);
            }
        }

        // 센서 데이터를 UI 아이템에 설정
        for (int i = 0; i < toxinsInBoard.Count; i++)
        {
            ObsMonitoringItem item = items[i];
            ToxinData toxin = toxinsInBoard[i];

            if (i + 1 > toxinsInBoard.Count)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.SetToxinData(obsId, toxin);
        }

        // 레이아웃 즉시 재계산
        RectTransform rt = items[0].transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    /// <summary>
    /// 상태에 따른 타이틀 표시 (정상/경계/경보/설비이상)
    /// </summary>
    void SetTitleStatus(ToxinStatus status)
    {
        imgSingalLamp.color = statusColorDic[status];
        /*switch (status)
        {
            case ToxinStatus.Green: lblStatus.text = "정 상"; break;
            case ToxinStatus.Yellow: lblStatus.text = "경 계"; break;
            case ToxinStatus.Red: lblStatus.text = "경 보"; break;
            case ToxinStatus.Purple: lblStatus.text = "설비 이상"; break;
        }*/
        // 텍스트는 항상 고정
        lblStatus.text = "세부 모니터링 결과";
    }

    /// <summary>
    /// 화면 이동 애니메이션
    /// </summary>
    void SetAnimation(Vector3 toPos, float duration)
    {
        Vector3 fromPos = GetComponent<RectTransform>().position;
        DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() => {
            GetComponent<RectTransform>().position = fromPos;
        });
    }

    #region [보드 상태 표시 기능]
    /// <summary>
    /// 성능 최적화된 보드 이미지 상태 업데이트 - 변경된 보드만 업데이트
    /// </summary>
    private void UpdateBoardImageStatusOptimized()
    {
        if (obsId <= 0) return;

        bool[] currentErrors = {
            false, // 0번 미사용
            HasBoardError(1), // 독성도 보드
            HasBoardError(2), // 화학물질 보드
            HasBoardError(3)  // 수질 보드
        };

        // 상태가 변경된 보드만 업데이트 (성능 최적화)
        for (int i = 1; i <= 3; i++)
        {
            if (currentErrors[i] != lastBoardErrors[i])
            {
                switch (i)
                {
                    case 1: SetImageColorEffect(lblToxin, currentErrors[i]); break;
                    case 2: SetImageColorEffect(lblChemical, currentErrors[i]); break;
                    case 3: SetImageColorEffect(lblQuality, currentErrors[i]); break;
                }
                lastBoardErrors[i] = currentErrors[i];
            }
        }

        Debug.Log($"보드 이미지 상태 - 독성도:{currentErrors[1]}, 화학:{currentErrors[2]}, 수질:{currentErrors[3]}");
    }

    /// <summary>
    /// 특정 보드에 설비이상 알람이 있는지 확인
    /// </summary>
    private bool HasBoardError(int boardId)
    {
        var activeAlarms = modelProvider.GetActiveAlarms();

        return activeAlarms.Any(alarm =>
            alarm.obsId == obsId &&
            alarm.boardId == boardId &&
            alarm.status == 0); // 설비이상 = 0
    }

    /// <summary>
    /// 이미지 색상 효과 적용/해제 - 설비이상 시 보라색 깜빡임
    /// </summary>
    private void SetImageColorEffect(Image image, bool hasError)
    {
        if (image == null) return;

        if (hasError)
        {
            // 설비이상: 보라색으로 변경 + 깜빡임 효과
            image.DOKill();
            image.DOColor(statusColorDic[ToxinStatus.Purple], 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            // 정상: 원래 색상으로 복원
            image.DOKill();
            image.color = originalImageColor;
        }
    }
    #endregion

    /// <summary>
    /// 오브젝트 파괴 시 애니메이션 정리
    /// </summary>
    private void OnDestroy()
    {
        lblToxin?.DOKill();
        lblChemical?.DOKill();
        lblQuality?.DOKill();
    }
}