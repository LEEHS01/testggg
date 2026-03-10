using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using DG.Tweening;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    public class PannableObservatoryMap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        //기타 참조
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageRegion/MarkerObservatoryMap");

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //UI 오브젝트 참조
        RectTransform panelRectTransform => GetComponent<RectTransform>();
        RectTransform crosshairRectTransform;
        Image imgBackground;
        List<MarkerObservatoryMap> obsMarkers = new List<MarkerObservatoryMap>();

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
            //markers = transform.Find("MarkerList").GetComponentsInChildren<MarkerRegionMap>(true).ToList();
            imgBackground = transform.Find("MapNationBackground").GetComponent<Image>();
            crosshairRectTransform = transform.parent.Find("CrosshairObservatory").GetComponent<RectTransform>();

            transform.Find("MapNationBackground").Find("PosRefPoint_1").TryGetComponent(out coordinateAnchor.first);
            transform.Find("MapNationBackground").Find("PosRefPoint_2").TryGetComponent(out coordinateAnchor.second);

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
        }


        private void OnInitiate(object obj)
        {
            List<ObservatoryInfo> obss = modelProvider.GetObss();
            List<GroupInfo> groups = modelProvider.GetGroups();

            //그룹이 없는 관측소도 포함
            groups.Add(null);

            //우선 그룹에 알맞는 관측소 매핑
            List<(GroupInfo groupInfo, List<ObservatoryInfo> obsList)> groupedObsList;
            groupedObsList = (List<(GroupInfo, List<ObservatoryInfo>)>)groups.Select(
                groupInfo => {
                    var list = obss.Where(obs => (groupInfo?.groupIdx ?? null) == obs.groupIdx).ToList();
                    return (groupInfo, list);
                }).ToList();

            Transform markerContainer = transform.Find("MapNationBackground").Find("MarkerList");
            groupedObsList.ForEach(groupedObs =>
            {
                groupedObs.obsList.ForEach(obs =>
                {
                    GameObject instant = Instantiate(itemPrefab, markerContainer);
                    Vector2 coord = new Vector2(obs.coordinate.X, obs.coordinate.Y);

                    float scale = transform.Find("MapNationBackground").GetComponent<RectTransform>().lossyScale.x;
                    Vector2 consult = CoordinateToLocalPosition(coord);
                    instant.GetComponent<RectTransform>().localPosition = new Vector3(consult.x, consult.y, 0f) * scale;
                    instant.GetComponent<MarkerObservatoryMap>().SetValue(groupedObs.groupInfo, obs);
                    //instant.GetComponent<Button>().onClick.AddListener();

                    

                    obsMarkers.Add(instant.GetComponent<MarkerObservatoryMap>());
                });
            });

            UiManager.Instance.Register(UiEventType.SelectObs, n =>
            {
                OnEndDrag(null);
                //lockingJob = null;
            });
        }
        private void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) throw new Exception("Not allowed Type for Payload of this event");

            crosshairRectTransform.GetComponentsInChildren<Image>().ToList().
                ForEach(img => img.color = new(1, 1, 1, obsIdx != -1 ? 0.1f : 0.2f));


            //// 관측소 선택시 확대
            //Vector3 fromScale = GetComponent<RectTransform>().localScale;

            //DOTween.To(() => fromScale, x => fromScale = x, Vector3.one * 3, 0.5f).OnUpdate(() =>
            //{
            //    GetComponent<RectTransform>().localScale = fromScale;
            //});

            //int selObsIdx = modelProvider.GetCurrentObsIdx();
            //if (selObsIdx != -1)
            //{

            //    MarkerObservatoryMap marker = obsMarkers.Find(marker => marker.obsInfo.obsIdx == selObsIdx);

            //    Vector3 fromPos = GetComponent<RectTransform>().position;
            //    fromScale = GetComponent<RectTransform>().localScale;
            //    Vector3 toPos = crosshairRectTransform.position - marker.GetComponent<RectTransform>().position + fromPos;

            //    GetComponent<RectTransform>().position = toPos;
            //}
        }


        #region [Util]
        Vector2 CoordinateToLocalPosition(Vector2 coordinate)
        {
            var ancF = coordinateAnchor.first;
            var ancS = coordinateAnchor.second;
            Vector2 diffCoord = ancF.referenceCoordinate - ancS.referenceCoordinate;
            Vector2 diffPos = (ancF.GetComponent<RectTransform>().localPosition - ancS.GetComponent<RectTransform>().localPosition);

            Vector2 gradient = new Vector2(diffPos.x / diffCoord.x, diffPos.y / diffCoord.y);
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
        public void OnBeginDrag(PointerEventData eventData)
        {
            //UiManager.Instance.Invoke(UiEventType.SelectObs, -1);
            lockingJob?.Pause();
            lockingJob?.Kill();
            lockingJob = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!controlable) return;
            
            Vector3 newPos = panelRectTransform.localPosition + new Vector3(eventData.delta.x, eventData.delta.y, 0);
            newPos = ClampPosition(newPos);
            panelRectTransform.localPosition = newPos;

            var crosshairPos = crosshairRectTransform.position;
            int selObsIdx = modelProvider.GetCurrentObsIdx();
            bool isTargetExist = false;

            //기존에 지정된 마커가 존재할 떄, 유효성 검증
            if (selObsIdx >= 0)
            {
                var obsMarker = obsMarkers.Find(marker => marker.obsInfo.obsIdx == selObsIdx);
                var markerPos = obsMarker.GetComponent<RectTransform>().position;

                //새로운 마커 수용 가능하게 비움
                if ((crosshairPos - markerPos).magnitude >= 100f)
                    selObsIdx = -1;
            }


            obsMarkers.ForEach(obsMarker => {
                var markerPos = obsMarker.GetComponent<RectTransform>().position;

                //100 이내에 존재 (지정 조건 충족)
                if ((crosshairPos - markerPos).magnitude >= 100f) return;
                isTargetExist = true;

                //마커가 지정되지 않아(비어있어) 지정 가능할 때만 새 마커 수용
                if (selObsIdx != -1 ) return;

                //새 마커 수용 및 이벤트로 공지
                selObsIdx = obsMarker.obsInfo.obsIdx;
                UiManager.Instance.Invoke(UiEventType.SelectObs, obsMarker.obsInfo.obsIdx);
            });

            //Debug.Log("isTargetExist : " + isTargetExist + " selObsIdx : " + selObsIdx + " >>> " + (!isTargetExist && selObsIdx != -1));

            if (!isTargetExist && selObsIdx != -1 ||    //Lockable 타겟을 찾지 못했음에도 타겟이 없을 경우(없애도 될지도?)
                modelProvider.GetCurrentObsIdx() != -1 && selObsIdx == -1)  // 기존 Lock In 타겟을  Lost 했을 경우
                //UI 단계에까지 마커 타겟이 Lost'됐음'를 공지
                UiManager.Instance.Invoke(UiEventType.SelectObs, -1);

        }

        private Tween lockingJob = null;
        public void OnEndDrag(PointerEventData eventData)
        {
            int selObsIdx = modelProvider.GetCurrentObsIdx();
            if (selObsIdx != -1)
            {
                float duration = 0.5f;

                MarkerObservatoryMap marker = obsMarkers.Find(marker => marker.obsInfo.obsIdx == selObsIdx);

                Vector3 fromPos = GetComponent<RectTransform>().position;
                Vector3 fromScale = GetComponent<RectTransform>().localScale;
                Vector3 toPos = crosshairRectTransform.position - marker.GetComponent<RectTransform>().position + fromPos;

                if(lockingJob != null)
                    lockingJob.Kill();

                lockingJob = DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() =>
                {
                    GetComponent<RectTransform>().position = fromPos;
                });
                lockingJob.onComplete += () => lockingJob = null;


            }


        }

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


            int selObsIdx = modelProvider.GetCurrentObsIdx();
            if (selObsIdx != -1)
            {

                MarkerObservatoryMap marker = obsMarkers.Find(marker => marker.obsInfo.obsIdx == selObsIdx);

                Vector3 fromPos = GetComponent<RectTransform>().position;
                Vector3 fromScale = GetComponent<RectTransform>().localScale;
                Vector3 toPos = crosshairRectTransform.position - marker.GetComponent<RectTransform>().position + fromPos;

                GetComponent<RectTransform>().position = toPos;
            }
        }


        Vector3 ClampPosition(Vector3 position)
        {
            float scale = (panelRectTransform.localScale.x) / (maxScale);
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
