using Assets.Scripts.Manager;
using DG.Tweening;
using I18N.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class PageContainer : MonoBehaviour
{
    //페이지 제어
    public Page PageNow { get; private set; }
    public List<Page> pages { get; private set; } = new List<Page>();

    public Image imgBgr => GetComponent<Image>();

    private void Start()
    {
        pages = GetComponentsInChildren<Page>(true).ToList();
        Debug.Log($"PageContainer - Start: 페이지 개수 {pages.Count}개");

        new List<UiEventType> {
            UiEventType.NavigateHome,
            UiEventType.NavigateRegion,
            UiEventType.NavigateObs,
            UiEventType.NavigateCctv,
            UiEventType.NavigateHistory,
            UiEventType.NavigateSetting,
        }.ForEach(type => {
            Debug.Log($"PageContainer - Start: {type} 이벤트 등록");
            UiManager.Instance.Register(type, obj => OnNavigatePage(type,obj));
        });
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
    }

    private void OnInitiate(object obj)
    {
        pages.ForEach(page => page.Hide(UiEventType.Initiate, UiEventType.NavigateHome));
        if (pages.Count > 0)
        {
            PageNow = pages[0];
            PageNow.Show(UiEventType.Initiate, UiEventType.NavigateHome);
        }
    }

    private void OnNavigatePage(UiEventType type, object obj)
    {
        UiEventType from = PageNow != null ? PageNow.CallingEventType : UiEventType.Initiate;
        Debug.Log($"PageContainer - OnNavigatePage: {type}로 페이지 전환 요청 받음");
        pages.ForEach(page => 
        {
            Debug.Log($"PageContainer - OnNavigatePage: 처리중인 페이지 {page.name} (호출타입: {page.CallingEventType})");
            Action<UiEventType, UiEventType> func = page.CallingEventType != type ? page.Hide : page.Show;
            func(from, type);
            PageNow = page.CallingEventType == type ? page : PageNow;

        });



        Color fromColor = imgBgr.color;
        DOTween.ToAlpha(() => fromColor, x => fromColor = x, type == UiEventType.NavigateObs? 0f : 1f, 0.3f).OnUpdate(() =>
            imgBgr.color = fromColor
        );

    }
}
