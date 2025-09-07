using DG.Tweening;
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
/// 지역 타입별 리스트 - 해양시설 또는 발전소 타입별로 관측소 현황 표시
/// </summary>
public class AreaListType : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    #region [Inspector 설정값]
    public AreaData.AreaType areaType;  // 해양시설(Ocean) 또는 발전소(Nuclear) 
    public Sprite nuclearSprite;        // 발전소 아이콘
    public Sprite oceanSprite;          // 해양시설 아이콘
    #endregion

    #region [UI 컴포넌트]
    Image imgAreaType;                  // 타입별 아이콘 이미지
    TMP_Text lblTitle;                  // 타입별 제목 텍스트
    List<AreaListTypeItem> items = new(); // 지역 아이템들
    Vector3 defaultPos;                 // 기본 위치
    #endregion

    /// <summary>
    /// Inspector에서 값 변경시 자동 업데이트 (에디터 전용)
    /// </summary>
    private void OnValidate()
    {
        imgAreaType = transform.Find("icon").GetComponent<Image>();
        imgAreaType.sprite = areaType == AreaData.AreaType.Ocean ? oceanSprite : nuclearSprite;

        lblTitle = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        lblTitle.text = areaType == AreaData.AreaType.Ocean ?
            "해양산업시설 방류구 주변 관측소 현황" :
            "발전소 방류구 주변 관측소 현황"; 
    }

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnInitiate);

        imgAreaType = transform.Find("icon").GetComponent<Image>();
        lblTitle = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        items = transform.Find("List_Panel").GetComponentsInChildren<AreaListTypeItem>().ToList();
        defaultPos = transform.position;
    }

    /// <summary>
    /// 해당 타입의 지역들 데이터 로드 및 표시
    /// </summary>
    private void OnInitiate(object obj)
    {
        // 현재 타입(해양/발전소)에 해당하는 지역들만 필터링
        List<AreaData> areasInType = modelProvider.GetAreas()
            .Where(area => area.areaType == this.areaType)
            .ToList();

        // 각 지역의 알람 현황 데이터를 아이템에 설정
        for (int i = 0; i < items.Count; i++)
        {
            AreaListTypeItem item = items[i];
            AreaData area = areasInType[i];
            AlarmCount alarmCount = modelProvider.GetObsStatusCountByAreaId(area.areaId);
            item.SetAreaData(area.areaId, area.areaName, alarmCount);
        }
    }

    /// <summary>
    /// 지역 화면 진입
    /// </summary>
    private void OnNavigateArea(object obj)
    {
        SetAnimation(defaultPos + new Vector3(800, 0f), 1f);
    }

    /// <summary>
    /// 홈 화면 복귀
    /// </summary>
    private void OnNavigateHome(object obj)
    {
        SetAnimation(defaultPos, 1f);
    }

    /// <summary>
    /// 관측소 화면 진입
    /// </summary>
    private void OnNavigateObs(object obj)
    {
        SetAnimation(defaultPos + new Vector3(800, 0f), 1f);
    }

    /// <summary>
    /// 슬라이드 애니메이션
    /// </summary>
    void SetAnimation(Vector3 toPos, float duration)
    {
        Vector3 fromPos = GetComponent<RectTransform>().position;
        DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() => {
            GetComponent<RectTransform>().position = fromPos;
        });
    }
}