using Onthesys;
using UMP;
using UnityEngine;

public class PanelVideoCCTV : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    UniversalMediaPlayer videoPlayerA;
    UniversalMediaPlayer videoPlayerB;

    private int currentObsId = -1;

    private void Awake()
    {
        videoPlayerA = transform.Find("Video_Player A")?.GetComponentInChildren<UniversalMediaPlayer>();
        videoPlayerB = transform.Find("Video_Player B")?.GetComponentInChildren<UniversalMediaPlayer>();

        if (videoPlayerA == null) Debug.LogError("[PanelVideoCCTV] Video_Player A를 찾을 수 없습니다!");
        if (videoPlayerB == null) Debug.LogError("[PanelVideoCCTV] Video_Player B를 찾을 수 없습니다!");
    }

    public void SetObservatory(int obsId)
    {
        if (obsId <= 0)
        {
            Debug.LogWarning($"[PanelVideoCCTV] 유효하지 않은 관측소 ID: {obsId}");
            return;
        }

        currentObsId = obsId;
        LoadCCTV(obsId);
    }

    private void LoadCCTV(int obsId)
    {
        ObsData obs = modelProvider.GetObs(obsId);

        if (obs == null)
        {
            Debug.LogError($"[PanelVideoCCTV] 관측소 {obsId}를 찾을 수 없습니다!");
            return;
        }

        Debug.Log($"[PanelVideoCCTV] ===== CCTV 로딩 시작 =====");
        Debug.Log($"[PanelVideoCCTV] 관측소: {obs.obsName} (ID: {obsId})");
        Debug.Log($"[PanelVideoCCTV] Video A URL: {obs.src_video1}");
        Debug.Log($"[PanelVideoCCTV] Video B URL: {obs.src_video2}");

        if (videoPlayerA != null)
        {
            if (!string.IsNullOrEmpty(obs.src_video1))
            {
                Debug.Log($"[PanelVideoCCTV] Video_Player A에 URL 설정 중...");
                Debug.Log($"[PanelVideoCCTV] URL: {obs.src_video1}");

                videoPlayerA.Path = obs.src_video1;
                videoPlayerA.Prepare();

                Debug.Log($"[PanelVideoCCTV] ✅ Video_Player A Prepare 완료");
            }
            else
            {
                Debug.LogWarning($"[PanelVideoCCTV] Video A URL이 비어있습니다!");
            }
        }
        else
        {
            Debug.LogError($"[PanelVideoCCTV] videoPlayerA가 null입니다!");
        }

        // Video_Player B
        if (videoPlayerB != null)
        {
            if (!string.IsNullOrEmpty(obs.src_video2))
            {
                Debug.Log($"[PanelVideoCCTV] Video_Player B에 URL 설정 중...");
                Debug.Log($"[PanelVideoCCTV] URL: {obs.src_video2}");

                videoPlayerB.Path = obs.src_video2;
                videoPlayerB.Prepare();

                Debug.Log($"[PanelVideoCCTV] ✅ Video_Player B Prepare 완료");
            }
            else
            {
                Debug.LogWarning($"[PanelVideoCCTV] Video B URL이 비어있습니다!");
            }
        }
        else
        {
            Debug.LogError($"[PanelVideoCCTV] videoPlayerB가 null입니다!");
        }

        Debug.Log($"[PanelVideoCCTV] ===== CCTV 로딩 요청 완료 =====");
    }
}
