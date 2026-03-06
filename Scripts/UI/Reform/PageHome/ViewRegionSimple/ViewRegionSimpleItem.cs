using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class ViewRegionSimpleItem : MonoBehaviour
    {
        Button btn;
        TMP_Text txtGroupName, txtObsCountNormal, txtObsCountExceed, txtObsCountMalfunction;

        GroupInfo groupInfo;
        List<ObservatoryInfo> obssInGroup;

        bool isStartCalled = false;

        private void Start()
        {
            if (isStartCalled) return;
            transform.Find("SignalLamps").Find("SignalLamp_Green").GetChild(0).TryGetComponent<TMP_Text>(out txtObsCountNormal);
            transform.Find("SignalLamps").Find("SignalLamp_Red").GetChild(0).TryGetComponent<TMP_Text>(out txtObsCountExceed);
            transform.Find("SignalLamps").Find("SignalLamp_Purple").GetChild(0).TryGetComponent<TMP_Text>(out txtObsCountMalfunction);
            transform.Find("TitleName_Button").GetChild(0).TryGetComponent<TMP_Text>(out txtGroupName);
            TryGetComponent<Button>(out btn);

            if (btn == null || txtObsCountNormal == null | txtObsCountExceed == null || txtObsCountMalfunction == null || txtGroupName == null)
            {
                Debug.LogError("ViewRegionSimpleItem: One or more components not found!");
            }

            isStartCalled = true;
            btn.onClick.AddListener(OnClick);
        }

        public void SetValue(GroupInfo group, List<ObservatoryInfo> obssInGroup)
        {
            if (btn == null || txtObsCountNormal == null | txtObsCountExceed == null || txtObsCountMalfunction == null || txtGroupName == null)
                Start();

            this.groupInfo = group;
            this.obssInGroup = obssInGroup;

            Dictionary<string,int> groupTypes = new() { { "normal", 0 },{ "exceed", 0 },{ "malfunction", 0 } };
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


            txtGroupName.text = group.groupName;
            txtObsCountNormal.text = $"{groupTypes["normal"]:D2}";
            txtObsCountExceed.text = $"{groupTypes["exceed"]:D2}";
            txtObsCountMalfunction.text = $"{groupTypes["malfunction"]:D2}";

        }
        private void OnClick()
        {
            if (groupInfo == null || obssInGroup == null) 
                throw new Exception("something wrong");

            //선택시 지역 화면으로 전환
            UiManager.Instance.Invoke(UiEventType.NavigateRegion);
            if (obssInGroup.Count != 0) //관측소가 있다면 자동선택
                UiManager.Instance.Invoke(UiEventType.SelectObs, obssInGroup.First().obsIdx);


        }







    }
}
