using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMachineInfo : MonoBehaviour
{
    Button btnClose;
    Vector3 defaultPos;

    TMP_Text txtalaStatus;

    private string currentStcd = "00";
    private Dictionary<string, string> stcdDescriptions = new Dictionary<string, string>()
    {
        { "00", "정상" },
        { "03", "점검중" },
        { "06", "불량" }
    };

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.PopupMachineInfo, OnPopupSetting);

        btnClose = transform.Find("Btn_Close").GetComponent<Button>();
        btnClose.onClick.AddListener(OnCloseSetting);
        txtalaStatus = transform.Find("txtalaStatus")?.GetComponent<TMP_Text>();

        defaultPos = transform.position;
        gameObject.SetActive(false);
    }
    private void OnPopupSetting(object obj)
    {
        transform.position = defaultPos;
        gameObject.SetActive(true);

        // STCD 정보 업데이트
        UpdateStcdDisplay(obj);
    }

    private void UpdateStcdDisplay(object data)
    {
        if (data is string stcdString)
        {
            // 직접 STCD 문자열이 전달된 경우
            currentStcd = stcdString;
        }
        else
        {
            // ModelManager에서 현재 관측소의 센서 데이터 가져오기
            ModelProvider modelProvider = UiManager.Instance.modelProvider;
            List<ToxinData> toxins = modelProvider.GetToxins();

            if (toxins.Count > 0)
            {
                // 첫 번째 센서의 STCD 사용 (또는 특정 보드의 STCD)
                currentStcd = toxins[0].stcd ?? "00";
            }
            else
            {
                currentStcd = "00"; // 기본값
            }
        }

        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (txtalaStatus != null)
        {
            string statusText = GetStcdDescription(currentStcd);
            txtalaStatus.text = statusText;
        }

        Debug.Log($"장비 상태 업데이트: STCD {currentStcd} - {GetStcdDescription(currentStcd)}");
    }

    private string GetStcdDescription(string stcd)
    {
        if (stcdDescriptions.TryGetValue(stcd, out string description))
        {
            return description;
        }
        return "알 수 없는 상태";
    }

    private void OnCloseSetting()
    {
        gameObject.SetActive(false);
    }
}