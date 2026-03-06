using legacy;
using Onthesys;
using UMP;
using UnityEngine;

public class PanelVideoCCTV : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    UniversalMediaPlayer videoPlayerA;
    UniversalMediaPlayer videoPlayerB;

    // ⭐ 버튼 참조 추가
    private GameObject btnPlayA;
    private GameObject btnPauseA;
    private GameObject btnPlayB;
    private GameObject btnPauseB;

    private int currentObsId = -1;

    private void Awake()
    {
        videoPlayerA = transform.Find("Video_Player A")?.GetComponentInChildren<UniversalMediaPlayer>();
        videoPlayerB = transform.Find("Video_Player B")?.GetComponentInChildren<UniversalMediaPlayer>();

        // ⭐ 버튼 찾기 (경로 확인 필요!)
        var buttonsA = transform.Find("Video_Player A/Buttons");
        if (buttonsA != null)
        {
            btnPlayA = buttonsA.Find("Btn_Play")?.gameObject;
            btnPauseA = buttonsA.Find("Btn_Pause")?.gameObject;
        }

        var buttonsB = transform.Find("Video_Player B/Buttons");
        if (buttonsB != null)
        {
            btnPlayB = buttonsB.Find("Btn_Play")?.gameObject;
            btnPauseB = buttonsB.Find("Btn_Pause")?.gameObject;
        }

        if (videoPlayerA == null) Debug.LogError("[PanelVideoCCTV] Video_Player A를 찾을 수 없습니다!");
        if (videoPlayerB == null) Debug.LogError("[PanelVideoCCTV] Video_Player B를 찾을 수 없습니다!");
    }

    // ⭐⭐⭐ OnEnable: 버튼 상태만 초기화 (Path는 건드리지 않음!)
    private void OnEnable()
    {
        Debug.Log("[PanelVideoCCTV] OnEnable - 버튼 상태 초기화");
        ResetButtonStates();
    }

    // ⭐ OnDisable: 비디오 정지
    private void OnDisable()
    {
        Debug.Log("[PanelVideoCCTV] OnDisable - 비디오 정지");

        if (videoPlayerA != null)
            videoPlayerA.Stop();

        if (videoPlayerB != null)
            videoPlayerB.Stop();
    }

    // ⭐ 버튼 상태만 초기화
    private void ResetButtonStates()
    {
        if (btnPlayA != null) btnPlayA.SetActive(true);
        if (btnPauseA != null) btnPauseA.SetActive(false);
        if (btnPlayB != null) btnPlayB.SetActive(true);
        if (btnPauseB != null) btnPauseB.SetActive(false);

        Debug.Log("[PanelVideoCCTV] 버튼 상태 초기화 완료");
    }

    public void SetObservatory(int obsId)
    {
        if (obsId <= 0)
        {
            Debug.LogWarning($"[PanelVideoCCTV] 유효하지 않은 관측소 ID: {obsId}");
            return;
        }

        currentObsId = obsId;

        // ⭐ 여기서 버튼 초기화 (SetObservatory 호출 시마다)
        ResetButtonStates();

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

        if (videoPlayerA != null && !string.IsNullOrEmpty(obs.src_video1))
        {
            videoPlayerA.Path = obs.src_video1;
            Debug.Log($"[PanelVideoCCTV] Video A URL: {obs.src_video1}");
        }

        if (videoPlayerB != null && !string.IsNullOrEmpty(obs.src_video2))
        {
            videoPlayerB.Path = obs.src_video2;
            Debug.Log($"[PanelVideoCCTV] Video B URL: {obs.src_video2}");
        }

        Debug.Log($"[PanelVideoCCTV] ===== CCTV 로딩 완료 =====");
    }
}