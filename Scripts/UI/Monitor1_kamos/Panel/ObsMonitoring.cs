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

public class ObsMonitoring : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    Vector3 defaultPos;
    Button btnSetting;

    TMP_Text lblStatus;
    Image imgSingalLamp;
    List<ObsMonitoringItem> allItems = new(), toxinItems = new(), qualityItems = new(), chemicalItems = new();

    int obsId;

    #region [보드 상태 표시 관련 변수]
    [Header("보드 범례 이미지")]
    public Image lblToxin;      // 독성도 범례 이미지
    public Image lblQuality;    // 수질 범례 이미지  
    public Image lblChemical;   // 화학물질 범례 이미지

    private Color originalImageColor = Color.white;
    private bool[] lastBoardErrors = new bool[4]; // 성능 최적화용
    #endregion

    static Dictionary<ToxinStatus, Color> statusColorDic = new();

    static ObsMonitoring()
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

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);

        UiManager.Instance.Register(UiEventType.ChangeSensorList, OnChangeSensorList);
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine);
        UiManager.Instance.Register(UiEventType.ChangeSensorStatus, OnChangeSensorStatus);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
        UiManager.Instance.Register(UiEventType.CommitSensorUsing, OnCommitSensorUsing);

        Transform scrollContent = transform.Find("Scroll").Find("Content");

        toxinItems = scrollContent.Find("lstToxin").GetComponentsInChildren<ObsMonitoringItem>().ToList();
        qualityItems = scrollContent.Find("lstQuality").GetComponentsInChildren<ObsMonitoringItem>().ToList();
        chemicalItems = scrollContent.Find("lstChemical").GetComponentsInChildren<ObsMonitoringItem>().ToList();

        btnSetting = transform.Find("Btn_Setting").GetComponent<Button>();
        btnSetting.onClick.AddListener(OnClickSetting);

        lblStatus = transform.Find("lblStatus").GetComponent<TMP_Text>();
        imgSingalLamp = transform.Find("Icon_EventPanel_TitleCircle").Find("Icon_SignalLamp").GetComponent<Image>();

        // ✅ 원본 이미지 색상 저장
        if (lblToxin != null)
            originalImageColor = lblToxin.color;

        defaultPos = transform.position;

        allItems.AddRange(toxinItems);
        allItems.AddRange(qualityItems);
        allItems.AddRange(chemicalItems);
        allItems.ForEach(item => item.gameObject.SetActive(false));
    }

    private void OnChangeAlarmList(object obj)
    {
        ToxinStatus status = modelProvider.GetObsStatus(obsId);
        SetTitleStatus(status);
    }

    private void OnNavigateArea(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }

    private void OnNavigateHome(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }

    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        this.obsId = obsId;
        SetAnimation(defaultPos + new Vector3(-575f, 0f), 1f);

        ToxinStatus status = modelProvider.GetObsStatus(obsId);
        SetTitleStatus(status);
    }

    private void OnCommitSensorUsing(object obj)
    {
        if (obj is not (int obsId, int boardId, int sensorId, bool isUsing)) return;

        if (this.obsId != obsId) return;

        List<ObsMonitoringItem> tItems = null;
        switch (boardId)
        {
            case 1: tItems = toxinItems; break;
            case 2: tItems = chemicalItems; break;
            case 3: tItems = qualityItems; break;
        }
        if (tItems is null) return;

        ObsMonitoringItem tItem = null;
        tItem = tItems[sensorId - 1];
        if (tItem is null) return;

        tItem.gameObject.SetActive(isUsing);
        RectTransform rt = tItem.transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private void OnChangeSensorStatus(object obj)
    {
        allItems.ForEach(item => item.ResetToxinStatus());

        // 이미지 색상으로 보드 상태 표시
        UpdateBoardImageStatusOptimized();
    }

    private void OnChangeTrendLine(object obj) => allItems.ForEach(item => item.UpdateTrendLine());

    private void OnChangeSensorList(object obj) => ApplySensorList(modelProvider.GetToxins());

    private void OnClickSetting()
    {
        UiManager.Instance.Invoke(UiEventType.PopupSetting, obsId);
    }

    void ApplySensorList(List<ToxinData> toxins)
    {
        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;

        toxinBoard = toxins.Where(item => item.boardid == 1).ToList();
        chemicalBoard = toxins.Where(item => item.boardid == 2).ToList();
        qualityBoard = toxins.Where(item => item.boardid == 3)
                            .OrderByDescending(item => item.hnsid)
                            .ToList();

        ApplySensorListBoard(toxinBoard, toxinItems);
        ApplySensorListBoard(chemicalBoard, chemicalItems);
        ApplySensorListBoard(qualityBoard, qualityItems);
    }

    void ApplySensorListBoard(List<ToxinData> toxinsInBoard, List<ObsMonitoringItem> items)
    {
        if (items.Count == 0) throw new Exception("ObsMonitoring - ApplySensorListBoard : 발견한 요소의 수가 0입니다.");

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

        RectTransform rt = items[0].transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void SetTitleStatus(ToxinStatus status)
    {
        imgSingalLamp.color = statusColorDic[status];
        switch (status)
        {
            case ToxinStatus.Green: lblStatus.text = "정 상"; break;
            case ToxinStatus.Yellow: lblStatus.text = "경 계"; break;
            case ToxinStatus.Red: lblStatus.text = "경 보"; break;
            case ToxinStatus.Purple: lblStatus.text = "설비 이상"; break;
        }
    }

    void SetAnimation(Vector3 toPos, float duration)
    {
        Vector3 fromPos = GetComponent<RectTransform>().position;
        DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() => {
            GetComponent<RectTransform>().position = fromPos;
        });
    }

    #region [보드 상태 표시 기능]
    /// <summary>
    /// 성능 최적화된 보드 이미지 상태 업데이트
    /// </summary>
    private void UpdateBoardImageStatusOptimized()
    {
        if (obsId <= 0) return;

        bool[] currentErrors = {
            false, // 0번 미사용
            HasBoardError(1), // 독성도
            HasBoardError(2), // 화학물질
            HasBoardError(3)  // 수질
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

        Debug.Log($"🔍 보드 이미지 상태 - 독성도:{currentErrors[1]}, 화학:{currentErrors[2]}, 수질:{currentErrors[3]}");
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
            alarm.status == 0); // 설비이상
    }

    /// <summary>
    /// 이미지 색상 효과 적용/해제
    /// </summary>
    private void SetImageColorEffect(Image image, bool hasError)
    {
        if (image == null) return;

        if (hasError)
        {
            // 🟣 이미지를 보라색으로 변경 + 깜빡임
            image.DOKill();
            image.DOColor(statusColorDic[ToxinStatus.Purple], 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            // 원래 색상으로 복원
            image.DOKill();
            image.color = originalImageColor;
        }
    }
    #endregion

    private void OnDestroy()
    {
        lblToxin?.DOKill();
        lblChemical?.DOKill();
        lblQuality?.DOKill();
    }
}