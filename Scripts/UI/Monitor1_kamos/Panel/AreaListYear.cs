using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 연간 지역별 알람 발생 TOP5 리스트
/// </summary>
public class AreaListYear : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;
    List<AreaListYearItem> items = new();
    TMP_Text titleText;

    private void Start()
    {
        var items = transform.Find("List_Panel").GetComponentsInChildren<AreaListYearItem>();
        this.items.AddRange(items);
        titleText = transform.Find("Title Text (TMP)").GetComponentInChildren<TMP_Text>();

        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmYearly, OnInitiate);
    }

    /// <summary>
    /// 현재 연도로 제목 설정
    /// </summary>
    private void SetCurrentMonthTitle()
    {
        if (titleText != null)
        {
            DateTime now = DateTime.Now;
            string monthText = $"{now.Year}년 알람 발생 TOP5";
            titleText.text = monthText;
        }
    }

    /// <summary>
    /// 연간 알람 데이터 로드 및 TOP5 표시
    /// </summary>
    private void OnInitiate(object obj)
    {
        SetCurrentMonthTitle();
        var alarmYearlyList = modelProvider.GetAlarmYearly();

        // DB 데이터 없으면 시연용 더미 데이터 사용
        if (alarmYearlyList.Count == 0)
            alarmYearlyList = new() {
                (4,new(18,10,9,11)),  
                (5,new(15,9,6,9)),    
                (10,new(14,6,5,7)),   
                (9,new(13,5,3,4)),    
                (3,new(12,2,1,4))     
            };

        // TOP5 아이템들에 데이터 설정
        for (int i = 0; i < items.Count; i++)
        {
            AreaListYearItem item = items[i];

            if (i < alarmYearlyList.Count)
            {
                (int areaId, AlarmCount alarmCount) = alarmYearlyList[i];
                AreaData area = modelProvider.GetArea(areaId);
                item.SetAreaData(area.areaId, area.areaName, alarmCount);
            }
            else
            {
                // 데이터 없는 아이템은 빈값으로 설정
                item.SetAreaData(-1, "-", new(0, 0, 0, 0));
            }
        }
    }
}