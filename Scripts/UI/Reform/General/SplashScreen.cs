using Assets.Scripts.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.General
{

    public class SplashScreen : MonoBehaviour
    {
        Image imgBg;
        TMP_Text lblTitle, lblLoading;

        bool isInitiated = false;
        bool isLoading = false;

        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.PopupError, OnPopupError);
            imgBg = transform.GetComponent<Image>();
            lblTitle = transform.Find("lblTitle").GetComponent<TMP_Text>();
            lblLoading = transform.Find("lblLoading").GetComponent<TMP_Text>();
            isLoading = true;
        }


        private void Update()
        {
            if (isLoading) 
                lblLoading.text = "화면을 불러오는 중" + new string ('.', ((int)(Time.timeSinceLevelLoad*2 % 3) + 1));// * ;
        }

        private void OnInitiate(object obj)
        {
            isInitiated = true;
            isLoading = false;
            this.lblTitle.DOColor(new(0, 0, 0, 0), 1).SetDelay(1);
            this.lblLoading.DOColor(new(0, 0, 0, 0), 1).SetDelay(1);
            this.imgBg.DOColor(new(0, 0, 0, 0), 1).SetDelay(1).OnComplete(() => this.gameObject.SetActive(false));
            lblLoading.text ="불러오기 완료";
        }
        private void OnPopupError(object obj)
        {
            if (obj is not Exception ex) return;

            if (ex.ToString().Contains("DB") && ex.ToString().Contains("연결") && ex.ToString().Contains("실패"))
            {
                isLoading = false;
                lblLoading.text = "DB 연결에 실패했습니다...!";
                lblLoading.color = Color.red;
            }

        }
    }

}
