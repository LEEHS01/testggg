using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.ModelsReform;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class MarkerRegionMap : MonoBehaviour/*, IPointerEnterHandler, IPointerExitHandler*/
    {
        Button btn;
        Animator animator;
        GameObject focusImage;

        TMP_Text txtGroupName;
        Image imgIconOcean, imgIconNuclear;
        Image imgColorableHead, imgColorableTail;

        GroupInfo groupInfo;
        public List<ObservatoryInfo> obssInGroup;


        private void Start()
        {
            TryGetComponent<Button>(out btn);
            transform.Find("txtGroupName").TryGetComponent<TMP_Text>(out txtGroupName);
            transform.Find("imgContainer").Find("imgColorableTail").TryGetComponent<Image>(out imgColorableTail);
            transform.Find("imgContainer").Find("imgColorableHead").TryGetComponent<Image>(out imgColorableHead);
            transform.Find("imgContainer").Find("imgColorableHead").Find("imgIconOcean").TryGetComponent<Image>(out imgIconOcean);
            transform.Find("imgContainer").Find("imgColorableHead").Find("imgIconNuclear").TryGetComponent<Image>(out imgIconNuclear);

            animator = GetComponentInChildren<Animator>();
            focusImage = transform.Find("Focus").gameObject;
            focusImage.SetActive(false);
            btn.onClick.AddListener(OnClick);

        }


        public void SetValue(GroupInfo groupInfo, List<ObservatoryInfo> obssInGroup)
        {
            if (btn == null || imgColorableTail == null | imgColorableHead == null || imgIconOcean == null || imgIconNuclear == null || txtGroupName == null)
                Start();

            this.groupInfo = groupInfo;
            this.obssInGroup = obssInGroup;

            Dictionary<string, int> groupTypes = new() { { "normal", 0 }, { "exceed", 0 }, { "malfunction", 0 } };
            obssInGroup.ForEach(o => {

                //기능장애 관련 알람 판단...
                //@TODO

                if (
                //범위 초과 알람이 존재하는가?
                    o.sensors.FindIndex(
                        s =>
                            new int[] {
                                (int)AlarmState.TH_HIGH,
                                (int)AlarmState.TH_LOW,
                                (int)AlarmState.TH_HIGH_2,
                                (int)AlarmState.TH_LOW_2
                            }
                            .Contains<int>(s.info.alarmType)
                        ) >= 0
                    )
                    //정상범위초과 관측소 1개 +
                    groupTypes["exceed"]++;
                else
                    //정상 관측소 1개 +
                    groupTypes["normal"]++;
            });

            Color color = groupTypes["malfunction"]!= 0? Color.Lerp(Color.blue, Color.red, 0.5f) : groupTypes["malfunction"] != 0? Color.red : Color.green;



            txtGroupName.text = groupInfo.groupName;
            imgColorableHead.color = color;
            imgColorableTail.color = color;
            imgIconNuclear.color    = new Color(1, 1, 1, groupInfo.groupType == GroupInfo.GroupType.NUCLEAR ? 1f : 0f);
            imgIconOcean.color      = new Color(1, 1, 1, groupInfo.groupType == GroupInfo.GroupType.OCEAN ? 1f : 0f);

        }
        private void OnClick()
        {
            if (groupInfo == null || obssInGroup == null || obssInGroup.Count == 0)
                throw new Exception("something wrong");

            //선택시 지역 화면으로 전환
            UiManager.Instance.Invoke(UiEventType.NavigateRegion);
            if (obssInGroup.Count != 0) //관측소가 있다면 자동선택
                UiManager.Instance.Invoke(UiEventType.SelectObs, obssInGroup.First().obsIdx);
        }

        private void Update()
        {
            GetComponent<RectTransform>().DOScale(2f * GetComponent<RectTransform>().localScale.x / GetComponent<RectTransform>().lossyScale.x, 0.1f);
        }


        #region [호버 애니메이션]

        public void OnPointerEnter(PointerEventData eventData)
        {
            animator?.SetTrigger("Play"); // Play 트리거를 설정
            focusImage.SetActive(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine(StopAnimationAfterDelay(1.0f)); // 1초 후에 Stop 트리거를 설정
            focusImage.SetActive(false);

        }

        private IEnumerator StopAnimationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            animator?.SetTrigger("Stop"); // Stop 트리거를 설정
        }


        private void OnNavigateHome(object obj)
        {
            animator?.SetTrigger("Stop"); // Stop 트리거를 설정
            focusImage.SetActive(false);
        }


        #endregion




    }
}
