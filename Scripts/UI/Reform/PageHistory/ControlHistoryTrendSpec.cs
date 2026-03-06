using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Reform.PageHistory
{
    internal class ControlHistoryTrendSpec : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiated);
        }

        private void OnInitiated(object obj)
        {
            //throw new NotImplementedException();
        }
    }
}
