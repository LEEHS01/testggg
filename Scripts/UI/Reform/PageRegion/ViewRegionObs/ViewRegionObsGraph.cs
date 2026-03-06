using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.UI.Reform.PageHome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageRegion
{
    public class ViewRegionObsGraph : MonoBehaviour
    {
        //system ref
        GameObject stickPrefab => Resources.Load<GameObject>("Reform/PageRegion/ViewGraphStick");
        GameObject legendItemPrefab => Resources.Load<GameObject>("Reform/PageRegion/ViewGraphLegendItem");
        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        
        
        //obj ref
        Transform legendContainer, stickMonthesContainer;
        Dictionary<int, List<Transform>> sticksByObs = new();

        TMP_Text txtTitle;
        Transform txtMonthsContainer, txtValueContainer;


        //staticLib
        List<Color> legendColors = new() {Color.red, Color.cyan, Color.green, Color.magenta, Color.yellow, Color.white, };

        //data
        List<ObservatoryInfo> obss;
        List<GroupInfo> groups;
        List<AlarmInfo> alarmsActivated;
        List<AlarmInfo> alarmWhole;


        private void Start()
        {
            transform.Find("txtTitle")?.TryGetComponent<TMP_Text>(out txtTitle);

            legendContainer = transform.Find("Panel_Frame").Find("histogramLegend");
            stickMonthesContainer = transform.Find("Panel_Histogram");
            txtMonthsContainer = transform.Find("Panel_Frame").Find("Panel_Month");
            txtValueContainer = transform.Find("Panel_Frame").Find("Panel_Count");


            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
        }



        void OnInitiate(object obj)
        {
            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
            alarmsActivated = modelProvider.GetAlarmsActivated();
            alarmWhole = modelProvider.GetAlarmsWhole();


            for (int i = 0; i < txtMonthsContainer.childCount; i++) 
                txtMonthsContainer.GetChild(i).GetComponent<TMP_Text>().text = $"{(DateTime.Now.Month+i)%12+1}월";
        }

        void OnSelectObs(object obj) 
        {
            if (obj is not int obsIdx) return;
                txtTitle.text = obsIdx.ToString();

            //전체 삭제
            foreach (RectTransform item in legendContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform itemCon in stickMonthesContainer)
                foreach (RectTransform item in itemCon) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in legendContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform itemCon in stickMonthesContainer)
                foreach (RectTransform item in itemCon) { item.parent = null; Destroy(item.gameObject); }
            sticksByObs.Clear();

            //관측소 선택 해제 시
            if (obsIdx == -1)
            {
                txtTitle.text = "-- 지역 추세";

                //전체 삭제
                foreach (RectTransform item in legendContainer) { item.parent = null; Destroy(item.gameObject); }
                foreach (RectTransform itemCon in stickMonthesContainer)
                    foreach (RectTransform item in itemCon) { item.parent = null; Destroy(item.gameObject); }
                sticksByObs.Clear();

                return;
            }

            //대상 관측소, 그룹 확인
            ObservatoryInfo selObs = obss.Find(obs => obs.obsIdx == obsIdx);
            GroupInfo group = groups.Find(group => group.groupIdx == selObs.groupIdx);

            //그룹이 있는 관측소 선택시
            if (group != null)
            {
                txtTitle.text = $"'{group.groupName}' 지역 관측소 목록";

                List<ObservatoryInfo> obssInGroup = obss.Where(obs => obs.groupIdx == group.groupIdx).ToList();
                List<AlarmInfo> alarmsInGroup = alarmWhole.Where(alarm => obssInGroup.Find(obs => obs.obsIdx == alarm.obsIdx) != null).ToList();

                //string oo = "";
                //obssInGroup.ForEach(obs => oo += $"{obs.obsIdx}|");
                //Debug.Log( oo );


                for (int i = 0; i < obssInGroup.Count; i++) 
                {
                    var obs = obssInGroup[i];
                    var color = legendColors[Math.Min(i, legendColors.Count - 1)];
                    //List<AlarmInfo> alarmsActivatedInObs = alarmsActivatedInGroup.Where(alarm => alarm.obsIdx == obs.obsIdx).ToList();

                    //범례 할당
                    {
                        GameObject instant = Instantiate(legendItemPrefab, legendContainer);
                        instant.GetComponent<Image>().color = color;
                        instant.transform.Find("txtObsName").GetComponent<TMP_Text>().text = obs.nameText;
                    }

                    //막대 그래프 할당
                    sticksByObs[obs.obsIdx] = new();
                    foreach (Transform itemCon in stickMonthesContainer)
                    {
                        GameObject instant = Instantiate(stickPrefab, itemCon);
                        sticksByObs[obs.obsIdx].Add(instant.transform);
                        instant.GetComponent<Image>().color = color;
                    }
                }

                SettingUpGraph(obssInGroup, alarmsInGroup);

            }
            //그룹이 없는 관측소 선택시
            else
            {
                txtTitle.text = $"'{selObs.nameText}' 관측소";

                int i = 0;
                var obs = selObs;
                var color = legendColors[Math.Min(i, legendColors.Count - 1)];
                List<AlarmInfo> alarmsSelObs = alarmWhole.Where(alarm => alarm.obsIdx == obs.obsIdx).ToList();

                //범례 할당
                {
                    GameObject instant = Instantiate(legendItemPrefab, legendContainer);
                    instant.GetComponent<Image>().color = color;
                    instant.transform.Find("txtObsName").GetComponent<TMP_Text>().text = obs.nameText;
                }

                //막대 그래프 할당
                sticksByObs[obs.obsIdx] = new();
                foreach (Transform itemCon in stickMonthesContainer)
                {
                    GameObject instant = Instantiate(stickPrefab, itemCon);
                    sticksByObs[obs.obsIdx].Add(instant.transform);
                    instant.GetComponent<Image>().color = color;
                }


                SettingUpGraph(new() { obs }, alarmsSelObs);
            }


        }

        void SettingUpGraph(List<ObservatoryInfo> obss, List<AlarmInfo> alarms) 
        {
            int maxVal = 0;

            Dictionary<ObservatoryInfo, Dictionary<int, int>> alarmMap = new();
            //모든 값을 0으로 초기화
            obss.ForEach(obs => {
                alarmMap.Add(obs, new());
                for (int i = 0; i < 12; i++)
                    alarmMap[obs][i] = 0;
            });

            //알람 매핑
            foreach (ObservatoryInfo obs in obss)
            {
                if (!sticksByObs.ContainsKey(obs.obsIdx)) throw new Exception("sticksByObs has not key! : " + obs.obsIdx);
                if (sticksByObs[obs.obsIdx].Count != 12) throw new Exception("sticksByObs has invalid sticks! : " + obs.obsIdx + " -> " + sticksByObs[obs.obsIdx].Count);

                alarms.ForEach(alarm =>
                {
                    DateTime occuredT = alarm.occured.timestamp;
                    TimeSpan tSpan = DateTime.Now - occuredT;

                    //표시 범위 내라면
                    if (tSpan.Days <= 365)
                    {
                        int monthDif = -999;
                        //작년이라면
                        if (occuredT.Year != DateTime.Now.Year)
                        {
                            monthDif = occuredT.Month - (DateTime.Now.Month + 12);
                        }
                        else
                        {
                            monthDif = occuredT.Month - DateTime.Now.Month;
                        }

                        //monthDif 범위 -11 ~ 0
                        monthDif += 11;
                        //monthDif 범위 0 ~ 11

                        ObservatoryInfo obs = obss.Find(obs => obs.obsIdx == alarm.obsIdx);
                        alarmMap[obs][monthDif] += 1;

                        if (maxVal < alarmMap[obs][monthDif])
                            maxVal = alarmMap[obs][monthDif];
                    }
                });

                maxVal = Math.Max(1, maxVal);
            }

            // 스틱 조작
            foreach (ObservatoryInfo obs in obss)
            { 
                Vector2 parentSizeDelta = sticksByObs[obs.obsIdx][0].parent.GetComponent<RectTransform>().sizeDelta;
                for (int i = 0; i < sticksByObs[obs.obsIdx].Count; i++)
                {
                    Transform item = sticksByObs[obs.obsIdx][i];

                    RectTransform stickRect = item.GetComponent<RectTransform>();
                    float ratio = (float)alarmMap[obs][i]/ maxVal;

                    stickRect.sizeDelta = new(stickRect.sizeDelta.x, parentSizeDelta.y * ratio);

                }
            }

            //범주 지정
            for (int i = 0; i < txtValueContainer.childCount; i++)
                txtValueContainer.GetChild(i).GetComponent<TMP_Text>().text = $"{(int)((float)i/(txtValueContainer.childCount-1) * maxVal)}";


        }




    }
}
