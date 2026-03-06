using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewAlarmRealtime : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        GameObject prefab => Resources.Load<GameObject>("Reform/PageHome/ViewAlarmRealtimeItem");

        private void Start()
        {
           UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
        }

        public void Update()
        {
            if (Time.time % 10 != (Time.time + Time.deltaTime) % 10) // Update every 10 seconds
            {
                OnChangeAlarmList(null);
            }
        }

        void OnChangeAlarmList(object obj)
        {
            List<AlarmInfo> alarmsNew = modelProvider.GetAlarmsActivated();

            //현재 그려진 아이템들
            List<ViewAlarmRealtimeItem> items = transform.Find("Scroll View")
                .GetComponentInChildren<VerticalLayoutGroup>()
                .GetComponentsInChildren<ViewAlarmRealtimeItem>()
                .ToList();

            //현재 그려진 알람들
            List<AlarmInfo> alarmsDrawn = items.Select(item => item.alarmInfo).ToList();
            
            //새로운 알람들과 비교
            List<AlarmInfo> alarmsToRemove = alarmsDrawn.Where(alarmDrawn => alarmsNew.Find(alarmNew => alarmNew.alarmIdx == alarmDrawn.alarmIdx) == null).ToList();
            List<AlarmInfo> alarmsToInsert =  alarmsNew.Where(alarmNew => alarmsDrawn.Find(alarmDrawn => alarmDrawn.alarmIdx == alarmNew.alarmIdx) == null).ToList();

            //삭제
            foreach (AlarmInfo alarmToRemove in alarmsToRemove)
            {
                ViewAlarmRealtimeItem itemToRemove = items.Find(item => item.alarmInfo.alarmIdx == alarmToRemove.alarmIdx);
                if (itemToRemove != null)
                {
                    Destroy(itemToRemove.gameObject);
                }
            }
            //삽입
            foreach (AlarmInfo alarmToInsert in alarmsToInsert)
            {
                GameObject itemObj = Instantiate(prefab, transform.Find("Scroll View").GetComponentInChildren<VerticalLayoutGroup>().transform);
                itemObj.transform.SetAsFirstSibling(); //최상단에 삽입
                ViewAlarmRealtimeItem item = itemObj.GetComponent<ViewAlarmRealtimeItem>();
                item.SetValue(alarmToInsert);
            }

            //동적 크기 조정
            VerticalLayoutGroup vLayout = transform.Find("Scroll View").GetComponentInChildren<VerticalLayoutGroup>();
            RectTransform container = vLayout.GetComponent<RectTransform>();
            int childCount = container.transform.childCount;
            float itemHeight = childCount == 0? 0f : container.transform.GetChild(0).GetComponent<RectTransform>().rect.height;

            container.sizeDelta = new Vector2(container.sizeDelta.x,
                itemHeight * childCount
                + vLayout.spacing * (childCount - 1)
                - container.parent.GetComponent<RectTransform>().rect.height
                );
        }


    }
}
