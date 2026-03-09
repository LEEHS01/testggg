using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHistory
{
    internal class ViewAlarmList : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        // 기존 코드와 동일하게 유지
        GameObject prefab => Resources.Load<GameObject>("Reform/PageHistory/ViewAlarmItem");
        public GameObject itemContainer;     // List_Group (Vertical Layout Group)

        public TMP_Dropdown dropdownGroup;
        public TMP_Dropdown dropdownObs;

        private int groupIndex = -1; // 선택된 지역 인덱스
        private int obsIndex = -1; // 선택된 관측소 인덱스

        // --------- 페이징 UI 연결 ----------
        [Header("Pagination UI")]
        public Button btnFirst;      // <<
        public Button btnPrev;       // <
        public Transform pageNumbersContainer; // 페이지 번호 버튼들이 들어갈 컨테이너
        public Button pageNumberButtonPrefab; // 페이지 번호 버튼 프리팹
        public Button btnNext;       // >
        public Button btnLast;       // >>

        [Header("Paging Settings")]
        [Min(1)] public int pageSize = 12;  // 한 페이지 행 수

        [Header("Page Button Colors")]
        public Color normalPageColor = Color.white;
        public Color selectedPageColor = Color.cyan;
        public Color normalTextColor = Color.black;
        public Color selectedTextColor = Color.white;

        // --------- 페이징 내부 상태 ----------
        private readonly List<ViewAlarmItem> _pool = new();  // 행 풀(최대 pageSize)
        private readonly List<Button> _pageButtons = new();  // 페이지 번호 버튼들
        private int _currentPage = 1;                        // 1-base
        private int TotalCount => (alarmWhole == null) ? 0 : alarmWhole.Count;
        private int TotalPages => Mathf.Max(1, Mathf.CeilToInt(TotalCount / (float)pageSize));


        private List<AlarmInfo> alarmWhole; // 알람 데이터 목록
        private List<ObservatoryInfo> obss; // 관측소 데이터 목록
        private List<GroupInfo> groups; // 그룹 데이터 목록

        void Start()
        {
            dropdownGroup = transform.Find("SubTitle").Find("DropdownGroup").GetComponent<TMP_Dropdown>();
            dropdownObs = transform.Find("SubTitle").Find("DropdownObs").GetComponent<TMP_Dropdown>();
            itemContainer = transform.Find("ListViewport").Find("List_Group").gameObject;

            btnFirst = transform.Find("PaginationBar").Find("Btn_First").GetComponent<Button>();
            btnPrev = transform.Find("PaginationBar").Find("Btn_Prev").GetComponent<Button>();
            pageNumbersContainer = transform.Find("PaginationBar").Find("PageNumbers_Container");
            pageNumberButtonPrefab = pageNumbersContainer.Find("Button").GetComponent<Button>();
            btnNext = transform.Find("PaginationBar").Find("Btn_Next").GetComponent<Button>();
            btnLast = transform.Find("PaginationBar").Find("Btn_Last").GetComponent<Button>();


            // 페이지 번호 버튼 프리팹 비활성화
            if (pageNumberButtonPrefab != null)
                pageNumberButtonPrefab.gameObject.SetActive(false);

            // 알람 리스트 변경 이벤트 구독
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.Initiate, OnUpdateAlarmList);
            UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnUpdateAlarmList);

            // 드롭다운 이벤트 연결
            dropdownObs.onValueChanged.AddListener(OnObsFilterChanged);
            dropdownGroup.onValueChanged.AddListener(OnAreaFilterChanged);

            // --- 페이징 UI 이벤트 바인딩 ---
            if (btnFirst) btnFirst.onClick.AddListener(() => GoPage(1));
            if (btnPrev) btnPrev.onClick.AddListener(() => GoPage(_currentPage - 1));
            if (btnNext) btnNext.onClick.AddListener(() => GoPage(_currentPage + 1));
            if (btnLast) btnLast.onClick.AddListener(() => GoPage(TotalPages));

            // 초기 풀 준비 (pageSize 기준)
            EnsurePool();
        }

        private void OnInitiate(object obj)
        {
            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
        }


        #region 페이징 코드 
        // pageSize 만큼만 셀 풀을 준비 (초과분은 비활성)
        private void EnsurePool()
        {
            if (prefab == null || itemContainer == null) return;

            while (_pool.Count < pageSize)
            {
                var cell = Instantiate(prefab, itemContainer.transform);
                cell.gameObject.SetActive(false);
                _pool.Add(cell.GetComponent<ViewAlarmItem>());
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
                    var alarm = alarmWhole[start + i];     // NOTE: list는 기존 클래스의 List<LogData>
                    cell.gameObject.SetActive(true);

                    var sensor = obss.Find(obs => obs.obsIdx == alarm.obsIdx)?.sensors.FirstOrDefault(s => s.idx == alarm.sensorIdx);
                    //obss.Find(obs => obs.obsIdx == alarm.obsIdx)?.sensors.ForEach(s => Debug.Log($"OBS_IDX: {alarm.obsIdx} >>> {s.idx} {s.info}"));

                    if (sensor == null) throw new Exception($"Sensor not found for OBS_IDX: {alarm.obsIdx}, SENSOR_IDX: {alarm.sensorIdx}");

                    cell.SetValue(alarm, sensor.Value.info);             
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
                var cellH = prefab.GetComponent<RectTransform>().sizeDelta.y;
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

            // 동적 페이징 버튼 생성
            CreateDynamicPageButtons();
        }

        private void CreateDynamicPageButtons()
        {
            int maxVisibleButtons = 7; // 최대 표시할 버튼 수 (... 포함)

            // 총 페이지가 maxVisibleButtons 이하면 모두 표시
            if (TotalPages <= maxVisibleButtons)
            {
                for (int i = 1; i <= TotalPages; i++)
                {
                    CreatePageButton(i);
                }
                return;
            }

            // 동적 페이징 로직
            List<int> pagesToShow = new List<int>();

            // 항상 1페이지는 표시
            pagesToShow.Add(1);

            // 현재 페이지 근처 표시 로직
            if (_currentPage <= 4)
            {
                // 시작 부분: [1] [2] [3] [4] [5] [...] [마지막]
                for (int i = 2; i <= 5; i++)
                {
                    if (i < TotalPages) pagesToShow.Add(i);
                }

                // ... 추가
                pagesToShow.Add(-1); // -1은 ... 표시용

                // 마지막 페이지
                pagesToShow.Add(TotalPages);
            }
            else if (_currentPage >= TotalPages - 3)
            {
                // 끝 부분: [1] [...] [끝-4] [끝-3] [끝-2] [끝-1] [끝]

                // ... 추가
                pagesToShow.Add(-1);

                for (int i = TotalPages - 4; i <= TotalPages; i++)
                {
                    if (i > 1) pagesToShow.Add(i);
                }
            }
            else
            {
                // 중간 부분: [1] [...] [현재-1] [현재] [현재+1] [...] [마지막]

                // 첫 번째 ...
                pagesToShow.Add(-1);

                // 현재 페이지 주변
                for (int i = _currentPage - 1; i <= _currentPage + 1; i++)
                {
                    pagesToShow.Add(i);
                }

                // 두 번째 ...
                pagesToShow.Add(-1);

                // 마지막 페이지
                pagesToShow.Add(TotalPages);
            }

            // 버튼 생성
            foreach (int pageNum in pagesToShow)
            {
                if (pageNum == -1)
                {
                    CreateEllipsisButton();
                }
                else
                {
                    CreatePageButton(pageNum);
                }
            }
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
            List<AlarmInfo> alarms = modelProvider.GetAlarmsWhole();
            alarmWhole = alarms;
            //// 받아온 데이터의 시간 확인
            //Debug.Log("=== DB에서 받아온 원본 데이터 시간 ===");
            //for (int i = 0; i < Math.Min(5, logs.Count); i++)
            //{
            //    Debug.Log($"{i}: {logs[i].time:yyyy-MM-dd HH:mm:ss} - {logs[i].hnsName}");
            //}

            this.alarmWhole.Sort((a, b) => b.occured.timestamp.CompareTo(a.occured.timestamp));

            // 드롭다운에 지역명 옵션 자동 추가
            //로그 데이터 => 그룹명 추출
            //var areaNames = alarms.Select(
            //    alarm => groups.Find(
            //        group => group.groupIdx == obss.Find(
            //            obs => obs.obsIdx == alarm.obsIdx
            //        ).groupIdx
            //    ).groupName
            //).Distinct().ToList();
            // 알람 필요 없지 않나?

            var groupNames = groups.Select(group => group.groupName).ToList();

            groupNames.Insert(0, "전체");  // 항상 "전체" 옵션을 맨 앞에 추가
            groupNames.Insert(1, "무소속"); // "무소속" 옵션을 "전체" 다음에 추가
            dropdownGroup.ClearOptions();   
            dropdownGroup.AddOptions(groupNames);

            // 드롭다운에 관측소 옵션 추가
            var obsOptions = new List<string>(); 

            if (new int[] { -1, 0, 1 }.Contains(groupIndex) == false)
            {
                obsOptions = 
                    obss.Where(obs => groups.Find(group => group.groupIdx == obs.groupIdx).groupName == dropdownGroup.options[groupIndex].text)
                    .Select(obs => obs.nameText)
                    .ToList();
            }
            else if (groupIndex == 1) // "무소속" 선택 시
            { 
                obsOptions = obss.Where(obs => obs.groupIdx.HasValue == false || obs.groupIdx.Value < 1)
                    .Select(obs => obs.nameText)
                    .ToList();

            }
            else if (groupIndex == 0) // "전체" 선택 시
            {
                obsOptions = obss
                    .Select(obs => obs.nameText)
                    .ToList();
            }

            obsOptions.Insert(0, "전체");   // 항상 "전체" 옵션을 맨 앞에 추가
            dropdownObs.ClearOptions();
            dropdownObs.AddOptions(obsOptions);

            _currentPage = 1;
            EnsurePool();
            RenderPage();
        }

        private List<AlarmInfo> GetFilteredAlarms()
        {
            List<AlarmInfo> alarms = modelProvider.GetAlarmsWhole();

            // 관측소 필터링이 없다면...
            if (new int[] { 0 }.Contains(obsIndex) == true)
            {
                // 지역 필터링
                if (new int[] { 0, 1 }.Contains(groupIndex) == false)
                {
                    string selectedAreaName = dropdownGroup.options[groupIndex].text;
                    alarms = alarms.Where(alarm => groups.Find(
                        group => group.groupIdx == obss.Find(
                            obs => obs.obsIdx == alarm.obsIdx
                        ).groupIdx
                    ).groupName == selectedAreaName).ToList();
                }
                // "무소속" 선택 시
                else if (groupIndex == 1) 
                {
                    alarms = alarms.Where(alarm =>
                    {
                        var obs = obss.Find(
                            obs => obs.obsIdx == alarm.obsIdx
                        );

                        return obs.groupIdx.HasValue == false || obs.groupIdx.Value < 1;

                    }).ToList();
                }
                // "전체" 선택 시
                else if (groupIndex == 0) 
                {
                    alarms = alarms.ToList();   //가공 없음
                }
            }
            // 관측소 필터링 (관측소가 지역의 하위 항목이기 때문에, 관측소 필터가 필요하다면 지역은 스킵 가능)
            else
            {
                alarms = alarms.Where(alarm =>
                    obss.Find(
                        obs => obs.obsIdx == alarm.obsIdx
                    ).nameText == dropdownObs.options[obsIndex].text
                ).ToList();
            }



            return alarms;
        }

        // 필터 적용 메서드
        private void ApplyFilters()
        {
            alarmWhole = GetFilteredAlarms();
            _currentPage = 1;
            RenderPage();
        }

        // 알람 필터링 (드롭다운 메뉴에 연결)
        public void OnAreaFilterChanged(int index)
        {
            if (this.groupIndex == index) return; // 변경된 인덱스가 현재와 같으면 무시
            this.groupIndex = index;
            this.obsIndex = 0;

            //OnUpdateAlarmList의 일부분을 그대로 사용
            {
                // 드롭다운에 관측소 옵션 추가
                var obsOptions = new List<string>(); // { "전체", "설비이상", "경고", "경계" };

                if (new int[] { -1, 0, 1 }.Contains(groupIndex) == false)
                {
                    obsOptions =
                        obss.Where(obs => groups.Find(group => group.groupIdx == obs.groupIdx).groupName == dropdownGroup.options[groupIndex].text)
                        .Select(obs => obs.nameText)
                        .ToList();
                }
                else if (groupIndex == 1) // "무소속" 선택 시
                {
                    obsOptions = obss.Where(obs => obs.groupIdx.HasValue == false || obs.groupIdx.Value < 1)
                        .Select(obs => obs.nameText)
                        .ToList();

                }
                else if (groupIndex == 0) // "전체" 선택 시
                {
                    obsOptions = obss
                        .Select(obs => obs.nameText)
                        .ToList();
                }

                dropdownObs.ClearOptions();
                dropdownObs.AddOptions(new List<string> { "전체" });   // 항상 "전체" 옵션을 맨 앞에 추가
                dropdownObs.AddOptions(obsOptions);
            }


            dropdownObs.SetValueWithoutNotify(0);// 관측소 드롭다운을 "전체"로 초기화
            ApplyFilters();
        }

        public void OnObsFilterChanged(int index)
        {
            if (this.obsIndex == index) return; // 변경된 인덱스가 현재와 같으면 무시
            this.obsIndex = index;

            ApplyFilters();
        }



    }
}
