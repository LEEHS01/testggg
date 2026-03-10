using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class PannableRegionMap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        //기타 참조
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageHome/MarkerRegionMap");

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //UI 오브젝트 참조
        RectTransform panelRectTransform => GetComponent<RectTransform>();
        Image imgBackground;

        //Panning기능
        float moveSpeed = 10f;
        float scrollSpeed = 0.1f;

        float minScale = 0.8f;
        float maxScale = 5f;
        float maxHorizontalMoveRange => maxScale * 500f;  // 최대 확대 시 좌우 이동 거리
        float maxVerticalMoveRange => maxScale * 1000f;    // 최대 확대 시 위아래 이동 거리

        bool controlable = true;

        private Vector3 originalPosition = new Vector3(-50, -100, 0);
        private Vector3 originalScale = new Vector3(1, 1, 1);

        //좌표 변환 기준점
        (CoordinateReference first, CoordinateReference second) coordinateAnchor = (null, null);

        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
            //markers = transform.Find("MarkerList").GetComponentsInChildren<MarkerRegionMap>(true).ToList();
            imgBackground = transform.Find("MapNationBackground").GetComponent<Image>();

            transform.Find("MapNationBackground").Find("PosRefPoint_1").TryGetComponent(out coordinateAnchor.first);
            transform.Find("MapNationBackground").Find("PosRefPoint_2").TryGetComponent(out coordinateAnchor.second);
        }

        private void OnChangeAlarmList(object obj)
        {
            List<ObservatoryInfo> obss = modelProvider.GetObss();
            List<GroupInfo> groups = modelProvider.GetGroups();
            List<AlarmInfo> alarms = modelProvider.GetAlarmsActivated();


            Transform markerContainer = transform.Find("MapNationBackground").Find("MarkerList");

            for (int i = 0; i < markerContainer.childCount; i++)
            {
                Transform marker = markerContainer.GetChild(i);
                MarkerRegionMap markerRegionMap = marker.GetComponent<MarkerRegionMap>();
                if (alarms.Find(alarm => markerRegionMap.obssInGroup.Select(obs => obs.obsIdx).Contains(alarm.obsIdx)) == null)
                    marker.gameObject.SetActive(false);
                else
                    marker.gameObject.SetActive(true);
            }


        }

        private void OnInitiate(object obj)
        {
            List<ObservatoryInfo> obss = modelProvider.GetObss();
            List<GroupInfo> groups = modelProvider.GetGroups();
            List<AlarmInfo> alarms = modelProvider.GetAlarmsActivated();

            Transform markerContainer = transform.Find("MapNationBackground").Find("MarkerList");
            DOVirtual.DelayedCall(0.1f, () => { 
                //우선 그룹에 알맞는 관측소 매핑
                List<(GroupInfo groupInfo, List<ObservatoryInfo> obsList)> groupedObsList;
                groupedObsList = (List<(GroupInfo, List<ObservatoryInfo>)>)groups.Select(
                    groupInfo => {
                        var list = obss.Where(obs => groupInfo.groupIdx == obs.groupIdx).ToList();
                        return (groupInfo, list);
                    }).ToList();

                groupedObsList.ForEach(groupedObs =>
                {
                    GameObject instant = Instantiate(itemPrefab,markerContainer);
                    Vector2 coord = new Vector2(groupedObs.groupInfo.coordinate.X, groupedObs.groupInfo.coordinate.Y);

                    float scale = transform.Find("MapNationBackground").GetComponent<RectTransform>().lossyScale.x;
                    Vector2 consult = CoordinateToLocalPosition(coord);
                    instant.GetComponent<RectTransform>().localPosition = new Vector3(consult.x, consult.y, 0f) * scale;

                    instant.GetComponent<MarkerRegionMap>().SetValue(groupedObs.groupInfo, groupedObs.obsList);

                    if (alarms.Find(alarm => groupedObs.obsList.Select(obs => obs.obsIdx).Contains(alarm.obsIdx)) == null) 
                       instant.gameObject.SetActive(false);

                });
            });
        }


       




        #region [Util]
        Vector2 CoordinateToLocalPosition(Vector2 coordinate)
        {
            var ancF = coordinateAnchor.first;
            var ancS = coordinateAnchor.second;
            Vector2 diffCoord = ancF.referenceCoordinate - ancS.referenceCoordinate;
            Vector2 diffPos = (ancF.GetComponent<RectTransform>().localPosition - ancS.GetComponent<RectTransform>().localPosition);

            Vector2 gradient = new Vector2(diffPos.x / diffCoord.x, diffPos.y / diffCoord.y) ;
            Vector2 zeroCoordPos = ancF.GetComponent<RectTransform>().localPosition - new Vector3(ancF.referenceCoordinate.x * gradient.x, ancF.referenceCoordinate.y * gradient.y);

            Vector2 res = ancF.GetComponent<RectTransform>().localPosition 
                + new Vector3(
                    (coordinate.x - ancF.referenceCoordinate.x) * gradient.x,
                    (coordinate.y - ancF.referenceCoordinate.y) * gradient.y);

            return res;
        }

        #endregion


        //Maybe, not use past
        private void OnNavigateArea(object obj)
        {
            controlable = false;
            SetAnimation(0f, new Vector3(960, 540) + new Vector3(700, 200), 0.60f, 1f);
        }
        private void OnNavigateHome(object obj)
        {
            controlable = true;
            SetAnimation(2 / 5f, new Vector3(960, 540), 1f, 1f);

            panelRectTransform.localPosition = originalPosition;
            panelRectTransform.localScale = originalScale;
        }
        private void OnNavigateObs(object obj)
        {
            controlable = false;
            SetAnimation(0f, new Vector3(960, 540) + new Vector3(700, 200), 0.60f, 1f);
        }


        public void SetAnimation(float alpha, Vector3 toPos, float toScale, float duration)
        {
            Color fromColor = imgBackground.color;
            Vector3 fromPos = GetComponent<RectTransform>().position;
            Vector3 fromScale = GetComponent<RectTransform>().localScale;

            if(alpha >= 0)
            DOTween.ToAlpha(() => fromColor, x => fromColor = x, alpha, duration / 2f).OnUpdate(() =>
            {
                imgBackground.color = fromColor;
            });

            DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() =>
            {
                GetComponent<RectTransform>().position = fromPos;
            });

            DOTween.To(() => fromScale, x => fromScale = x, Vector3.one * toScale, duration).OnUpdate(() =>
            {
                GetComponent<RectTransform>().localScale = fromScale;
            });
        }




        #region [Panning Interactions]
        public void OnBeginDrag(PointerEventData eventData) { }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!controlable) return;

            Vector3 newPos = panelRectTransform.localPosition + new Vector3(eventData.delta.x, eventData.delta.y, 0);
            newPos = ClampPosition(newPos);
            panelRectTransform.localPosition = newPos;
        }
        
        public void OnEndDrag(PointerEventData eventData) { }

        public void OnScroll(PointerEventData eventData)
        {
            if (!controlable) return;

            var rt = panelRectTransform;

            // 1) 스크롤 전 스케일
            Vector3 oldScale = rt.localScale;

            // 2) 새 스케일 계산/클램프
            Vector3 targetScale = oldScale + Vector3.one * eventData.scrollDelta.y * scrollSpeed;
            targetScale = ClampScale(targetScale);

            // 변화 없으면 종료
            if (Mathf.Approximately(targetScale.x, oldScale.x) && Mathf.Approximately(targetScale.y, oldScale.y))
                return;

            // 3) 마우스가 가리키는 "패널 내부 로컬 좌표" 구하기
            // Screen Space Overlay면 camera는 null
            Camera cam = eventData.pressEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, cam, out Vector2 localPointBefore))
                return;

            // 4) 스케일 적용
            rt.localScale = targetScale;

            // 5) 같은 로컬 포인트가 화면에서 같은 위치에 남도록 position 보정
            // 로컬 포인트는 스케일에 비례해서 이동하므로, 스케일 비율만큼 위치를 역으로 이동
            float ratioX = targetScale.x / oldScale.x;
            float ratioY = targetScale.y / oldScale.y;

            Vector3 pos = rt.localPosition;
            pos.x -= localPointBefore.x * (ratioX - 1f);
            pos.y -= localPointBefore.y * (ratioY - 1f);

            // 6) 이동 범위/스케일에 맞게 클램프
            rt.localPosition = ClampPosition(pos);
        }


        Vector3 ClampPosition(Vector3 position)
        {
            float scale = (panelRectTransform.localScale.x - minScale) / (maxScale - minScale);
            float horizontalMoveRange = Mathf.Lerp(0, maxHorizontalMoveRange, scale);
            float verticalMoveRange = Mathf.Lerp(0, maxVerticalMoveRange, scale);

            position.x = Mathf.Clamp(position.x, originalPosition.x - horizontalMoveRange, originalPosition.x + horizontalMoveRange);
            position.y = Mathf.Clamp(position.y, originalPosition.y - verticalMoveRange, originalPosition.y + verticalMoveRange);
            return position;
        }

        Vector3 ClampScale(Vector3 scale)
        {
            scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
            scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
            scale.z = 1f; // z 값 고정
            return scale;
        }
        #endregion
    }
}
