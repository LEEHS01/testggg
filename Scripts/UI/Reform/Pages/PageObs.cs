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
    public class PageObs : Page
    {
        public override UiEventType CallingEventType => UiEventType.NavigateObs;


        FadingPageComponent fadingComponent;


        private void Start()
        {
            fadingComponent = gameObject.AddComponent<FadingPageComponent>();
        }


        override public void Show(UiEventType from, UiEventType to)
        {
            if (from == to) return;
            fadingComponent.Show(from, to);
        }

        public override void Hide(UiEventType from, UiEventType to)
        {
            fadingComponent.Hide(from, to);
        }

    }
}
