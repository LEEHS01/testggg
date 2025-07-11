using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlarmListItem : MonoBehaviour
{
    // 알람 데이터
    public LogData data;

    public TMP_Text txtTime;
    public TMP_Text txtDesc;
    public TMP_Text txtMap;
    public TMP_Text txtArea;
    public TMP_Text txtToxin;


    Button btnInspectAlarm;

    // 클릭 이벤트 - UiManager로 전달
    void Start()
    {
        btnInspectAlarm = GetComponent<Button>();
        btnInspectAlarm.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateObs, data.obsId);
        UiManager.Instance.Invoke(UiEventType.SelectAlarm, data.idx);
    }

    public void SetText(LogData data)
    {
        this.data = data;
        this.txtTime.text = data.time.ToString("HH:mm");
        this.txtDesc.text = data.hnsName;
        this.txtMap.text = data.areaName;
        this.txtArea.text = data.obsName;

        switch (data.status)
        {
            case 0:
                this.txtToxin.text = "설비이상";
                break;
            case 1:
                this.txtToxin.text = "경계";
                break;
            case 2:
                this.txtToxin.text = "경보";
                break;
        }
        //this.txtToxin.text = data.toxin.values.Last().ToString() + "%";
    }
}

