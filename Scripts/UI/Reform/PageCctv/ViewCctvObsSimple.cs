using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Reform.PageCctv
{
    internal class ViewCctvObsSimple : MonoBehaviour
    {
        TMP_Text txtTitle;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        private void Start()
        {
            txtTitle = transform.Find("TxtTitle").GetComponent<TMP_Text>();
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
        }

        private void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) return;

            string groupName = "--", obsName = "---";

            if (obsIdx > 0)
            {
                var obsInfo = modelProvider.GetObsByIdx(obsIdx);
                if (obsInfo != null)
                {
                    obsName = obsInfo.nameText;
                    if (obsInfo.groupIdx.HasValue)
                    {
                        var groupInfo = modelProvider.GetGroupByIdx(obsInfo.groupIdx.Value);
                        if (groupInfo != null)
                        {
                            groupName = groupInfo.groupName;
                        }
                    }

                }
            }



            txtTitle.text = $"'{groupName}' '{obsName}' 관측소";
        }
    }
}
