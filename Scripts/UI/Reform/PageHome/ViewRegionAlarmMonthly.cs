using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.ModelsReform;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Assets.Scripts.Info.BoardSpecInfo;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewRegionAlarmMonthly : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //TMP_Text titleText;
        //List<Image> imgRingCharts = new();
        //List<AreaListMonthItem> items = new();

        // 등록 장비 현황
        TMP_Text txtRegCntToxin, txtRegCntHns, txtRegCntWq;


        // 장비 연결 상태
        TMP_Text txtRegCntAll, txtRegCntNorm, txtRegCntAbnorm;
        Image imgNormRatio;



        private RectTransform rectTransform;
        private Vector2 startAnchoredPos;
        private bool hasInitPosition = false;

        private Tween moveTween;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            //var items = transform.Find("List_Panel").GetComponentsInChildren<AreaListMonthItem>();
            //this.items.AddRange(items);

            //var charts = transform.Find("Doughnut Chart").GetComponentsInChildren<Image>();
            //imgRingCharts.AddRange(charts);

            //titleText = transform.Find("Title Text (TMP)").GetComponentInChildren<TMP_Text>();


            txtRegCntToxin = transform.Find("PnlSensorsStatByType").Find("grpToxin").Find("txtCount").GetComponent<TMP_Text>();
            txtRegCntHns = transform.Find("PnlSensorsStatByType").Find("grpHns").Find("txtCount").GetComponent<TMP_Text>();
            txtRegCntWq = transform.Find("PnlSensorsStatByType").Find("grpWq").Find("txtCount").GetComponent<TMP_Text>();

            txtRegCntAll = transform.Find("PnlSensorsStatByStatus").Find("ArcData").Find("txtProgress").GetComponent<TMP_Text>();
            txtRegCntNorm = transform.Find("PnlSensorsStatByStatus").Find("ArcData").Find("txtNormal").GetComponent<TMP_Text>();
            txtRegCntAbnorm = transform.Find("PnlSensorsStatByStatus").Find("ArcData").Find("txtAbnormal").GetComponent<TMP_Text>();
            imgNormRatio = transform.Find("PnlSensorsStatByStatus").Find("ArcData").Find("imgProgress").GetComponent<Image>();


            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarm);

            UiManager.Instance.Register(UiEventType.NavigateRegion, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateCctv, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateHistory, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateSetting, OnNavigateOut);
            UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        }

        private void OnDestroy()
        {
            moveTween?.Kill();
        }

        // 애니메이션
        private void OnNavigateHome(object obj)
        {
            if (rectTransform == null || !hasInitPosition)
                return;

            moveTween?.Kill();
            moveTween = rectTransform
                .DOAnchorPos(startAnchoredPos, 0.5f)
                .SetEase(Ease.OutCubic);
        }

        // 애니메이션
        private void OnNavigateOut(object obj)
        {
            if (rectTransform == null)
                return;

            moveTween?.Kill();

            Vector2 targetPos = rectTransform.anchoredPosition + new Vector2(-400f, 0f);

            moveTween = rectTransform
                .DOAnchorPos(targetPos, 0.5f)
                .SetEase(Ease.OutCubic);
        }

        private void OnChangeAlarm(object obj)
        {
            List<ObservatoryInfo> obss = modelProvider.GetObss();
            List<GroupInfo> groups = modelProvider.GetGroups();
            List<AlarmInfo> alarms = modelProvider.GetAlarmsWhole();
            List<BoardSpecInfo> boardSpecs = modelProvider.GetBoardSpecs();

            int cntToxin = 0, cntHns = 0, cntWq = 0;
            int cntNorm = 0, cntAbnorm = 0;

            List<(int obsIdx, int sensorIdx)> sensorList = new();


            int cntAll = obss.Sum(o => o.boards.Sum(brd =>
            {
                if (brd.info.modelCode != "" && boardSpecs.Find(spec => spec.modelCode == brd.info.modelCode) != null)
                {
                    var brdSpec = boardSpecs.Find(spec => spec.modelCode == brd.info.modelCode);

                    switch (brd.type)
                    {
                        case BoardType.TOXIN:
                            cntToxin += brdSpec.sensorsDefinitionMap.Count;
                            break;
                        case BoardType.HNS:
                            cntHns += brdSpec.sensorsDefinitionMap.Count;
                            break;
                        case BoardType.WQ:
                            cntWq += brdSpec.sensorsDefinitionMap.Count;
                            break;
                    }

                    brdSpec.sensorsDefinitionMap.Keys.ToList().ForEach(sensorIdx =>
                    {
                        sensorList.Add((o.obsIdx, sensorIdx));
                    });


                    return brdSpec.sensorsDefinitionMap.Count;
                }
                return 0;
            }));


            //AlarmState.SYSTEM_ERROR는 미할당에 해당하는 부분으로 제외. 통신이나 장비 단위의 에러가 아님
            List<AlarmState> abnormalStates = new(){ AlarmState.COM_ERROR, AlarmState.LIVE_ERROR, AlarmState.ETC_ERROR, AlarmState.SYSTEM_ERROR };

            sensorList.ForEach(addr => {



                var s = obss.Find(o => o.obsIdx == addr.obsIdx).sensors.Find(s => s.idx == addr.sensorIdx).info;

                Debug.Log($"{addr.obsIdx} / {addr.sensorIdx} / {(AlarmState)s.alarmType}");
                if (abnormalStates.Contains((AlarmState)s.alarmType))
                    cntAbnorm++;
                else
                    cntNorm++;
            });

            txtRegCntAbnorm.text = $"비정상 {cntAbnorm.ToString()}";
            txtRegCntNorm.text = $"정상 작동 {cntNorm.ToString()}";
            txtRegCntAll.text = $"총 관리 대수 {cntAll.ToString()}";
            txtRegCntToxin.text = cntToxin.ToString();
            txtRegCntHns.text = cntHns.ToString();
            txtRegCntWq.text = cntWq.ToString();
            imgNormRatio.fillAmount = cntAll != 0 ? (float)cntNorm / cntAll : 0f;
            imgNormRatio.fillAmount = 0.1f + 0.8f * imgNormRatio.fillAmount; //시각적으로 10% 씩 말미가 있어서 10~90% 사이로 표현
        }

        private void OnInitiate(object obj)
        {
            if (rectTransform != null && !hasInitPosition)
            {
                startAnchoredPos = rectTransform.anchoredPosition;
                hasInitPosition = true;
            }

            OnChangeAlarm(obj);
        }
    }
}


