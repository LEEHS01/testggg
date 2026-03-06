using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewRegionAlarmMonthly : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        TMP_Text titleText;
        List<Image> imgRingCharts = new();
        List<AreaListMonthItem> items = new();


        private void Start()
        {
            var items = transform.Find("List_Panel").GetComponentsInChildren<AreaListMonthItem>();
            this.items.AddRange(items);

            var charts = transform.Find("Doughnut Chart").GetComponentsInChildren<Image>();
            imgRingCharts.AddRange(charts);

            titleText = transform.Find("Title Text (TMP)").GetComponentInChildren<TMP_Text>();

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarm);
        }


        private void OnChangeAlarm(object obj)
        {
            //throw new NotImplementedException();
        }

        private void OnInitiate(object obj)
        {
            if (titleText != null)
            {
                DateTime now = DateTime.Now;
                string monthText = $"알람 발생 건수({now.Month}월)";
                titleText.text = monthText;
            }

            List<ObservatoryInfo> obss = modelProvider.GetObss();
            List<GroupInfo> groups = modelProvider.GetGroups();
            List<AlarmInfo> alarms = modelProvider.GetAlarmsWhole();

            //우선 그룹에 알맞는 관측소 매핑
            List<(GroupInfo groupInfo, List<ObservatoryInfo> obsList)> groupedObsList;
            groupedObsList = (List<(GroupInfo, List<ObservatoryInfo>)>)groups.Select(
                groupInfo => {
                    var list = obss.Where(obs => groupInfo.groupIdx == obs.groupIdx).ToList();
                    return (groupInfo, list);
                }).ToList();

            //당월의 알람만 타겟
            alarms = alarms.Where(alarm => alarm.occured.timestamp.Month == DateTime.Now.Month).ToList();

            //카운팅작업
            List<(int regionIdx, int count)> alarmMonthlyList = new();
            groupedObsList.ForEach(groupedObs => {
                int groupIdx = groupedObs.groupInfo.groupIdx;
                int alarmCount = alarms.Where(alarm => groupedObs.obsList.Select(obs => obs.obsIdx).Contains(alarm.obsIdx)).Count();

                alarmMonthlyList.Add((groupIdx, alarmCount));
            });

            //DB에서 받은 데이터가 없는 경우, 시연용 데이터로 대체
            if (alarmMonthlyList.Count == 0)
                alarmMonthlyList = new() {
                (1,5),(2,3),(3,3),(4,2),(5,1),
            };

            //상위 5개 지역 5개를 선택23.0783348
            alarmMonthlyList = alarmMonthlyList.OrderByDescending(item => item.count).ToList().GetRange(0, 5);

            //상위 5개 지역의 알람 총계를 산출
            int sum = Math.Max(alarmMonthlyList.Sum(item => item.count), 1);

            //AreaListMonthItem 업데이트
            for (int i = 0; i < items.Count; i++)
            {
                AreaListMonthItem item = items[i];

                if (i < alarmMonthlyList.Count)
                {
                    (int, int) alarmMonthly = alarmMonthlyList[i];
                    GroupInfo group = groups.Find(group => group.groupIdx == alarmMonthly.Item1);
                    float percent = (float)alarmMonthly.Item2 / sum;
                    int obsCount = groupedObsList.Find(groupedObs => groupedObs.groupInfo == group).obsList.Count;
                    item.SetAreaData(imgRingCharts[i].color, group.groupIdx, group.groupName, obsCount, alarmMonthly.Item2, percent);
                }
                else
                {
                    item.SetAreaData(imgRingCharts[i].color, -1,"-", -1, 0, 0);
                }

            }


            //RingChart 업데이트
            const float fillRatioMin = 0.01f; // 최소 fillAmount 값

            var duration = 1f;
            var rotation = fillRatioMin;

            for (int i = 0; i < items.Count; i++)
            {
                (int, int) alarmYearly = (i < alarmMonthlyList.Count) ? alarmMonthlyList[i] : (0, 0);

                float p = (float)alarmYearly.Item2 / sum;

                var setPercent = p < fillRatioMin ? fillRatioMin : p;
                imgRingCharts[i].DOFillAmount(setPercent, duration);
                imgRingCharts[i].transform.DOLocalRotate(new Vector3(0, 0, rotation), duration);

                rotation -= (360 * setPercent);
            }

        }

    }
}