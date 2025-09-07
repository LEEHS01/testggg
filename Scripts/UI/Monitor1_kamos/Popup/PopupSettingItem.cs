using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 환경설정 팝업의 개별 센서 아이템 - 표시여부 토글 + 임계값 편집
/// </summary>
public class PopupSettingItem : MonoBehaviour
{
    public Toggle tglVisibility;      // 센서 표시/숨김 토글
    public TMP_Text lblSensorName;    // 센서명 표시
    int obsId, boardId, hnsId;
    public bool isValid = false;      // 유효한 센서인지 여부 (hnsId > 0)

    public TMP_InputField hiField;    // 경계값 (ALAHIVAL) 편집
    public TMP_InputField hihiField;  // 경보값 (ALAHIHIVAL) 편집

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

    /// <summary>
    /// 센서 정보 및 임계값 설정
    /// </summary>
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
        isValid = hnsId > 0; // 더미 데이터(-1) 제외
    }

    private void OnValueChanged(bool isVisible)
    {
        UiManager.Instance.Invoke(UiEventType.CommitSensorUsing, (obsId, boardId, hnsId, isVisible));
    }

    #region 경계, 경고값 수정
    /// <summary>
    /// 경계값(ALAHIVAL)
    /// </summary>
    private void OnHiValueChanged(string value)
    {
        if (!isValid) return;
        if (float.TryParse(value, out float newHi))
        {
            UiManager.Instance.Invoke(UiEventType.UpdateThreshold,
                (obsId, boardId, hnsId, "ALAHIVAL", newHi));
        }
    }

    /// <summary>
    /// 경보값(ALAHIHIVAL)
    /// </summary>
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