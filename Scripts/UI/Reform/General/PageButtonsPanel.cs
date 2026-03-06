using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Assets.Scripts.UI.Reform.General
{
    public class PageButtonsPanel : MonoBehaviour
    {
        
        List<Button> btns { get; set; } = new List<Button>();
        Transform mobileTaskbar;
        RectTransform hoverSensor;
        float hideDelaySec = HIDE_DELAY_MAX;
        const float HIDE_DELAY_MAX = 0.5f;


        private void Start()
        {
            mobileTaskbar = transform.Find("MobileTaskbar");
            hoverSensor = transform.Find("HoverSensor").GetComponent<RectTransform>();


            btns = GetComponentsInChildren<Button>(true).ToList();
            Dictionary<string, UiEventType> btnEventMap = new Dictionary<string, UiEventType>()
            {
                { "BtnHome",    UiEventType.NavigateHome },
                { "BtnRegion",  UiEventType.NavigateRegion },
                { "BtnObs",     UiEventType.NavigateObs },
                { "BtnCctv",    UiEventType.NavigateCctv },
                { "BtnHistory",    UiEventType.NavigateHistory },
                { "BtnSetting",    UiEventType.NavigateSetting },
            };
            btns.ForEach(btn => {
                string name = btn.transform.parent.gameObject.name;
                if(btnEventMap.TryGetValue(name, out UiEventType value))
                {
                    btn.onClick.AddListener(() =>
                    {
                        UiManager.Instance.Invoke(value);
                    });
                }
                else 
                    Debug.LogError($"PageButtonsPanel - Start: 버튼 이름 매핑 실패: {name}");
            });
        }

        private void Update()
        {
            try
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(hoverSensor, Input.mousePosition, Camera.current))
                {
                    if (hideDelaySec != HIDE_DELAY_MAX)
                    {
                        hideDelaySec = HIDE_DELAY_MAX;
                        mobileTaskbar.DOMoveY(40, 0.3f).SetEase(Ease.OutCubic);
                    }
                }
                else
                {
                    hideDelaySec -= Time.deltaTime;

                    if (hideDelaySec <= 0f && 0 < hideDelaySec + Time.deltaTime)
                    {
                        mobileTaskbar.DOMoveY(-40f, 0.3f).SetEase(Ease.OutCubic);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
                
        }


    }
}
