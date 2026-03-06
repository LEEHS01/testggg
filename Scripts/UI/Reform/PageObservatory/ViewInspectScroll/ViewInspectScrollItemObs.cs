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
    internal class ViewInspectScrollItemObs : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        TMP_Text txtDesc, txtTitle;


        private void Start()
        {
            txtDesc = transform.Find("TxtDesc").GetComponent<TMP_Text>();
            txtTitle = transform.Find("Title").Find("TxtTitle").GetComponent<TMP_Text>();

            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);

        }

        void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) return;

            var obs =  modelProvider.GetObsByIdx(obsIdx);
            if (obs is not null)
            {
                var group = obs.groupIdx.HasValue ? modelProvider.GetGroupByIdx(obs.groupIdx.Value) : null;

                txtTitle.text = $"'{obs.nameText}' 관측소 정보";
                txtDesc.text = $"관측소 이름\t: '{obs.nameText}' 관측소\r\n소속 지역\t: '{(group == null ? "--" : group.groupName)}' 지역\r\n관측소 주소\t: {obs.addrText}";
            }
            else 
            {
                txtTitle.text = $"-- 관측소 정보";
                txtDesc.text = $"관측소 이름\t: --- 관측소\r\n소속 지역\t: -- 지역\r\n관측소 주소\t: --";
            }
        }


    }
}
