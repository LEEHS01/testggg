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

public class AreaListMonth : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;
    List<AreaListMonthItem> items = new();
    List<Image> imgRingCharts = new();
    TMP_Text titleText;
    private void Start()
    {
        var items = transform.Find("List_Panel").GetComponentsInChildren<AreaListMonthItem>();
        this.items.AddRange(items);
        
        var charts = transform.Find("Doughnut Chart").GetComponentsInChildren<Image>();
        imgRingCharts.AddRange(charts);

        titleText = transform.Find("Title Text (TMP)").GetComponentInChildren<TMP_Text>();

        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmMonthly, OnInitiate);
    }
    private void SetCurrentMonthTitle()
    {
        if (titleText != null)
        {
            DateTime now = DateTime.Now;
            string monthText = $"{now.Month}월 최다 알람 발생 TOP 5";
            titleText.text = monthText;
        }
    }

    private void OnInitiate(object obj)
    {
        SetCurrentMonthTitle();
        var alarmMonthlyList = modelProvider.GetAlarmMonthly();

        //DB에서 받은 데이터가 없는 경우, 시연용 데이터로 대체
        if (alarmMonthlyList.Count == 0)
            alarmMonthlyList = new() {
                (1,5),(2,3),(3,3),(4,2),(5,1),
            };

        //상위 5개 지역 5개를 선택
        alarmMonthlyList = alarmMonthlyList.OrderByDescending(item => item.count).ToList().GetRange(0, 5);

        //상위 5개 지역의 알람 총계를 산출
        int sum = Math.Max(alarmMonthlyList.Sum(item => item.count), 1);

        //AreaListMonthItem 업데이트
        for (int i = 0; i < items.Count; i++)
        {
            AreaListMonthItem item = items[i];

            if (i < alarmMonthlyList.Count)
            {
                (int, int) alarmYearly = alarmMonthlyList[i];
                AreaData area = modelProvider.GetArea(alarmYearly.Item1);
                float percent = (float)alarmYearly.Item2 / sum;
                item.SetAreaData(imgRingCharts[i].color, area.areaId, area.areaName, alarmYearly.Item2, percent);
            }
            else
            {
                item.SetAreaData(imgRingCharts[i].color, -1, "-", 0, 0);
            }

        }


        //RingChart 업데이트
        const float fillRatioMin = 0.01f; // 최소 fillAmount 값

        var duration = 1f;
        var rotation = fillRatioMin;

        for (int i = 0; i < items.Count; i++)
        {
            (int, int) alarmYearly = (i < alarmMonthlyList.Count)? alarmMonthlyList[i] : (0,0);

            float p = (float)alarmYearly.Item2 / sum;

            var setPercent = p < fillRatioMin ? fillRatioMin : p;
            imgRingCharts[i].DOFillAmount(setPercent, duration);
            imgRingCharts[i].transform.DOLocalRotate(new Vector3(0, 0, rotation), duration);

            rotation -= (360 * setPercent);
        }



    }
}