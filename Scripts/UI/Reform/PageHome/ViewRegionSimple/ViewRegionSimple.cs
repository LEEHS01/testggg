using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewRegionSimple : MonoBehaviour
    {
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageHome/ViewRegionSimpleItem");

        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiated);
        }

        void OnInitiated(object obj) 
        {
            List<ObservatoryInfo> obss = modelProvider.GetObss();
            List<GroupInfo> groups = modelProvider.GetGroups();

            //유형-지역-관측소 구조
            Dictionary<GroupInfo.GroupType, Dictionary<GroupInfo, List<ObservatoryInfo>>> groupTypeObsMap = new() {
                { GroupInfo.GroupType.GENERAL,  new Dictionary<GroupInfo, List<ObservatoryInfo>>(){ } },
                { GroupInfo.GroupType.OCEAN,    new Dictionary<GroupInfo, List<ObservatoryInfo>>(){ } },
                { GroupInfo.GroupType.NUCLEAR,  new Dictionary<GroupInfo, List<ObservatoryInfo>>(){ } },
            };

            //우선 그룹에 알맞는 관측소 매핑
            List<(GroupInfo groupInfo, List<ObservatoryInfo> obsList)> groupedObsList;
            groupedObsList = (List<(GroupInfo, List<ObservatoryInfo>)>)groups.Select(
                groupInfo => {
                    var list = obss.Where(obs => groupInfo.groupIdx == obs.groupIdx).ToList();
                    return (groupInfo, list);
                }).ToList();

            //각 그룹을 유형에 따라 자료구조에 적재
            groupedObsList.ForEach(kvp => 
                groupTypeObsMap[kvp.groupInfo.groupType].Add(kvp.groupInfo, kvp.obsList)
            );

            //generator = nuclear;
            //region = area = group;
            Transform itemContainer, generalSplitter, oceanSplitter, generatorSplitter;
            itemContainer = transform.Find("Scroll View").Find("Viewport").Find("List_Panel");
            generalSplitter = itemContainer.Find("ListSplitterGeneral");
            oceanSplitter = itemContainer.Find("ListSplitterOcean");
            generatorSplitter = itemContainer.Find("ListSplitterGenerator");


            groupTypeObsMap[GroupInfo.GroupType.GENERAL].AsEnumerable().Reverse().ToList().ForEach(kvp => {
                GameObject instant = Instantiate(itemPrefab, itemContainer.transform);
                instant.GetComponent<ViewRegionSimpleItem>().SetValue(kvp.Key, kvp.Value);
                instant.transform.SetSiblingIndex(generalSplitter.GetSiblingIndex() + 1);
            });
            groupTypeObsMap[GroupInfo.GroupType.OCEAN].AsEnumerable().Reverse().ToList().ForEach(kvp => {
                GameObject instant = Instantiate(itemPrefab, itemContainer.transform);
                instant.GetComponent<ViewRegionSimpleItem>().SetValue(kvp.Key, kvp.Value);
                instant.transform.SetSiblingIndex(oceanSplitter.GetSiblingIndex() + 1);
            });
            groupTypeObsMap[GroupInfo.GroupType.NUCLEAR].AsEnumerable().Reverse().ToList().ForEach(kvp => {
                GameObject instant = Instantiate(itemPrefab, itemContainer.transform);
                instant.GetComponent<ViewRegionSimpleItem>().SetValue(kvp.Key, kvp.Value);
                instant.transform.SetSiblingIndex(generatorSplitter.GetSiblingIndex() + 1);
            });


            //동적 크기 조정

            VerticalLayoutGroup vLayout = itemContainer.GetComponent<VerticalLayoutGroup>();
            RectTransform container = vLayout.GetComponent<RectTransform>();
            int childCount = itemContainer.transform.childCount - groupTypeObsMap.Count;
            float splitterHeight = container.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
            float itemHeight = childCount == 3 ? 0f : container.transform.GetChild(1).GetComponent<RectTransform>().rect.height;

            container.sizeDelta = new Vector2(container.sizeDelta.x,
                splitterHeight * groupTypeObsMap.Count
                + itemHeight * childCount
                + vLayout.spacing * (childCount + groupTypeObsMap.Count - 1)
                - container.parent.GetComponent<RectTransform>().rect.height
                );



        }

    }
}
