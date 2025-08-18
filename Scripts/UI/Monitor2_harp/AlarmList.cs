using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlarmList : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    // 기존 코드와 동일하게 유지
    private List<LogData> list; // 알람 데이터 목록

    public AlarmListItem itemPrefab;
    public GameObject itemContainer;     // List_Group (Vertical Layout Group)

    public TMP_Dropdown dropdownMap;
    public TMP_Dropdown dropdownStatus;

    private int areaIndex = -1; // 선택된 지역 인덱스
    private int statusIndex = -1; // 선택된 상태 인덱스

    // --------- 페이징 UI 연결 ----------
    [Header("Pagination UI")]
    public Button btnFirst;      // <<
    public Button btnPrev;       // <
    public Transform pageNumbersContainer; // 페이지 번호 버튼들이 들어갈 컨테이너
    public Button pageNumberButtonPrefab; // 페이지 번호 버튼 프리팹
    public Button btnNext;       // >
    public Button btnLast;       // >>

    [Header("Paging Settings")]
    [Min(1)] public int pageSize = 15;  // 한 페이지 행 수

    [Header("Page Button Colors")]
    public Color normalPageColor = Color.white;
    public Color selectedPageColor = Color.cyan;
    public Color normalTextColor = Color.black;
    public Color selectedTextColor = Color.white;

    // --------- 페이징 내부 상태 ----------
    private readonly List<AlarmListItem> _pool = new();  // 행 풀(최대 pageSize)
    private readonly List<Button> _pageButtons = new();  // 페이지 번호 버튼들
    private int _currentPage = 1;                        // 1-base
    private int TotalCount => (list == null) ? 0 : list.Count;
    private int TotalPages => Mathf.Max(1, Mathf.CeilToInt(TotalCount / (float)pageSize));

    #region 버튼안씀
    private Button btnTimeAsc;
    private Button btnTimeDesc;
    private Button btnContentAsc;
    private Button btnContentDesc;
    private Button btnAreaAsc;
    private Button btnAreaDesc;
    private Button btnObsAsc;
    private Button btnObsDesc;
    private Button btnStatusAsc;
    private Button btnStatusDesc;
    #endregion

    void Start()
    {
        this.itemPrefab.gameObject.SetActive(false);

        // 페이지 번호 버튼 프리팹 비활성화
        if (pageNumberButtonPrefab != null)
            pageNumberButtonPrefab.gameObject.SetActive(false);

        // 알람 리스트 변경 이벤트 구독
        UiManager.Instance.Register(UiEventType.Initiate, OnUpdateAlarmList);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnUpdateAlarmList);

        // 드롭다운 이벤트 연결
        dropdownStatus.onValueChanged.AddListener(OnStatusFilterChanged);
        dropdownMap.onValueChanged.AddListener(OnAreaFilterChanged);

        // --- 페이징 UI 이벤트 바인딩 ---
        if (btnFirst) btnFirst.onClick.AddListener(() => GoPage(1));
        if (btnPrev) btnPrev.onClick.AddListener(() => GoPage(_currentPage - 1));
        if (btnNext) btnNext.onClick.AddListener(() => GoPage(_currentPage + 1));
        if (btnLast) btnLast.onClick.AddListener(() => GoPage(TotalPages));

        // 초기 풀 준비 (pageSize 기준)
        EnsurePool();
    }

    #region 페이징 코드 
    // pageSize 만큼만 셀 풀을 준비 (초과분은 비활성)
    private void EnsurePool()
    {
        if (itemPrefab == null || itemContainer == null) return;

        while (_pool.Count < pageSize)
        {
            var cell = Instantiate(itemPrefab, itemContainer.transform);
            cell.gameObject.SetActive(false);
            _pool.Add(cell);
        }
        for (int i = 0; i < _pool.Count; i++)
            _pool[i].gameObject.SetActive(i < pageSize ? false : false); // 초기에는 렌더에서 활성화
    }

    private void RenderPage()
    {
        // 현재 페이지 범위
        int start = (_currentPage - 1) * pageSize;
        int end = Mathf.Min(start + pageSize, TotalCount);
        int count = Mathf.Max(0, end - start);

        // 바인딩
        for (int i = 0; i < pageSize; i++)
        {
            var cell = _pool[i];
            if (i < count)
            {
                var data = list[start + i];     // NOTE: list는 기존 클래스의 List<LogData>
                cell.gameObject.SetActive(true);
                cell.SetText(data);             // 기존 AlarmListItem API 유지
            }
            else
            {
                cell.gameObject.SetActive(false);
            }
        }

        // 컨테이너 높이는 페이지 크기 기준(행 높이 * 노출 행수)로 고정(선택)
        var rt = itemContainer.GetComponent<RectTransform>();
        if (rt != null)
        {
            var cellH = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, pageSize * cellH);
        }

        UpdatePageButtons();

        // 버튼 상태 동기화
        if (btnFirst) btnFirst.interactable = _currentPage > 1;
        if (btnPrev) btnPrev.interactable = _currentPage > 1;
        if (btnNext) btnNext.interactable = _currentPage < TotalPages;
        if (btnLast) btnLast.interactable = _currentPage < TotalPages;
    }

    private void UpdatePageButtons()
    {
        if (pageNumbersContainer == null || pageNumberButtonPrefab == null) return;

        // 기존 페이지 버튼들 정리
        foreach (var btn in _pageButtons)
        {
            if (btn != null) DestroyImmediate(btn.gameObject);
        }
        _pageButtons.Clear();

        // [1] [2] [3] [...] [총페이지] 형태로 생성
        CreateSimplePageButtons();
    }

    private void CreateSimplePageButtons()
    {
        // 총 페이지가 4개 이하면 모두 표시
        if (TotalPages <= 4)
        {
            for (int i = 1; i <= TotalPages; i++)
            {
                CreatePageButton(i);
            }
            return;
        }

        // 4개 이상인 경우: [1] [2] [3] [...] [총페이지]
        CreatePageButton(1);
        CreatePageButton(2);
        CreatePageButton(3);
        CreateEllipsisButton();
        CreatePageButton(TotalPages);
    }

    private void CreatePageButton(int pageNumber)
    {
        var btnObj = Instantiate(pageNumberButtonPrefab.gameObject, pageNumbersContainer);
        btnObj.SetActive(true);

        var btn = btnObj.GetComponent<Button>();
        var txt = btnObj.GetComponentInChildren<TMP_Text>();

        if (txt != null)
        {
            txt.text = pageNumber.ToString();
        }

        // 현재 페이지 스타일링
        bool isCurrentPage = pageNumber == _currentPage;
        var btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = isCurrentPage ? selectedPageColor : normalPageColor;
        }
        if (txt != null)
        {
            txt.color = isCurrentPage ? selectedTextColor : normalTextColor;
        }

        // 버튼 클릭 이벤트
        int page = pageNumber; // 클로저용 지역변수
        btn.onClick.AddListener(() => GoPage(page));

        _pageButtons.Add(btn);
    }

    private void CreateEllipsisButton()
    {
        var btnObj = Instantiate(pageNumberButtonPrefab.gameObject, pageNumbersContainer);
        btnObj.SetActive(true);

        var btn = btnObj.GetComponent<Button>();
        var txt = btnObj.GetComponentInChildren<TMP_Text>();

        if (txt != null)
        {
            txt.text = "...";
        }

        // 점점점 버튼은 클릭 불가
        btn.interactable = false;
        
        // 스타일링
        var btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = normalPageColor;
        }
        if (txt != null)
        {
            txt.color = normalTextColor;
        }

        _pageButtons.Add(btn);
    }

    private void GoPage(int page)
    {
        _currentPage = Mathf.Clamp(page, 1, TotalPages);
        RenderPage();
    }

    #endregion

    // 알람 리스트 업데이트 이벤트 핸들러
    private void OnUpdateAlarmList(object data)
    {
        List<LogData> logs = modelProvider.GetAlarmsForDisplay();
        list = logs;
        // 받아온 데이터의 시간 확인
        Debug.Log("=== DB에서 받아온 원본 데이터 시간 ===");
        for (int i = 0; i < Math.Min(5, logs.Count); i++)
        {
            Debug.Log($"{i}: {logs[i].time:yyyy-MM-dd HH:mm:ss} - {logs[i].hnsName}");
        }

        this.list.Sort((a, b) => b.time.CompareTo(a.time));

        // 드롭다운에 지역명 옵션 자동 추가
        var areaNames = logs.Select(log => log.areaName).Distinct().ToList();
        areaNames.Insert(0, "전체");
        dropdownMap.ClearOptions();
        dropdownMap.AddOptions(areaNames);

        // 드롭다운에 상태 옵션 추가 (정수 → 텍스트 매핑)
        var statusOptions = new List<string> { "전체", "설비이상", "경고", "경계" };
        dropdownStatus.ClearOptions();
        dropdownStatus.AddOptions(statusOptions);

        _currentPage = 1;
        EnsurePool();
        RenderPage();
    }

    private List<LogData> GetFilteredAlarms()
    {
        List<LogData> alarms = modelProvider.GetAlarmsForDisplay();

        // 지역 필터링
        if (areaIndex > 0)
        {
            string selectedAreaName = dropdownMap.options[areaIndex].text;
            alarms = alarms.Where(a => a.areaName == selectedAreaName).ToList();
        }
        // 상태 필터링
        if (statusIndex >= 0)
        {
            alarms = alarms.Where(a => a.status == statusIndex).ToList();
        }

        return alarms;
    }

    // 필터 적용 메서드
    private void ApplyFilters()
    {
        list = GetFilteredAlarms();
        _currentPage = 1;
        RenderPage();
    }

    // 알람 필터링 (드롭다운 메뉴에 연결)
    public void OnAreaFilterChanged(int index)
    {
        this.areaIndex = index;
        ApplyFilters();
    }

    public void OnStatusFilterChanged(int index)
    {
        this.statusIndex = index;
        ApplyFilters();
    }

    // UI 토글 기능도 포함한 완전한 정렬 메서드
    public void OnClickTimeUp()
    {
        // UI 토글
        GameObject.Find("Image_UP").SetActive(true);
        GameObject.Find("Image_DOWN").SetActive(false);

        // 실제 정렬 실행
        OnClickOrder(AlramOrder.TIME_UP.ToString());
    }

    public void OnClickOrder(string order)
    {
        // 정렬 전 DateTime 값들 확인
        for (int i = 0; i < Math.Min(3, list.Count); i++)
        {
            // Debug.Log($"{i}: {list[i].time:yyyy-MM-dd HH:mm:ss} - {list[i].hnsName}");
        }

        this.list.Sort((a, b) =>
        {
            if (order == AlramOrder.TIME_UP.ToString())
            {
                return b.time.CompareTo(a.time);  // 최신이 위로 오도록
            }
            else if (order == AlramOrder.TIME_DOWN.ToString())
            {
                return a.time.CompareTo(b.time);  // 과거가 위로 오도록
            }
            else if (order == AlramOrder.STATUS_UP.ToString())
            {
                return a.status.CompareTo(b.status);  // 심각한 순서: 설비이상→경보→경계 (0→1→2)
            }
            else if (order == AlramOrder.STATUS_DOWN.ToString())
            {
                return b.status.CompareTo(a.status);  // 낮은 순서: 경계→경보→설비이상 (2→1→0)
            }
            else if (order == AlramOrder.MAP_UP.ToString())
            {
                return b.areaName.CompareTo(a.areaName);  // Z→A 순서
            }
            else if (order == AlramOrder.MAP_DOWN.ToString())
            {
                return a.areaName.CompareTo(b.areaName);  // A→Z 순서
            }
            else if (order == AlramOrder.AREA_UP.ToString())
            {
                return b.obsName.CompareTo(a.obsName);   // Z→A 순서
            }
            else if (order == AlramOrder.AREA_DOWN.ToString())
            {
                return a.obsName.CompareTo(b.obsName);   // A→Z 순서
            }
            return 0;
        });

        // 정렬 후 DateTime 값들 확인
        Debug.Log("=== 정렬 후 시간 확인 ===");
        for (int i = 0; i < Math.Min(5, list.Count); i++)
        {
            Debug.Log($"{i}: {list[i].time:yyyy-MM-dd HH:mm:ss} - {list[i].hnsName}");
        }

        _currentPage = 1; // 정렬 시 1페이지로 복귀
        RenderPage();
    }

    public enum AlramOrder
    {
        TIME_UP,
        TIME_DOWN,
        STATUS_UP,
        STATUS_DOWN,
        MAP_UP,
        MAP_DOWN,
        AREA_UP,
        AREA_DOWN,
        VALUE_UP,
        VALUE_DOWN
    };
}