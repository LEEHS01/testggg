using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 에러 팝업 창 - 예외 발생 시 사용자에게 오류 정보 표시
/// </summary>
public class PopupError : MonoBehaviour
{
    #region [UI 컴포넌트들]
    Vector3 deafultPos;     // 기본 위치
    TMP_Text lblTitle;      // 팝업 제목
    TMP_Text lblMessage;    // 에러 메시지 내용
    Button btnClose;        // 닫기 버튼
    #endregion

    /// <summary>
    /// UI 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        Transform titleLine = transform.Find("TitleLine");
        lblTitle = titleLine.Find("lblTitle").GetComponent<TMP_Text>();
        btnClose = titleLine.Find("btnClose").GetComponent<Button>();
        lblMessage = transform.Find("lblSummary").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        deafultPos = transform.position;

        // 에러 팝업 이벤트 등록
        UiManager.Instance.Register(UiEventType.PopupError, OnPopupError);

        btnClose.onClick.AddListener(OnCloseError);
        gameObject.SetActive(false); // 초기에는 숨김
    }

    /// <summary>
    /// 에러 팝업 표시 - 핵심 메서드
    /// </summary>
    /// <param name="obj">Exception 객체</param>
    void OnPopupError(object obj)
    {
        Exception showEx = null;

        // TargetInvocationException의 경우 내부 예외 추출
        if (obj is TargetInvocationException invocationEx)
            showEx = invocationEx.InnerException;

        // 일반 Exception 처리
        if (obj is Exception ex)
            showEx = ex;

        if (showEx == null) return;

        // 팝업 표시
        transform.position = deafultPos;
        gameObject.SetActive(true);

        // 에러 정보 표시
        lblTitle.text = "오류 발생";
        lblMessage.text = $"{showEx.GetType()}\n{showEx.Message}\n{showEx.StackTrace}";

        // 콘솔에도 에러 로그 출력
        Debug.LogError(showEx);
    }

    /// <summary>
    /// 에러 팝업 닫기
    /// </summary>
    void OnCloseError()
    {
        gameObject.SetActive(false);
    }
}