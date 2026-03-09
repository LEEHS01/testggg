using Assets.Scripts.Manager;
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

namespace Assets.Scripts.UI.Reform.General
{
    public class FadingPageComponent : MonoBehaviour
    {
        private Dictionary<GameObject, float> originalAlphaMap = new();
        Page page;
        const float MAP_MOVE_DURATION = 0.3f;
        private void Start()
        {
            if (!TryGetComponent<Page>(out page))
                throw new Exception($"FadingPageComponent must be attached to a GameObject with a Page component. GameObject: {gameObject.name}");


            MemorizeAlphas();
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        }


        private void OnInitiate(object obj)
        {
            MemorizeAlphas();
        }



        public void Show(UiEventType from, UiEventType to)
        {
            if (from == to) return;
            gameObject.SetActive(true);

            transform.GetComponentsInChildren<Image>().ToList().ForEach(img =>
            {
                if (originalAlphaMap.TryGetValue(img.gameObject, out _))
                {
                    Color fromColor = img.color;
                    DOTween.ToAlpha(() => fromColor, x => fromColor = x, originalAlphaMap[img.gameObject], MAP_MOVE_DURATION).OnUpdate(() =>
                        img.color = fromColor
                    );
                }
            });

            transform.GetComponentsInChildren<TMP_Text>().ToList().ForEach(img =>
            {
                if (originalAlphaMap.TryGetValue(img.gameObject, out _))
                {
                    Color fromColor = img.color;
                    DOTween.ToAlpha(() => fromColor, x => fromColor = x, originalAlphaMap[img.gameObject], MAP_MOVE_DURATION).OnUpdate(() =>
                        img.color = fromColor
                    );
                }
            });
        }

        public void Hide(UiEventType from, UiEventType to)
        {
            if (from != page.CallingEventType)
            {
                transform.GetComponentsInChildren<Image>().ToList()
                    .ForEach(img => img.color = new Color(img.color.r, img.color.g, img.color.b, 0f));

                transform.GetComponentsInChildren<TMP_Text>().ToList()
                    .ForEach(img => img.color = new Color(img.color.r, img.color.g, img.color.b, 0f));

                gameObject.SetActive(false);
                return;
            }

            MemorizeAlphas();

            transform.GetComponentsInChildren<Image>().ToList().ForEach(img =>
            {
                Color fromColor = img.color;
                DOTween.ToAlpha(() => fromColor, x => fromColor = x, 0, MAP_MOVE_DURATION).OnUpdate(() =>
                    img.color = fromColor
                );
            });

            transform.GetComponentsInChildren<TMP_Text>().ToList().ForEach(img =>
            {
                Color fromColor = img.color;
                DOTween.ToAlpha(() => fromColor, x => fromColor = x, 0, MAP_MOVE_DURATION).OnUpdate(() =>
                    img.color = fromColor
                );
            });

            DOTween.To(() => default, x => { }, string.Empty, MAP_MOVE_DURATION).onComplete += () => gameObject.SetActive(false);
        }

        void MemorizeAlphas()
        {
            transform.GetComponentsInChildren<Image>(true).ToList().ForEach(img => {
                if (!originalAlphaMap.ContainsKey(img.gameObject))
                    originalAlphaMap[img.gameObject] = img.color.a;
            });
            transform.GetComponentsInChildren<TMP_Text>(true).ToList().ForEach(img => {
                if (!originalAlphaMap.ContainsKey(img.gameObject))
                    originalAlphaMap[img.gameObject] = img.color.a;
            });
        }
    }
}
