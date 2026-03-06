using Assets.Scripts.Manager;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AreaListMonthItem : MonoBehaviour
{
    ModelProvider ModelProvider => UiManager.Instance.modelProvider;
    int regionId;

    TMP_Text lblAreaName, lblPercentage;
    Image imgColor;
    Button btnNavigateArea;
    List<GetNumData> numDatas;

    private void Start()
    {
        lblAreaName = transform.Find("Text (TMP)_Content").GetComponent<TMP_Text>();
        lblPercentage = transform.Find("Text (TMP)_Percent").GetComponent<TMP_Text>();

        numDatas = GetComponentsInChildren<GetNumData>().ToList();

        btnNavigateArea = GetComponent<Button>();
        btnNavigateArea.onClick.AddListener(OnClick);

        imgColor = transform.Find("Label_Colors").GetComponent<Image>();
    }

    public void SetAreaData(Color color, int regionId, string areaName, int obsCount, int alarmCount, float percent)
    {
        //Debug.Log($"SetAreaData 호출: {areaName} = {count}건");
        imgColor.color = color;
        this.regionId = regionId;
        lblAreaName.text = areaName + $"({obsCount})";

        numDatas.ForEach(numData => numData.ForcedUpdateView(alarmCount));

        lblPercentage.text = "" + Mathf.FloorToInt(percent * 100f) + " %";
    }

    void OnClick()
    {
        if (regionId < 1) return;
        UiManager.Instance.Invoke(UiEventType.NavigateRegion, regionId);
    }

}