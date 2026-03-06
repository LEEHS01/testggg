using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Reform.PageSetting
{
    internal class ControlSettingBoardSingular : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        public BoardSpecInfo.BoardType boardType;

        //TODO

        List<ObservatoryInfo> obss;
        List<GroupInfo> groups;


        private void Start()
        {

            //TODO

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);

        }


        private void OnInitiate(object obj)
        {
            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
        }
        private void OnSelectObs(object obj)
        {
            //TODO
        }
    }
}
