using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 센서 단계별 정보 팝업 - 현재 센서 진행 단계에 맞는 팝업 표시
/// </summary>
public class PopupStep : MonoBehaviour
{
    int stepBefore = -1; // 이전 단계 추적 (중복 호출 방지)

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.ChangeSensorStep, OnChangeSensorStep);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateObs);
        SetPopupPanels(stepBefore);
    }

    /// <summary>
    /// 화면 이동시 팝업 초기화
    /// </summary>
    private void OnNavigateObs(object obj)
    {
        stepBefore = -1;
        SetPopupPanels(stepBefore); // 모든 팝업 숨김
    }

    /// <summary>
    /// 센서 단계 변경시만 팝업 업데이트 (이전과 다를때만)
    /// </summary>
    private void OnChangeSensorStep(object obj)
    {
        if (obj is not int step) return;
        if (stepBefore > 0 && stepBefore != step) SetPopupPanels(step); // 단계 변경시에만
        stepBefore = step;
    }

    /// <summary>
    /// 해당 단계 팝업만 활성화, 나머지는 비활성화
    /// </summary>
    void SetPopupPanels(int step)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(child.name == ("Popup_Info_Step_0" + step));
    }
}