using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.UI.Reform.PageHome;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageRegion
{
    public class ViewRegionObsList : MonoBehaviour
    {
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageRegion/ViewRegionObsItem");
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        TMP_Text txtTitle;

        List<ObservatoryInfo> obss;
        List<GroupInfo> groups;
        List<AlarmInfo> alarmsActivated;

        private void Start()
        {

            txtTitle = transform.Find("TxtTitle").GetComponent<TMP_Text>();
            txtTitle.text = "-- 지역 관측소";


            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
        }

        void OnInitiate(object obj)
        {
            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
            alarmsActivated = modelProvider.GetAlarmsActivated();
        }
        void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) throw new Exception("Not allowed Type for Payload of this event");

            Transform itemContainer = transform.Find("Scroll View").Find("Viewport").Find("List_Panel");

            //전체 삭제
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }


            //관측소 선택 해제 시
            if (obsIdx == -1) 
            {
                txtTitle.text = "-- 지역 관측소 목록";
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

                obssInGroup.ForEach(obs =>
                {
                    List<AlarmInfo> alarmsActivatedSelObs = alarmsActivated.Where(alarm => alarm.obsIdx == obs.obsIdx).ToList();
                    GameObject instant = Instantiate(itemPrefab, itemContainer);
                    instant.GetComponent<ViewRegionObsItem>().SetValue(obs, alarmsActivatedSelObs);
                });

            }
            //그룹이 없는 관측소 선택시
            else
            {
                txtTitle.text = $"'{selObs.nameText}' 관측소";

                //전체 삭제
                foreach (Transform item in itemContainer)
                    Destroy(item);

                List<AlarmInfo> alarmsActivatedSelObs = alarmsActivated.Where(alarm => alarm.obsIdx == selObs.obsIdx).ToList();
                GameObject instant = Instantiate(itemPrefab, itemContainer);
                instant.GetComponent<ViewRegionObsItem>().SetValue(selObs, alarmsActivatedSelObs);


            }



            //동적 크기 조정
            VerticalLayoutGroup vLayout = transform.Find("Scroll View").GetComponentInChildren<VerticalLayoutGroup>();
            RectTransform container = vLayout.GetComponent<RectTransform>();
            int childCount = container.transform.childCount;
            float itemHeight = childCount == 0 ? 0f : container.transform.GetChild(0).GetComponent<RectTransform>().rect.height;

            container.sizeDelta = new Vector2(container.sizeDelta.x,
                Math.Max(
                itemHeight * childCount
                + vLayout.spacing * (childCount - 1)
                - container.parent.GetComponent<RectTransform>().rect.height,
                0f)
                );


        } 



    }
}
