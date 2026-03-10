using Assets.Scripts.UI.Reform.PageHome;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHome
{
    /// <summary>
    /// 좌하단 맵 컨트롤 패널
    /// - 나침반 버튼(N/S/W/E): PannableRegionMap localPosition 이동
    /// - ISO View / Top View 버튼: 추후 구현
    /// </summary>
    public class MapControlPanel : MonoBehaviour
    {
        #region [이동 설정]
        public float moveAmount = 100f;
        public float moveDuration = 0.2f;
        #endregion

        #region [내부 참조]
        PannableRegionMap regionMap;
        #endregion

        #region [Unity]
        private void Start()
        {
            regionMap = transform.parent.GetComponentInChildren<PannableRegionMap>();

            transform.Find("CompassPanel/BtnNorth").GetComponent<Button>().onClick.AddListener(() => MoveMap(Vector3.down));
            transform.Find("CompassPanel/BtnSouth").GetComponent<Button>().onClick.AddListener(() => MoveMap(Vector3.up));
            transform.Find("CompassPanel/BtnWest").GetComponent<Button>().onClick.AddListener(() => MoveMap(Vector3.right));
            transform.Find("CompassPanel/BtnEast").GetComponent<Button>().onClick.AddListener(() => MoveMap(Vector3.left));
        }
        #endregion

        #region [내부 인터페이스]
        void MoveMap(Vector3 direction)
        {
            if (regionMap == null) return;

            RectTransform rt = regionMap.GetComponent<RectTransform>();
            Vector3 target = regionMap.ClampPosition(rt.localPosition + direction * moveAmount);
            rt.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad);
        }
        #endregion
    }
}