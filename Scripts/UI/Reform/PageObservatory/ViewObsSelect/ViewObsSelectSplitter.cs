using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Reform.PageObservatory
{
    public class ViewObsSelectSplitter : MonoBehaviour
    {
        TMP_Text lblRegionName, lblObsCount;
        Transform iconCon;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;


        public bool isStarted = false;
        private void Start()
        {
            if (isStarted) return;
            {
                lblRegionName = transform.Find("TxtName").GetComponent<TMP_Text>();
                lblObsCount = transform.Find("TxtObsCount").GetComponent<TMP_Text>();
                iconCon = transform.Find("IconContainer");
            }
            isStarted = true;
        }

        public void SetValue(int groupIdx)
        {
            if (!isStarted) Start();

            List<GroupInfo> groups = modelProvider.GetGroups();
            List<ObservatoryInfo> obss = modelProvider.GetObss();

            GroupInfo myGroup = groups.Find(grp => grp.groupIdx == groupIdx);
            GroupInfo.GroupType myGroupType = myGroup.groupType;
            int obsCount = obss.Count(obs => obs.groupIdx == groupIdx);

            lblRegionName.text = $"{myGroup.groupName} 지역";
            lblObsCount.text = $"(관측소: {obsCount}개)";


            foreach (Transform item in iconCon)
                item.gameObject.SetActive(false);

            switch (myGroupType)
            {
                case GroupInfo.GroupType.GENERAL:
                    iconCon.Find("iconNormal").gameObject.SetActive(true);
                    break;
                case GroupInfo.GroupType.OCEAN:
                    iconCon.Find("iconOcean").gameObject.SetActive(true);
                    break;
                case GroupInfo.GroupType.NUCLEAR:
                    iconCon.Find("iconNuclear").gameObject.SetActive(true);
                    break;
            }
        }
    }
}
