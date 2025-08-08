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
    public GameObject itemContainer;

    public List<AlarmListItem> items;
    public TMP_Dropdown dropdownMap;
    public TMP_Dropdown dropdownStatus;


    private int areaIndex = -1; // 선택된 지역 인덱스
    private int statusIndex = -1; // 선택된 상태 인덱스

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

        // 알람 리스트 변경 이벤트 구독
        UiManager.Instance.Register(UiEventType.Initiate, OnUpdateAlarmList);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnUpdateAlarmList);

        // 드롭다운 이벤트 연결
        dropdownStatus.onValueChanged.AddListener(OnStatusFilterChanged);
        dropdownMap.onValueChanged.AddListener(OnAreaFilterChanged);


    }



    // 알람 리스트 업데이트 이벤트 핸들러
    private void OnUpdateAlarmList(object data)
    {
        List<LogData> logs = modelProvider.GetAlarms();
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


        UpdateText();

    }

    private void UpdateText()
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            AlarmListItem item = this.items[i];

            if (item != itemPrefab) DestroyImmediate(this.items[i].gameObject);
        }
        this.items.Clear();

        float height = this.list.Count * this.itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
        for (int i = 0; i < this.list.Count; i++)
        {
            AlarmListItem item = Instantiate(this.itemPrefab);
            item.gameObject.SetActive(true);
            item.transform.SetParent(this.itemContainer.transform);
            item.SetText(this.list[i]);
            //item.GetComponent<Button>().onClick.AddListener(() => OnAlarmSelected(item.data.idx));
            this.items.Add(item);
        }
        this.itemContainer.GetComponent<RectTransform>().sizeDelta =
            new Vector2(this.itemContainer.GetComponent<RectTransform>().sizeDelta.x, height);



    }


    private List<LogData> GetFilteredAlarms()
    {
        List<LogData> alarms = modelProvider.GetAlarms();

        /*// 센서 사용여부 필터링 추가 (USEYN='0'인 센서의 알람 제외)
        alarms = alarms.Where(alarm =>
        {
            var toxin = modelProvider.GetToxin(alarm.boardId, alarm.hnsId);
            return toxin?.on ?? true; // 센서가 활성화된 경우만 표시
        }).ToList();
*/
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


    // 알람 필터링 (드롭다운 메뉴에 연결)
    public void OnAreaFilterChanged(int areaIndex)
    {
        this.areaIndex = areaIndex;
        list = GetFilteredAlarms(); // 필터링 반영
        UpdateText();
    }

    public void OnStatusFilterChanged(int statusIndex)
    {
        this.statusIndex = statusIndex - 1; // 0: 전체, 1~: 상태코드
        list = GetFilteredAlarms(); // 필터링 반영
        UpdateText();
    }

    private void UpdateFilter()
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            AlarmListItem item = this.items[i]; // ← 정확한 타입으로 수정
            item.gameObject.SetActive(true);

            // Area Filtering
            if (this.areaIndex > 0)
            {
                // Fetch the area by name from ModelProvider
                var area = modelProvider.GetAreaByName(item.data.areaName);
                if (area != null && area.areaId != this.areaIndex)
                {
                    item.gameObject.SetActive(false);
                }
            }

            if (this.statusIndex > -1)
            {
                if (item.data.status != this.statusIndex)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
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
        //Debug.Log($"정렬 실행: {order}");

        // 정렬 전 DateTime 값들 확인
        //Debug.Log("=== 정렬 전 시간 확인 ===");
        for (int i = 0; i < Math.Min(3, list.Count); i++)
        {
           // Debug.Log($"{i}: {list[i].time:yyyy-MM-dd HH:mm:ss} - {list[i].hnsName}");
        }

        this.list.Sort((a, b) =>
        {
            if (order == AlramOrder.TIME_UP.ToString())
            {
                //Debug.Log("🔵 TIME_UP 정렬 로직 실행 - 최신이 위");
                return b.time.CompareTo(a.time);  // 변경: 최신이 위로 오도록
            }
            else if (order == AlramOrder.TIME_DOWN.ToString())
            {
                //Debug.Log("🔴 TIME_DOWN 정렬 로직 실행 - 과거가 위");
                return a.time.CompareTo(b.time);  // 변경: 과거가 위로 오도록
            }
            // STATUS는 실제 상태값(status)으로 정렬
            // status: 0=설비이상(가장심각), 1=경보, 2=경계(가장낮음)
            else if (order == AlramOrder.STATUS_UP.ToString())
            {
                return a.status.CompareTo(b.status);  // 심각한 순서: 설비이상→경보→경계 (0→1→2)
            }
            else if (order == AlramOrder.STATUS_DOWN.ToString())
            {
                return b.status.CompareTo(a.status);  // 낮은 순서: 경계→경보→설비이상 (2→1→0)
            }
            // MAP은 지역명(areaName)으로 정렬
            else if (order == AlramOrder.MAP_UP.ToString())
            {
                return b.areaName.CompareTo(a.areaName);  // Z→A 순서
            }
            else if (order == AlramOrder.MAP_DOWN.ToString())
            {
                return a.areaName.CompareTo(b.areaName);  // A→Z 순서
            }
            // AREA는 관측소명(obsName)으로 정렬
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

        this.UpdateText();
        this.UpdateFilter();
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

