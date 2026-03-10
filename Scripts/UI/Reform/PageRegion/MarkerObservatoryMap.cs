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
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class MarkerObservatoryMap : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        Button btn;
        Animator animator;
        GameObject focusImage;

        TMP_Text txtGroupName;
        Image imgIconOcean, imgIconNuclear;
        Image imgColorableHead, imgColorableTail;

        GroupInfo groupInfo;
        public ObservatoryInfo obsInfo { get; private set; }

        bool isStarted = false;
        private void Start()
        {
            if(isStarted) return;
            TryGetComponent<Button>(out btn);
            transform.Find("txtGroupName").TryGetComponent<TMP_Text>(out txtGroupName);
            transform.Find("imgContainer").Find("imgColorableTail").TryGetComponent<Image>(out imgColorableTail);
            transform.Find("imgContainer").Find("imgColorableHead").TryGetComponent<Image>(out imgColorableHead);

            animator = GetComponentInChildren<Animator>();
            focusImage = transform.Find("Focus").gameObject;
            focusImage.SetActive(false);
            btn.onClick.AddListener(OnClick);

            isStarted = true;
        }


        public void SetValue(GroupInfo groupInfo, ObservatoryInfo obsInfo)
        {
            if (btn == null || imgColorableTail == null | imgColorableHead == null || imgIconOcean == null || imgIconNuclear == null || txtGroupName == null)
                Start();

            this.groupInfo = groupInfo;
            this.obsInfo = obsInfo;

            Dictionary<string, int> groupTypes = new() { { "caution", 0 }, { "warning", 0 }, { "malfunction", 0 } };

            var o = obsInfo;

            //기능장애 관련 알람 판단...
            if ( //설비이상 관측소 확인
                o.sensors.FindIndex(
                    s =>
                        new int[] {
                            (int)AlarmState.COM_ERROR,
                            (int)AlarmState.ETC_ERROR,
                            (int)AlarmState.LIVE_ERROR,
                        }
                        .Contains<int>(s.info.alarmType)
                    ) >= 0
                )
                groupTypes["malfunction"]++;

            else if ( //경보 관측소 확인
                o.sensors.FindIndex(
                    s =>
                        new int[] {
                            (int)AlarmState.TH_HIGH_2,
                            (int)AlarmState.TH_LOW_2
                        }
                        .Contains<int>(s.info.alarmType)
                    ) >= 0
                )
                groupTypes["warning"]++;

            else if ( //경계 관측소 확인
                o.sensors.FindIndex(
                    s =>
                        new int[] {
                            (int)AlarmState.TH_HIGH,
                            (int)AlarmState.TH_LOW,
                        }
                        .Contains<int>(s.info.alarmType)
                    ) >= 0
                )
                groupTypes["caution"]++;

            Color color =
                groupTypes["malfunction"] != 0 ? Color.Lerp(Color.blue, Color.red, 0.5f) :
                groupTypes["warning"] != 0 ? Color.red :
                groupTypes["caution"] != 0 ? Color.yellow :
                Color.green;



            txtGroupName.text = obsInfo.nameText;
            imgColorableHead.color = color;
            imgColorableTail.color = color;

            //이상이 없다면...
            //크기로 표현
            if (color == Color.green)
                imgColorableHead.transform.parent.localScale = Vector3.one * 0.3f;
            else
                imgColorableHead.transform.parent.localScale = Vector3.one * 0.6f;


        }
        private void OnClick()
        {
            if (obsInfo == null)
                throw new Exception("something wrong");

            //선택시 지역 화면으로
            //
            if (modelProvider.GetCurrentObsIdx() == obsInfo.obsIdx) //관측소가 있다면 자동선택
                UiManager.Instance.Invoke(UiEventType.NavigateObs);
            else
                UiManager.Instance.Invoke(UiEventType.SelectObs, obsInfo.obsIdx);
        }

        bool isFocused = false; 

        private void Update()
        {
            if (obsInfo == null)
                return;

            if (Time.time % 0.2f != (Time.time + Time.deltaTime) % 0.2f) // Update every 1 seconds
            {
                bool isFocusedNow = modelProvider.GetCurrentObsIdx() == obsInfo.obsIdx;
                if (!isFocused && isFocusedNow) OnPointerEnter();
                if (isFocused && !isFocusedNow) OnPointerExit();
                isFocused = isFocusedNow;
            }

            GetComponent<RectTransform>().DOScale(2f * GetComponent<RectTransform>().localScale.x / GetComponent<RectTransform>().lossyScale.x, 0.1f);

        }


        #region [호버 애니메이션]

        public void OnPointerEnter(/*PointerEventData eventData*/)
        {
            animator?.SetTrigger("Play"); // Play 트리거를 설정
            focusImage.SetActive(true);
        }
        public void OnPointerExit(/*PointerEventData eventData*/)
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
