using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class ObsMonitoringItem : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    //Componenets
    UILineRenderer trdSensor;
    Image imgSignalLamp;
    TMP_Text lblSensorName, lblUnit, lblValue;
    Button btnSelectCurrentSensor;

    //value
    ToxinData toxin;
    int obsId = 0;


    static Dictionary<ToxinStatus, Color> statusColorDic = new();
    //static Dictionary<AreaData.AreaType, Sprite> areaSpriteDic = new();
    static ObsMonitoringItem()
    {
        Dictionary<ToxinStatus, string> rawColorSets = new() {
            { ToxinStatus.Green,    "#3EFF00"},
            { ToxinStatus.Yellow,   "#FFF600"},
            { ToxinStatus.Red,      "#FF0000"},
            { ToxinStatus.Purple,   "#6C00E2"},
        };

        Color color;
        foreach (var pair in rawColorSets)
            if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                statusColorDic[pair.Key] = color;
    }
    private void Awake()
    {
        trdSensor = GetComponentInChildren<UILineRenderer>();
        imgSignalLamp = transform.Find("Icon_SignalLamp").GetComponent<Image>();

        lblSensorName = transform.Find("Text (TMP) List").GetComponent<TMP_Text>();
        lblValue = transform.Find("Text (TMP) List (1)").GetComponent<TMP_Text>();
        lblUnit = transform.Find("Text (TMP) List (2)").GetComponent<TMP_Text>(); //단위로 수정
        btnSelectCurrentSensor = GetComponent<Button>();
        btnSelectCurrentSensor.onClick.AddListener(OnClick);
    }

    public void SetToxinData(int obsId, ToxinData toxin) 
    {
        //Debug.Log($"ObsMonitoringItem toxin is null: {toxin == null}");
        //Debug.Log($"ObsMonitoringItem lblSensorName is null: {lblSensorName == null}");
        //Debug.Log($"ObsMonitoringItem imgSignalLamp is null: {imgSignalLamp == null}");
        this.toxin = toxin;
        lblSensorName.text = toxin.hnsName;
        //lblUnit.text =  "" + toxin.warning;
        lblValue.text = "" + toxin.GetLastValue().ToString("F2");
        lblUnit.text = toxin.unit ?? "";
        this.obsId = obsId;

        gameObject.SetActive(toxin.on);

        ToxinStatus sensorStatus = modelProvider.GetSensorStatus(obsId, toxin.boardid, toxin.hnsid);
        imgSignalLamp.color = statusColorDic[sensorStatus];
    }

    public void ResetToxinStatus() 
    {
        if (obsId < 1 || toxin == null) return; 

        ToxinStatus sensorStatus = modelProvider.GetSensorStatus(obsId, toxin.boardid, toxin.hnsid);
        imgSignalLamp.color = statusColorDic[sensorStatus];

        // *** 추가: 실시간 값도 함께 업데이트 ***
        lblValue.text = "" + toxin.GetLastValue().ToString("F2");
    }


    private void OnClick() 
    {
        UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (toxin.boardid, toxin.hnsid));
    }

    public void UpdateTrendLine()
    {
        if (toxin == null) return;
        if (toxin.values.Count == 0) return;

        List<float> normalizedValues = new();
        //float max = Math.Max(toxin.values.Max(), toxin.warning);

        float max = toxin.values.Max() + 1;

        toxin.values.ForEach(val => normalizedValues.Add(val/max));

        //trdSensor.UpdateControlPoints(normalizedValues.GetRange(normalizedValues.Count * 2 / 3 - 1, normalizedValues.Count / 3));
        trdSensor.UpdateControlPoints(normalizedValues);
        lblValue.text = "" + toxin.GetLastValue().ToString("F2");
    }
}