using Onthesys;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BtnCCTV : MonoBehaviour
{
    public GameObject panelVideo; // Inspector에서 Panel_Video 할당

    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClickCCTV);
    }

    void OnClickCCTV()
    {
        if (panelVideo == null)
        {
            Debug.LogError("[BtnCCTV] panelVideo가 할당되지 않았습니다!");
            return;
        }

        // 현재 선택된 관측소 ID 가져오기
        int currentObsId = modelProvider.GetCurrentObsId();

        if (currentObsId <= 0)
        {
            Debug.LogWarning("[BtnCCTV] 선택된 관측소가 없습니다!");
            // 필요시 에러 팝업 표시
            UiManager.Instance.Invoke(UiEventType.PopupErrorMonitorB, new Exception("관측소를 먼저 선택해주세요."));
            return;
        }

        // Panel_Video 활성화
        Debug.Log($"[BtnCCTV] Panel_Video 활성화 시도...");
        panelVideo.SetActive(true);

        // Panel_Video에 관측소 ID 전달
        var panelVideoCCTV = panelVideo.GetComponent<PanelVideoCCTV>();
        if (panelVideoCCTV != null)
        {
            Debug.Log($"[BtnCCTV] PanelVideoCCTV.SetObservatory({currentObsId}) 호출");
            panelVideoCCTV.SetObservatory(currentObsId);
            Debug.Log($"[BtnCCTV] ✅ Panel_Video 활성화 완료");
        }
        else
        {
            Debug.LogError("[BtnCCTV] PanelVideoCCTV 컴포넌트를 찾을 수 없습니다!");
        }
        Debug.Log("[BtnCCTV] ===== CCTV 버튼 처리 완료 =====");
    }
}