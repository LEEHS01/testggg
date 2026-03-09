using Assets.Scripts.Manager;
using Assets.Scripts.UI.Reform.General;
using Assets.Scripts.UI.Reform.PageHome;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.Pages
{
    public class PageRegion : Page
    {
        private Dictionary<GameObject, float> originalAlphaMap = new();

        public override UiEventType CallingEventType => UiEventType.NavigateRegion;

        const float MAP_MOVE_DURATION = 0.3f;

        PannableObservatoryMap obsMap;
        public Vector3 positionValue => obsMap.GetComponent<RectTransform>().position;
        public float scaleValue => obsMap.GetComponent<RectTransform>().localScale.x;

        FadingPageComponent fadingComponent;


        private void Start()
        {
            fadingComponent = gameObject.AddComponent<FadingPageComponent>();

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        }

        private void OnInitiate(object obj)
        {
            obsMap = transform.GetComponentInChildren<PannableObservatoryMap>();
        }
        override public void Show(UiEventType from, UiEventType to)
        {
            if (from == to) return;

            fadingComponent.Show(from, to);


            if (obsMap != null && from == UiEventType.NavigateHome)
            {
                PageHome pageHome = transform.parent.GetComponentInChildren<PageHome>(true);

                var positionValue = this.positionValue;
                var scaleValue = this.scaleValue;

                obsMap.GetComponent<RectTransform>().position = pageHome.positionValue;
                obsMap.GetComponent<RectTransform>().localScale = Vector3.one * pageHome.scaleValue;

                obsMap.SetAnimation(-1, positionValue, scaleValue, MAP_MOVE_DURATION);
            }
        }

        public override void Hide(UiEventType from, UiEventType to)
        {
            fadingComponent.Hide(from, to);
        }

    }
}
