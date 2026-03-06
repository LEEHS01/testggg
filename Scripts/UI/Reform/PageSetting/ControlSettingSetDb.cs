using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageSetting
{
    internal class ControlSettingSetDb : MonoBehaviour
    {
        Button btnSetDb;
        

        private void Start()
        {
            btnSetDb = GetComponent<Button>();
            btnSetDb.onClick.AddListener(OnClickSetDb);
        }

        private void OnClickSetDb()
        {
            UiManager.Instance.Invoke(UiEventType.PopupDbSet);
        }
    }
}
