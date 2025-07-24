using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PopupError2 : MonoBehaviour
{
    Vector3 deafultPos;

    TMP_Text lblTitle, lblMessage;
    Button btnClose;

    private void Awake()
    {

        Transform titleLine = transform.Find("TitleLine");
        lblTitle = titleLine.Find("lblTitle").GetComponent<TMP_Text>();
        btnClose = titleLine.Find("btnClose").GetComponent<Button>();

        lblMessage = transform.Find("lblSummary").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        //Debug.Log("PopupError2 Start() 시작");

        // UiManager.Instance null 체크
        if (UiManager.Instance == null)
        {
            Debug.LogError("UiManager.Instance가 null입니다!");
            return;
        }

        //Debug.Log("UiManager.Instance 확인됨");

        deafultPos = transform.position;
        //Debug.Log($"기본 위치 설정: {deafultPos}");

        try
        {
            UiManager.Instance.Register(UiEventType.PopupErrorMonitorB, OnPopupError);
           // Debug.Log("이벤트 등록 성공");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"이벤트 등록 실패: {ex.Message}");
        }

        btnClose.onClick.AddListener(OnCloseError);
        gameObject.SetActive(false);

        //Debug.Log("PopupError2 Start() 완료");
        /*deafultPos = transform.position;
        UiManager.Instance.Register(UiEventType.PopupErrorMonitorB, OnPopupError);

        btnClose.onClick.AddListener(OnCloseError);
        gameObject.SetActive(false);*/
    }

    void OnPopupError(object obj)
    {
        Exception showEx = null;

        if (obj is TargetInvocationException invocationEx)
            showEx = invocationEx.InnerException;

        if (obj is Exception ex)
            showEx = ex;

        if (showEx == null) return;

        //transform.position = deafultPos;
        gameObject.SetActive(true);

        lblTitle.text = "오류 발생";
        lblMessage.text = $"{showEx.GetType()}\n{showEx.Message}\n{showEx.StackTrace}";
        Debug.LogError(showEx);
    }

    void OnCloseError()
    {
        gameObject.SetActive(false);
    }


}