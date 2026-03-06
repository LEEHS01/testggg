using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageSetting
{
    internal class ControlSettingObs : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;


        TMP_Text txtObsName, txtGroupName, txtObsAddress;

        Button btnResetInspect;
        Button btnResetThreshold;

        List<ObservatoryInfo> obss;
        List<GroupInfo> groups;
        int obsIdx = -1;

        private void Start()
        {
            txtObsName =        transform.Find("txtObsName").GetComponent<TMP_Text>();
            txtGroupName =      transform.Find("txtGroupName").GetComponent<TMP_Text>();
            txtObsAddress =    transform.Find("txtObsAddress").GetComponent<TMP_Text>();

            btnResetInspect =   transform.Find("btnAllSensorResetInspect").GetComponent<Button>();
            btnResetInspect.onClick.AddListener(OnClickResetInspect);
            btnResetThreshold = transform.Find("btnAllSensorResetThreshold").GetComponent<Button>();
            btnResetThreshold.onClick.AddListener(OnClickResetThreshold);


            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
        }

        private void OnClickResetThreshold()
        {
            UiManager.Instance.Invoke(UiEventType.RequestResetThreshold, obsIdx);
        }

        private void OnClickResetInspect()
        {
            UiManager.Instance.Invoke(UiEventType.RequestResetInspect, obsIdx);
        }

        private void OnInitiate(object obj)
        {
            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
        }

        private void OnSelectObs(object obj)
        {
            if(obj is not int obsIdx) { return; }

            if (obsIdx <= 0)
            {
                txtObsName.text = $"관측소 이름 : '---' 관측소";
                txtGroupName.text = $"지역 : --";
                txtObsAddress.text = $"주소 : ---,---";

                return;
            }

            this.obsIdx = obsIdx;

            ObservatoryInfo obs = obss.Find(obs => obs.obsIdx == obsIdx);
            GroupInfo group = groups.Find(group => obs.groupIdx == group.groupIdx);

            txtObsName.text = $"관측소 이름 : '{obs.nameText}' 관측소";
            txtGroupName.text = $"지역 : {(group == null? "--": group.groupName)}";
            txtObsAddress.text = $"주소 : {obs.addrText}";
        }
    }
}
