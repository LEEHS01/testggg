using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupSettingItem : MonoBehaviour
{
    public Toggle tglVisibility;
    public TMP_Text lblSensorName;

    int obsId, boardId, hnsId;

    public bool isValid = false;

    private void Awake()
    {
        tglVisibility = GetComponentInChildren<Toggle>();
        lblSensorName = GetComponentInChildren<TMP_Text>();
    }
    private void Start()
    {

        tglVisibility.onValueChanged.AddListener(OnValueChanged);
    }

    public void SetItem(int obsId, int boardId, int hnsId, string sensorName, bool isVisible) 
    {
        lblSensorName.text = sensorName;
        tglVisibility.SetIsOnWithoutNotify(isVisible);
        this.obsId = obsId;
        this.boardId = boardId;
        this.hnsId = hnsId;

        isValid = hnsId > 0;
    }
    private void OnValueChanged(bool isVisible)
    {
        //Temporary Function
        UiManager.Instance.Invoke(UiEventType.CommitSensorUsing, (obsId, boardId, hnsId, isVisible));
    }
}