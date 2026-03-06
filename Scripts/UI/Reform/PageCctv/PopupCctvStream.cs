using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageCctv
{
    internal class PopupCctvStream : MonoBehaviour
    {
        //참고용
        PopupCCTV popup;

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
