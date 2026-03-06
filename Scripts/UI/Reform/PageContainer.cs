using Assets.Scripts.Manager;
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
        pages.ForEach(page => page.Hide());
        if (pages.Count > 0)
        {
            PageNow = pages[0];
            PageNow.Show();
        }
    }

    private void OnNavigatePage(UiEventType type, object obj)
    {
        Debug.Log($"PageContainer - OnNavigatePage: {type}로 페이지 전환 요청 받음");
        pages.ForEach(page => 
        {
            Debug.Log($"PageContainer - OnNavigatePage: 처리중인 페이지 {page.name} (호출타입: {page.CallingEventType})");
            Action func = page.CallingEventType != type ? page.Hide : page.Show;
            func();
        });
    }
}
