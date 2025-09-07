using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 기계 제원 정보 팝업 - 센서/장비의 상태 정보 표시
/// </summary>
public class PopupMachineInfo : MonoBehaviour
{
    #region [UI 컴포넌트들]
    Button btnClose;        // 닫기 버튼
    Vector3 defaultPos;     // 기본 위치
    TMP_Text txtalaStatus;  // 장비 상태 텍스트
    #endregion

    #region [상태 코드 관련]
    private string currentStcd = "00";  // 현재 상태 코드

    // STCD(상태 코드) 설명 딕셔너리
    private Dictionary<string, string> stcdDescriptions = new Dictionary<string, string>()
    {
        { "00", "정상" },      // 정상 작동
        { "03", "점검중" },    // 점검/유지보수 중
        { "06", "불량" }       // 장비 불량/고장
    };
    #endregion

    private void Start()
    {
        // 기계 정보 팝업 이벤트 등록
        UiManager.Instance.Register(UiEventType.PopupMachineInfo, OnPopupSetting);

        // UI 컴포넌트 초기화
        btnClose = transform.Find("Btn_Close").GetComponent<Button>();
        btnClose.onClick.AddListener(OnCloseSetting);
        txtalaStatus = transform.Find("txtalaStatus")?.GetComponent<TMP_Text>();

        defaultPos = transform.position;
        gameObject.SetActive(false); // 초기에는 숨김
    }

    /// <summary>
    /// 기계 정보 팝업 표시 - 핵심 메서드
    /// </summary>
    /// <param name="obj">STCD 문자열 또는 기타 데이터</param>
    private void OnPopupSetting(object obj)
    {
        transform.position = defaultPos;
        gameObject.SetActive(true);

        // 전달받은 데이터로 상태 정보 업데이트
        UpdateStcdDisplay(obj);
    }

    /// <summary>
    /// STCD 상태 표시 업데이트
    /// </summary>
    /// <param name="data">상태 데이터 (문자열 또는 객체)</param>
    private void UpdateStcdDisplay(object data)
    {
        if (data is string stcdString)
        {
            // 직접 STCD 문자열이 전달된 경우 (예: "00", "03", "06")
            currentStcd = stcdString;
        }
        else
        {
            // 데이터가 없으면 ModelManager에서 센서 데이터 가져오기
            ModelProvider modelProvider = UiManager.Instance.modelProvider;
            List<ToxinData> toxins = modelProvider.GetToxins();

            if (toxins.Count > 0)
            {
                // 첫 번째 센서의 STCD 사용 (실제로는 특정 보드나 센서 선택 가능)
                currentStcd = toxins[0].stcd ?? "00";
            }
            else
            {
                currentStcd = "00"; // 기본값: 정상
            }
        }

        // UI 텍스트 업데이트
        UpdateStatusText();
    }

    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    private void UpdateStatusText()
    {
        if (txtalaStatus != null)
        {
            string statusText = GetStcdDescription(currentStcd);
            txtalaStatus.text = statusText;
        }

        Debug.Log($"장비 상태 업데이트: STCD {currentStcd} - {GetStcdDescription(currentStcd)}");
    }

    /// <summary>
    /// STCD 코드를 한글 설명으로 변환
    /// </summary>
    /// <param name="stcd">상태 코드 (예: "00", "03", "06")</param>
    /// <returns>한글 상태 설명</returns>
    private string GetStcdDescription(string stcd)
    {
        if (stcdDescriptions.TryGetValue(stcd, out string description))
        {
            return description;
        }
        return "알 수 없는 상태"; // 정의되지 않은 코드의 경우
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    private void OnCloseSetting()
    {
        gameObject.SetActive(false);
    }
}