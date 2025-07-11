using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ObsMachineButton : MonoBehaviour 
{
    Button btnMachineInfo; 
    private void Start()
    {
        btnMachineInfo = GetComponentInChildren<Button>();
        btnMachineInfo.onClick.AddListener(OnClick);
        
    }

    void OnClick() 
    {
        UiManager.Instance.Invoke(UiEventType.PopupMachineInfo);    
    }
}
