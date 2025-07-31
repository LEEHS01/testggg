using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TMGetTime : MonoBehaviour
{
    public TextMeshProUGUI text;

    public bool isHMSorYMD = true;


    //1초마다
    private float updateInterval = 1.0f;
    private float lastUpdateTime = 0f;

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            if (isHMSorYMD)
                text.text = System.DateTime.Now.ToString("HH:mm:ss");
            else
                text.text = System.DateTime.Now.ToString("yyyy/MM/dd");

            lastUpdateTime = Time.time;
        }
    }
    /*void Update()
    {
        if(isHMSorYMD)
        //text.text = System.DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        text.text = System.DateTime.Now.ToString("HH:mm:ss");
        else
            text.text = System.DateTime.Now.ToString("yyyy/MM/dd");

    }*/
}
