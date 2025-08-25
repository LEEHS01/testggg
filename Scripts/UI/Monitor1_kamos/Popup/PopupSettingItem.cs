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

    public TMP_InputField hiField;    // 경계값 (ALAHIVAL)
    public TMP_InputField hihiField;  // 경보값 (ALAHIHIVAL)
    private void Awake()
    {
        tglVisibility = GetComponentInChildren<Toggle>();
        lblSensorName = GetComponentInChildren<TMP_Text>();
    }
    private void Start()
    {

        tglVisibility.onValueChanged.AddListener(OnValueChanged);

        // null 체크 추가
        if (hiField != null)
            hiField.onEndEdit.AddListener(OnHiValueChanged);
        if (hihiField != null)
            hihiField.onEndEdit.AddListener(OnHiHiValueChanged);

    }

    public void SetItem(int obsId, int boardId, int hnsId, string sensorName, bool isVisible, float hiValue, float hihiValue) 
    {
        lblSensorName.text = sensorName;
        tglVisibility.SetIsOnWithoutNotify(isVisible);

        // 임계값 설정
        if (hiField != null) hiField.text = hiValue.ToString("F1");
        if (hihiField != null) hihiField.text = hihiValue.ToString("F1");


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

    #region 경계, 경고값 수정
    private void OnHiValueChanged(string value)
    {
        if (!isValid) return;

        if (float.TryParse(value, out float newHi))
        {
            UiManager.Instance.Invoke(UiEventType.UpdateThreshold,
                (obsId, boardId, hnsId, "ALAHIVAL", newHi));
        }
    }

    private void OnHiHiValueChanged(string value)
    {
        if (!isValid) return;

        if (float.TryParse(value, out float newHiHi))
        {
            UiManager.Instance.Invoke(UiEventType.UpdateThreshold,
                (obsId, boardId, hnsId, "ALAHIHIVAL", newHiHi));
        }
    }
    #endregion
}