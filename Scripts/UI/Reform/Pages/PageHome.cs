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
    internal class PageHome : Page
    {
        public override UiEventType CallingEventType => UiEventType.NavigateHome;

        const float MAP_MOVE_DURATION = 0.3f;

        PannableRegionMap regionMap;
        public Vector3 positionValue { 
            get => regionMap.GetComponent<RectTransform>().position; 
            set => regionMap.GetComponent<RectTransform>().position = value; 
        }
        public float scaleValue
        {
            get => regionMap.GetComponent<RectTransform>().localScale.x;
            set => regionMap.GetComponent<RectTransform>().localScale = new Vector3(value, value, value);
        }

        FadingPageComponent fadingComponent;


        private void Start()
        {
            fadingComponent = gameObject.AddComponent<FadingPageComponent>();

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        }

        private void OnInitiate(object obj)
        {
            regionMap = transform.GetComponentInChildren<PannableRegionMap>();
        }



        override public void Show(UiEventType from, UiEventType to)
        {
            if (from == to) return;

            fadingComponent.Show(from, to);


            if(regionMap != null && from == UiEventType.NavigateRegion)
            {
                PageRegion pageRegion = transform.parent.GetComponentInChildren<PageRegion>(true);

                //초기화
                this.positionValue = new Vector3(960, 540, 0);
                this.scaleValue = 1f;

                var positionValue = this.positionValue;
                var scaleValue = this.scaleValue;

                regionMap.GetComponent<RectTransform>().position = pageRegion.positionValue;
                regionMap.GetComponent<RectTransform>().localScale = Vector3.one * pageRegion.scaleValue;

                regionMap.SetAnimation(-1, positionValue, scaleValue, MAP_MOVE_DURATION);
            }
        }

        public override void Hide(UiEventType from, UiEventType to)
        {
            fadingComponent.Hide(from, to);
        }

    }
}
