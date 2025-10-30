using Onthesys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DummyDataProvider : MonoBehaviour, ModelProvider
{
    public static ModelProvider Instance;

    private List<ObsData> obss = new List<ObsData>();
    private List<AreaData> areas = new List<AreaData>();
    private List<ToxinData> toxins = new List<ToxinData>();
    private List<LogData> currentAlarms = new List<LogData>();
    private List<LogData> historicalAlarms = new List<LogData>();
    private int currentObsId = -1;
    private DateTime currentChartEndTime = DateTime.Now;

    private string cctvOut = "rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42:50556/video1?profile=high";
    private string cctvIn = "rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42:554/video1?profile=high";

    void Awake()
    {
        // ⭐ 가장 먼저 Instance 설정
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ⭐ Instance 설정 직후 바로 데이터 초기화
        InitializeDummyData();

        Debug.Log($"[DummyDataProvider] Awake 완료 - areas.Count: {areas.Count}, obss.Count: {obss.Count}");
    }

    void Start()
    {
        // 관측소 선택 이벤트 등록
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);

        // 초기화 코루틴 시작
        StartCoroutine(InitializeWithoutData());
    }

    private IEnumerator InitializeWithoutData()
    {
        yield return null;

        Debug.Log($"[DummyDataProvider] 초기화 시작");
        Debug.Log($"[DummyDataProvider] obss.Count = {obss.Count}");
        Debug.Log($"[DummyDataProvider] areas.Count = {areas.Count}");

        // areas 내용 출력
        for (int i = 0; i < areas.Count; i++)
        {
            Debug.Log($"  Area[{i}]: id={areas[i].areaId}, name={areas[i].areaName}");
        }

        Debug.Log("[DummyDataProvider] Initiate 이벤트 발생");
        UiManager.Instance.Invoke(UiEventType.Initiate);

        yield return new WaitForSeconds(0.5f); // 조금 더 대기

        Debug.Log("[DummyDataProvider] ChangeAlarmMonthly 이벤트 발생");
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmMonthly);

        yield return new WaitForSeconds(0.1f);

        Debug.Log("[DummyDataProvider] ChangeAlarmYearly 이벤트 발생");
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmYearly);

        yield return new WaitForSeconds(0.1f);

        Debug.Log("[DummyDataProvider] ChangeAlarmMap 이벤트 발생");
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmMap);

        Debug.Log("[DummyDataProvider] 모든 초기화 완료");
    }

    // 관측소 선택 이벤트 처리
    private void OnNavigateObs(object obj)
    {
        if (obj is int obsId)
        {
            currentObsId = obsId;

            Debug.Log($"[DummyDataProvider] 관측소 선택: {obsId}");

            // 관측소 선택 시 데이터 관련 이벤트 발생
            UiManager.Instance.Invoke(UiEventType.ChangeAlarmList);
            UiManager.Instance.Invoke(UiEventType.ChangeSensorList);
            UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
            UiManager.Instance.Invoke(UiEventType.ChangeSettingSensorList);
        }
    }

    private void InitializeDummyData()
    {
        // Areas
        areas.Add(new AreaData { areaId = 1, areaName = "인천", areaType = AreaData.AreaType.Ocean });
        areas.Add(new AreaData { areaId = 2, areaName = "평택/대산", areaType = AreaData.AreaType.Ocean });
        areas.Add(new AreaData { areaId = 3, areaName = "대산/광양", areaType = AreaData.AreaType.Ocean });
        areas.Add(new AreaData { areaId = 4, areaName = "부산", areaType = AreaData.AreaType.Ocean });
        areas.Add(new AreaData { areaId = 5, areaName = "울산", areaType = AreaData.AreaType.Ocean });
        areas.Add(new AreaData { areaId = 6, areaName = "보령 화력", areaType = AreaData.AreaType.Nuclear });
        areas.Add(new AreaData { areaId = 7, areaName = "영광 원자력", areaType = AreaData.AreaType.Nuclear });
        areas.Add(new AreaData { areaId = 8, areaName = "사천 화력", areaType = AreaData.AreaType.Nuclear });
        areas.Add(new AreaData { areaId = 9, areaName = "고리 원자력", areaType = AreaData.AreaType.Nuclear });
        areas.Add(new AreaData { areaId = 10, areaName = "동해화력", areaType = AreaData.AreaType.Nuclear });

        // Observations (30개)
        for (int i = 1; i <= 30; i++)
        {
            int areaIdx = ((i - 1) / 3) + 1;
            int localIdx = ((i - 1) % 3) + 1;
            string obsnm = $"지역{localIdx}";

            var obsModel = new ObservatoryModel
            {
                obsidx = i,
                areaidx = areaIdx,
                areanm = areas[areaIdx - 1].areaName,
                obsnm = obsnm,
                areatype = (int)areas[areaIdx - 1].areaType,
                out_cctvUrl = cctvOut,
                in_cctvUrl = cctvIn
            };

            obss.Add(ObsData.FromObsModel(obsModel));
        }

        InitializeSensors();
        InitializeAlarms();
    }

    private void InitializeSensors()
    {
        // Board 1: 독성도 (1개)
        toxins.Add(CreateToxin(1, 1, "독성도", 0, 20, "1", "%"));

        // Board 2: 화학물질 (36개)
        string[] chemNames = {
        "1,4-디미톡산", "클로로포름", "트리클로로에틸렌", "페놀", "메틸 메틸 카톤",
        "디클로로메탄", "헥산(오르도-크실렌)", "n-알칸(C10-C20)", "정제탈겁결솜산나늄(C11-C50)",
        "초스포러스향각 달미, 클래전나늄", "싸클로알서비크로아나(70%이하)", "정전솜(C17+) 알게이노산구리업",
        "자등차연료용 노르익제 화년물", "메틸시클로펜티디엔", "드림터 염산외삽탐 칼륨(알칼료)", "디티오인산(연알킬)일(C7-C16)",
        "카르복스디메인드(연알킬)일", "디티오인산 마요알발(C3-C14)", "보로부릴로포라모", "아세톤",
        "시안", "비소", "카드뮴", "수은", "셀레늄",
        "니켈", "조석", "은", "바나듐", "몰소",
        "1-나타논", "2-프로판올", "메탄올", "메탄올", "알델니아",
        "1,1,1-트리클로로에탄"
    };

        for (int i = 0; i < 36; i++)
        {
            toxins.Add(CreateToxin(2, i + 1, chemNames[i], 9999, 9999, "1", "mg/L"));
        }

        // Board 3: 수질 (8개)
        string[] waterNames = { "Temperature", "DO", "BOD", "Conductivity", "pH", "Turbidity", "TSS", "Battery Voltage" };
        float[] waterHi = { 555f, 60f, 66f, 91f, 88f, 222f, 40f, 999f };
        float[] waterHihi = { 555f, 999f, 103f, 999f, 999f, 399f, 80f, 999f };
        string[] waterUnits = { "℃", "mg/L", "mg/L", "㎲/s", "NULL", "NTU", "mg/L", "NULL" };
        string[] waterUse = { "1", "1", "1", "1", "1", "1", "1", "0" };

        for (int i = 0; i < 8; i++)
        {
            toxins.Add(CreateToxin(3, i + 1, waterNames[i], waterHi[i], waterHihi[i], waterUse[i], waterUnits[i]));
        }

        Debug.Log($"[DummyDataProvider] InitializeSensors 완료 - toxins.Count: {toxins.Count}");
    }

    private ToxinData CreateToxin(int boardIdx, int hnsIdx, string hnsNm, float hiVal, float hihiVal, string useyn, string unit)
    {
        var model = new HnsResourceModel
        {
            boardidx = boardIdx,
            hnsidx = hnsIdx,
            hnsnm = hnsNm,
            alahival = hiVal,
            alahihival = hihiVal,
            alahihisec = null,
            useyn = useyn,
            inspectionflag = "0",
            unit = unit
        };

        var toxin = new ToxinData(model, unit);

        // ⭐ ToxinData 생성자에서 이미 초기화되지만, 혹시 모르니 다시 확인
        if (toxin.values == null) toxin.values = new List<float>();
        if (toxin.aiValues == null) toxin.aiValues = new List<float>();
        if (toxin.diffValues == null) toxin.diffValues = new List<float>();
        if (toxin.dateTimes == null) toxin.dateTimes = new List<DateTime>();
        if (toxin.chartValues == null) toxin.chartValues = new List<float>();
        if (toxin.chartAiValues == null) toxin.chartAiValues = new List<float>();
        if (toxin.chartDiffValues == null) toxin.chartDiffValues = new List<float>();
        if (toxin.chartDateTimes == null) toxin.chartDateTimes = new List<DateTime>();

        // ⭐ 72개 값 생성 (12시간 * 6회/시간 = 10분 간격)
        DateTime now = DateTime.Now;

        for (int i = 0; i < 72; i++)
        {
            // ⭐ 센서 타입별로 적절한 랜덤 값 생성
            float value;

            if (boardIdx == 1) // 독성도
            {
                // 0~5% 사이의 작은 값
                value = UnityEngine.Random.Range(0f, 5f);
            }
            else if (boardIdx == 2) // 화학물질
            {
                // 0~50 사이의 랜덤 값
                value = UnityEngine.Random.Range(0f, 50f);
            }
            else // Board 3: 수질
            {
                // ⭐ 수질 센서는 특별 처리
                if (hnsNm == "Temperature")
                    value = UnityEngine.Random.Range(15f, 25f); // 15~25도
                else if (hnsNm == "DO")
                    value = UnityEngine.Random.Range(5f, 15f); // 5~15 mg/L
                else if (hnsNm == "BOD")
                    value = UnityEngine.Random.Range(1f, 30f); // 1~30 mg/L
                else if (hnsNm == "Conductivity")
                    value = UnityEngine.Random.Range(50f, 500f); // 50~500 μS/s
                else if (hnsNm == "pH")
                    value = UnityEngine.Random.Range(6.5f, 8.5f); // 6.5~8.5
                else if (hnsNm == "Turbidity")
                    value = UnityEngine.Random.Range(1f, 50f); // 1~50 NTU
                else if (hnsNm == "TSS")
                    value = UnityEngine.Random.Range(1f, 20f); // 1~20 mg/L
                else if (hnsNm == "Battery Voltage")
                    value = UnityEngine.Random.Range(11f, 13f); // 11~13V
                else
                    value = UnityEngine.Random.Range(0f, 10f);
            }

            // 소수점 2자리로 반올림
            value = Mathf.Round(value * 100f) / 100f;
            toxin.values.Add(value);

            // 시간 생성 (12시간 전부터 현재까지, 10분 간격)
            DateTime time = now.AddMinutes(-720 + (i * 10));
            toxin.dateTimes.Add(time);
        }

        toxin.status = ToxinStatus.Green;

        Debug.Log($"[CreateToxin] board={boardIdx}, hns={hnsIdx}, name={hnsNm}, values.Count={toxin.values.Count}, dateTimes.Count={toxin.dateTimes.Count}, firstValue={toxin.values[0]}, lastValue={toxin.values[toxin.values.Count - 1]}");

        return toxin;
    }

    private void InitializeAlarms()
    {
        // 인천 지역1 (OBSIDX=1) - 경보 (Red)
        currentAlarms.Add(new LogData(
            1, 1, "인천", "지역1", 1, "독성도",
            DateTime.Parse("2025-10-29 15:30:22"), 2, 35.8f, 3001, 0f, 20f, false
        ));

        // 평택/대산 지역1 (OBSIDX=4) - 경계 (Yellow)
        currentAlarms.Add(new LogData(
            4, 3, "평택/대산", "지역1", 3, "BOD",
            DateTime.Parse("2025-10-28 14:20:11"), 1, 75.2f, 3002, 66f, 103f, false
        ));

        // 동해화력 지역1 (OBSIDX=28) - 설비이상 (Purple)
        currentAlarms.Add(new LogData(
            28, 1, "동해화력", "지역1", 1, "독성도",
            DateTime.Parse("2025-10-27 10:15:33"), 0, 0f, 3003, 0f, 20f, false
        ));

        historicalAlarms.AddRange(currentAlarms);
    }

    // ModelProvider 인터페이스 구현
    public ObsData GetObs(int obsId)
    {
        var obs = obss.FirstOrDefault(o => o.id == obsId);
        if (obs == null)
        {
            Debug.LogError($"[DummyDataProvider] GetObs({obsId}) - obs를 찾을 수 없음! obss.Count={obss.Count}");

            // ⭐ ObsData 생성자 직접 호출 (ExampleDummyDataProvider 방식)
            return new ObsData(
                "알 수 없음",                    // areaName
                1,                               // areaId
                "알 수 없음",                    // obsName
                AreaData.AreaType.Ocean,         // areaType
                obsId,                           // id
                cctvOut,                         // outCctvUrl
                cctvIn                           // inCctvUrl
            );
        }
        return obs;
    }

    public List<ObsData> GetObss() => obss;
    public List<ObsData> GetObssByAreaId(int areaId) => obss.Where(o => o.areaId == areaId).ToList();
    public ObsData GetObsByName(string obsName) => obss.FirstOrDefault(o => o.obsName == obsName);

    public ToxinStatus GetObsStatus(int obsId)
    {
        var alarms = currentAlarms.Where(a => a.obsId == obsId).ToList();
        if (alarms.Any(a => a.status == 2)) return ToxinStatus.Red;
        if (alarms.Any(a => a.status == 1)) return ToxinStatus.Yellow;
        if (alarms.Any(a => a.status == 0)) return ToxinStatus.Purple;
        return ToxinStatus.Green;
    }

    public List<AreaData> GetAreas() => areas;

    public AreaData GetArea(int areaId)
    {
        var area = areas.FirstOrDefault(a => a.areaId == areaId);
        if (area == null)
        {
            Debug.LogError($"[DummyDataProvider] GetArea({areaId}) - area를 찾을 수 없음!");
            Debug.LogError($"[DummyDataProvider] 등록된 area IDs: {string.Join(", ", areas.Select(a => a.areaId))}");
            return new AreaData
            {
                areaId = areaId,
                areaName = "알 수 없음",
                areaType = AreaData.AreaType.Ocean
            };
        }
        return area;
    }

    public AreaData GetAreaByName(string areaName) => areas.FirstOrDefault(a => a.areaName == areaName);

    public ToxinStatus GetAreaStatus(int areaId)
    {
        var obsInArea = obss.Where(o => o.areaId == areaId).Select(o => o.id).ToList();
        var alarms = currentAlarms.Where(a => obsInArea.Contains(a.obsId)).ToList();
        if (alarms.Any(a => a.status == 2)) return ToxinStatus.Red;
        if (alarms.Any(a => a.status == 1)) return ToxinStatus.Yellow;
        if (alarms.Any(a => a.status == 0)) return ToxinStatus.Purple;
        return ToxinStatus.Green;
    }

    public ToxinData GetToxin(int boardId, int hnsId) => toxins.FirstOrDefault(t => t.boardid == boardId && t.hnsid == hnsId);
    public List<ToxinData> GetToxins() => toxins;
    public List<ToxinData> GetToxinsInLog() => toxins;
    public List<ToxinData> GetToxinsSetting() => toxins;

    public List<LogData> GetAlarmsForDisplay() => historicalAlarms;
    public List<LogData> GetActiveAlarms() => currentAlarms.Where(a => !a.isCancelled).ToList();
    public LogData GetAlarm(int alarmId) => historicalAlarms.FirstOrDefault(a => a.idx == alarmId);

    // ⭐ 하드코딩 - 월간 알람 발생 TOP 10
    public List<(int areaId, int count)> GetAlarmMonthly()
    {
        var result = new List<(int, int)>
    {
        (1, 18),  // 인천
        (2, 15),  // 평택/대산
        (3, 12),  // 대산/광양
        (4, 9),   // 부산
        (5, 7),   // 울산
        (6, 5),   // 보령 화력
        (7, 4),   // 영광 원자력
        (8, 3),   // 사천 화력
        (9, 2),   // 고리 원자력
        (10, 1)   // 동해화력
    };

        Debug.Log($"[DummyDataProvider] GetAlarmMonthly 호출됨 - result.Count: {result.Count}");
        foreach (var item in result)
        {
            var area = GetArea(item.Item1);
            Debug.Log($"  AreaId: {item.Item1}, AreaName: {area?.areaName ?? "null"}, Count: {item.Item2}");
        }

        return result;
    }

    // ⭐ 하드코딩 - 연간 알람 발생 TOP 10
    public List<(int areaId, AlarmCount counts)> GetAlarmYearly()
    {
        var result = new List<(int, AlarmCount)>
    {
        (1, new AlarmCount(95, 82, 8, 2)),
        (2, new AlarmCount(82, 58, 6, 1)),
        (3, new AlarmCount(68, 41, 5, 1)),
        (4, new AlarmCount(55, 23, 3, 0)),
        (5, new AlarmCount(42, 10, 2, 0)),
        (6, new AlarmCount(35, 5, 1, 0)),
        (7, new AlarmCount(28, 4, 1, 0)),
        (8, new AlarmCount(21, 3, 0, 0)),
        (9, new AlarmCount(14, 2, 0, 0)),
        (10, new AlarmCount(7, 1, 0, 0))
    };

        Debug.Log($"[DummyDataProvider] GetAlarmYearly 호출됨 - result.Count: {result.Count}");

        return result;
    }

    public List<AlarmSummaryModel> GetAlarmSummary()
    {
        return areas.Select(area => {
            var obsInArea = obss.Where(o => o.areaId == area.areaId).Select(o => o.id);
            var alarms = currentAlarms.Where(a => obsInArea.Contains(a.obsId));
            return new AlarmSummaryModel
            {
                obsidx = area.areaId,
                month = DateTime.Now.Month,
                year = DateTime.Now.Year,
                cnt = alarms.Count()
            };
        }).ToList();
    }

    // ⭐ 하드코딩 - 지역별 관측소 상태 카운트 (집중우심해역, 주요발전소 모니터링용)
    public AlarmCount GetObsStatusCountByAreaId(int areaId)
    {
        switch (areaId)
        {
            case 1:  // 인천 - 지역1이 경보
                return new AlarmCount(2, 0, 1, 0);  // 정상2, 경고0, 경보1, 설비이상0

            case 2:  // 평택/대산 - 지역1이 경계
                return new AlarmCount(2, 1, 0, 0);  // 정상2, 경고1, 경보0, 설비이상0

            case 10: // 동해화력 - 지역1이 설비이상
                return new AlarmCount(2, 0, 0, 1);  // 정상2, 경고0, 경보0, 설비이상1

            default: // 나머지 모든 지역은 정상
                return new AlarmCount(3, 0, 0, 0);  // 모두 정상
        }
    }

    public ToxinStatus GetSensorStatus(int obsId, int boardId, int hnsId)
    {
        var alarm = currentAlarms.FirstOrDefault(a => a.obsId == obsId && a.boardId == boardId && a.hnsId == hnsId);
        if (alarm == null) return ToxinStatus.Green;
        return alarm.status switch
        {
            2 => ToxinStatus.Red,
            1 => ToxinStatus.Yellow,
            0 => ToxinStatus.Purple,
            _ => ToxinStatus.Green
        };
    }

    public DateTime GetCurrentChartEndTime() => currentChartEndTime;
    public int GetCurrentObsId()
    {
        Debug.Log($"[DummyDataProvider] GetCurrentObsId() = {currentObsId}");
        return currentObsId;
    }
}