using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SetTime : MonoBehaviour
{
    public TextMeshProUGUI text;

    public bool isHMSorYMD = true;
    
    public void SetText(DateTime date)
    {
        if(isHMSorYMD)
            text.text = date.ToString("HH:mm:ss");
        else
            text.text = date.ToString("yyyy/MM/dd");

    }
}
