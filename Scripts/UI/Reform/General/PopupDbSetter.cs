using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.General
{
    internal class PopupDbSetter : MonoBehaviour
    {
        TMP_InputField txbDbUrl;
        Button btnClose, btnCancel, btnConfirm;

        private void Start()
        {
            btnCancel = transform.Find("Back_Image").Find("btnCancel").GetComponent<Button>();
            btnConfirm = transform.Find("Back_Image").Find("btnConfirm").GetComponent<Button>();
            btnClose = transform.Find("btnClose").GetComponent<Button>();
            txbDbUrl = transform.Find("Back_Image").Find("txbDbUrl").GetComponent<TMP_InputField>();

            btnClose.onClick.AddListener(OnClickClose);
            btnCancel.onClick.AddListener(OnClickClose);
            btnConfirm.onClick.AddListener(OnClickConfirm);

            UiManager.Instance.Register(UiEventType.PopupDbSet, OnPopupDbSet);

            gameObject.SetActive(false);
        }

        private void OnClickConfirm()
        {
            // 입력된 URL을 저장
            PlayerPrefs.SetString("DB_URL", txbDbUrl.text);
            PlayerPrefs.Save();


            Application.Quit();
        }

        private void OnClickClose()
        {
            // 팝업을 닫는다. 필요한 정리 작업이 있다면 여기에 추가.
            gameObject.SetActive(false);
        }

        private void OnPopupDbSet(object obj)
        {
            gameObject.SetActive(true);
            txbDbUrl.text = PlayerPrefs.GetString("DB_URL", "http://127.0.0.1:2000/");
        }
    }
}
