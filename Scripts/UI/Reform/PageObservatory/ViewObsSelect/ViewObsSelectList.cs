using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.UI.Reform.PageHome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageObservatory
{
    public class ViewObsSelectList : MonoBehaviour
    {
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageObservatory/ViewObsSelectItem");
        GameObject splitterPrefab => Resources.Load<GameObject>("Reform/PageObservatory/ViewObsSelectSplitter");
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        RectTransform listPanel;

        List<ObservatoryInfo> obss;
        List<GroupInfo> groups;
        List<AlarmInfo> alarmsActivated;

        bool isInit = false, isFilled = false;

        private void Start()
        {
            listPanel = transform.Find("Scroll View").Find("Viewport").Find("List_Panel").GetComponent<RectTransform>();

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiated);
        }

        void OnInitiated(object obj)
        {
            isInit = true;
        }
        private void Update()
        {
            if (isInit == isFilled) return;

            isFilled = true;

            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
            alarmsActivated = modelProvider.GetAlarmsActivated();

            Transform itemContainer = transform.Find("Scroll View").Find("Viewport").Find("List_Panel");

            //전체 삭제
            //foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            //foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }

            groups = groups.Where(group => obss.Find(obs => obs.groupIdx == group.groupIdx) != null).ToList();

            //그룹-관측소 매핑
            Dictionary< GroupInfo, List < ObservatoryInfo >> groupObsMap = new();
            groups.ForEach(group =>
            {
                var list = obss.Where(obs => group.groupIdx == obs.groupIdx).ToList();
                groupObsMap.Add(group, list);
            });

            //아이템 생성
            foreach (KeyValuePair<GroupInfo, List<ObservatoryInfo>> kvp in groupObsMap)
            {
                GroupInfo grp = kvp.Key;
                List<ObservatoryInfo> obssInGrp = kvp.Value;

                //지역 생성
                GameObject instant = Instantiate(splitterPrefab, itemContainer);
                instant.GetComponent<ViewObsSelectSplitter>().SetValue(grp.groupIdx);
                //관측소들 생성
                obssInGrp.ForEach(obs =>
                {
                    List<AlarmInfo> alarmsActivatedSelObs = alarmsActivated.Where(alarm => alarm.obsIdx == obs.obsIdx).ToList();
                    GameObject instant = Instantiate(itemPrefab, itemContainer);
                    instant.GetComponent<ViewObsSelectItem>().SetValue(obs, alarmsActivatedSelObs);
                });

            }

            //동적 크기 조정
            VerticalLayoutGroup vLayout = transform.Find("Scroll View").GetComponentInChildren<VerticalLayoutGroup>();
            RectTransform container = vLayout.GetComponent<RectTransform>();
            int splitterCount = groupObsMap.Count;
            int childCount = groupObsMap.Sum(kvp => kvp.Value.Count);
            float splitterHeight = splitterPrefab.GetComponent<RectTransform>().rect.height;
            float itemHeight = childCount == 0 ? 0f : itemPrefab.GetComponent<RectTransform>().rect.height;

            container.sizeDelta = new Vector2(container.sizeDelta.x,
                Math.Max(
                splitterHeight * groupObsMap.Count
                + itemHeight * childCount
                + vLayout.spacing * (childCount + groupObsMap.Count - 1)
                - container.parent.GetComponent<RectTransform>().rect.height
                , 0)
                );


        }
    }
}
