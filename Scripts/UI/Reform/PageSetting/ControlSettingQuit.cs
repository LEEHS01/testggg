using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageSetting
{
    internal class ControlSettingQuit : MonoBehaviour
    {
        Button btnQuit;
        

        private void Start()
        {
            btnQuit = GetComponent<Button>();
            btnQuit.onClick.AddListener(OnClickQuit);
        }

        private void OnClickQuit()
        {
            Application.Quit(1);
        }
    }
}
