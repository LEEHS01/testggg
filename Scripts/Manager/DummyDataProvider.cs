using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Onthesys;

/// <summary>
/// 최소한의 더미 데이터 제공자 - 컴파일 에러 없이 UI만 확인
/// </summary>
public class DummyDataProvider : MonoBehaviour, ModelProvider
{
    private List<ToxinData> dummyToxinData = new List<ToxinData>();
    private List<LogData> dummyAlarmData = new List<LogData>();

    void Awake()
    {
        // 더미 센서 데이터 미리 생성
        CreateDummyToxinData();
        // 더미 알람 데이터 미리 생성
        CreateDummyAlarmData();
        Debug.Log("더미 데이터 프로바이더 활성화됨");

        // 더미 데이터 초기화 완료 후 Initiate 이벤트를 직접 호출
        UiManager.Instance.Invoke(UiEventType.Initiate);
    }

    void Start()
    {
        Debug.Log("더미 데이터 프로바이더 활성화됨");
    }

    // 지역 데이터 (기존 유지)
    // 지역 데이터 (발전소 타입 추가)
    public List<AreaData> GetAreas() => new List<AreaData>
{
    new AreaData { areaId = 1, areaName = "인천", areaType = AreaData.AreaType.Ocean },
    new AreaData { areaId = 2, areaName = "평택/대산", areaType = AreaData.AreaType.Ocean },
    new AreaData { areaId = 3, areaName = "고려 원자력", areaType = AreaData.AreaType.Nuclear },
    new AreaData { areaId = 4, areaName = "동해 화력", areaType = AreaData.AreaType.Nuclear },
    new AreaData { areaId = 5, areaName = "보령 화력", areaType = AreaData.AreaType.Nuclear },
    new AreaData { areaId = 6, areaName = "부산", areaType = AreaData.AreaType.Ocean },
    new AreaData { areaId = 7, areaName = "사천 화력", areaType = AreaData.AreaType.Nuclear },
    new AreaData { areaId = 8, areaName = "여수/광양", areaType = AreaData.AreaType.Ocean },
    new AreaData { areaId = 9, areaName = "영광 원자력", areaType = AreaData.AreaType.Nuclear },
    new AreaData { areaId = 10, areaName = "울산", areaType = AreaData.AreaType.Ocean }
};

    // 관측소 목록 반환 (모든 지역의 관측소들)
    public List<ObsData> GetObss()
    {
        var allObs = new List<ObsData>();
        foreach (var area in GetAreas())
        {
            allObs.AddRange(GetObssByAreaId(area.areaId));
        }
        return allObs;
    }

    // 특정 지역의 관측소들 반환
    public List<ObsData> GetObssByAreaId(int areaId)
    {
        var area = GetArea(areaId);
        string areaName = area?.areaName ?? "기본지역";

        return new List<ObsData>
        {
            new ObsData(areaName, areaId, "지역1", AreaData.AreaType.Ocean, areaId * 10 + 1, "rtsp://sample1", "rtsp://sample2"),
            new ObsData(areaName, areaId, "지역2", AreaData.AreaType.Ocean, areaId * 10 + 2, "rtsp://sample3", "rtsp://sample4"),
            new ObsData(areaName, areaId, "지역3", AreaData.AreaType.Nuclear, areaId * 10 + 3, "rtsp://sample5", "rtsp://sample6")
        };
    }

    // 특정 관측소 반환
    public ObsData GetObs(int obsId) => GetObss().FirstOrDefault(obs => obs.id == obsId);

    // 더미 센서 데이터를 반환
    public List<ToxinData> GetToxins() => dummyToxinData;
    public List<ToxinData> GetToxinsInLog() => dummyToxinData;
    public List<ToxinData> GetToxinsSetting() => dummyToxinData;
    public ToxinData GetToxin(int boardId, int hnsId) => dummyToxinData.FirstOrDefault(t => t.boardid == boardId && t.hnsid == hnsId);

    public List<LogData> GetAlarmsForDisplay() => dummyAlarmData;
    public List<LogData> GetActiveAlarms() => dummyAlarmData;
    public LogData GetAlarm(int alarmId) => dummyAlarmData.FirstOrDefault(log => log.idx == alarmId);

    // 나머지 기본값들
    public AreaData GetArea(int areaId) => GetAreas().FirstOrDefault(area => area.areaId == areaId);
    public ToxinStatus GetObsStatus(int obsId)
    {
        // 임시 알람에 맞춰 상태 변경
        if (dummyAlarmData.Any(log => log.obsId == obsId && log.status == 2)) return ToxinStatus.Red;
        if (dummyAlarmData.Any(log => log.obsId == obsId && log.status == 1)) return ToxinStatus.Yellow;
        if (dummyAlarmData.Any(log => log.obsId == obsId && log.status == 0)) return ToxinStatus.Purple;
        return ToxinStatus.Green;
    }
    public ToxinStatus GetAreaStatus(int areaId)
    {
        // 임시 알람에 맞춰 상태 변경
        if (dummyAlarmData.Any(log => GetObs(log.obsId).areaId == areaId && log.status == 2)) return ToxinStatus.Red;
        if (dummyAlarmData.Any(log => GetObs(log.obsId).areaId == areaId && log.status == 1)) return ToxinStatus.Yellow;
        if (dummyAlarmData.Any(log => GetObs(log.obsId).areaId == areaId && log.status == 0)) return ToxinStatus.Purple;
        return ToxinStatus.Green;
    }
    public ObsData GetObsByName(string obsName) => GetObss().FirstOrDefault(obs => obs.obsName == obsName);
    public AreaData GetAreaByName(string areaName) => GetAreas().FirstOrDefault(area => area.areaName == areaName);

    // 월간 알람 발생 TOP5 (실제 지역명 기반)
    public List<(int, int)> GetAlarmMonthly() => new List<(int, int)>
    {
        (1, 18),  // 인천: 18건
        (6, 15),  // 부산: 15건  
        (3, 12),  // 고려 화학단지: 12건
        (4, 9),   // 동해 화력: 9건
        (8, 7)    // 여수/광양: 7건
    };

    // 연간 알람 발생 TOP5 (실제 지역명 기반)
    public List<(int, AlarmCount)> GetAlarmYearly() => new List<(int, AlarmCount)>
    {
        (1, new AlarmCount(95, 18, 8, 2)),   // 인천
        (6, new AlarmCount(82, 15, 6, 1)),   // 부산
        (3, new AlarmCount(68, 12, 5, 1)),   // 고려 화학단지
        (4, new AlarmCount(55, 9, 3, 0)),    // 동해 화력
        (8, new AlarmCount(42, 7, 2, 0))     // 여수/광양
    };

    public List<AlarmSummaryModel> GetAlarmSummary() => new List<AlarmSummaryModel>();
    public AlarmCount GetObsStatusCountByAreaId(int areaId)
    {
        var obsInArea = GetObssByAreaId(areaId);
        return new AlarmCount(obsInArea.Count, 0, 0, 0); // 정상: 3개, 나머지: 0개
    }
    public ToxinStatus GetSensorStatus(int obsId, int boardId, int hnsId) => ToxinStatus.Green;
    public DateTime GetCurrentChartEndTime() => DateTime.Now;

    private void CreateDummyToxinData()
    {
        // 독성도 (BoardID: 1)
        var toxin1 = new ToxinData(new HnsResourceModel
        {
            boardidx = 1,
            hnsidx = 1,
            hnsnm = "독성도",
            alahival = 0,
            alahihival = 20,
            useyn = "1",
            unit = "%"
        });
        toxin1.CreateRandomValues();
        dummyToxinData.Add(toxin1);

        // 유해물질 (BoardID: 2), 19개 센서만 포함
        string[] hnsNames = new string[] {
            "1,4-다이옥산", "클로로포름", "트리클로로에틸렌", "페놀", "메틸 에틸 케톤",
            "디클로로메탄", "헥산(모든이성질체)", "n-알칸(C10-C20)", "장쇄알카릴슬폰산바륨(C11-C50)", "포스포러스황화 폴리 올레핀바륨",
            "중크롬산나트륨 용액(70%이하)", "장연쇄(C17+) 알케이노산구리염", "자동차연료용 노킹억제 화합물", "메틸시클로펜타디엔일", "드릴링 염수(염화 아연 함유)",
            "디티오인산아연알카릴(C7-C16)", "카르복스아마이드아연알키닐", "디티오인산 아연알킬(C3-C14)", "브로모클로르메탄"
        };
        for (int i = 0; i < hnsNames.Length; i++)
        {
            string useyn = new int[] { 1, 3, 4, 8, 10, 16, 17, 19 }.Contains(i + 1) ? "0" : "1";
            var toxin = new ToxinData(new HnsResourceModel
            {
                boardidx = 2,
                hnsidx = i + 1,
                hnsnm = hnsNames[i],
                alahival = 9999,
                alahihival = 9999,
                useyn = useyn,
                unit = "mg/L"
            });
            toxin.CreateRandomValues();
            dummyToxinData.Add(toxin);
        }

        // 수질 (BoardID: 3), 7개 센서만 포함
        dummyToxinData.Add(CreateToxin(3, 1, "Temperature", 555, 999, "1", "°C"));
        dummyToxinData.Add(CreateToxin(3, 2, "DO", 555, 999, "1", "mg/L"));
        dummyToxinData.Add(CreateToxin(3, 3, "BOD", 66, 103, "1", "mg/L"));
        dummyToxinData.Add(CreateToxin(3, 4, "Conductivity", 999, 999, "1", "μS/s"));
        dummyToxinData.Add(CreateToxin(3, 5, "pH", 999, 999, "1", "")); // NULL은 빈 문자열로 처리
        dummyToxinData.Add(CreateToxin(3, 6, "Turbidity", 222, 333, "1", "NTU"));
        dummyToxinData.Add(CreateToxin(3, 7, "TSS", 40, 80, "1", "mg/L"));
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
            useyn = useyn,
            unit = unit
        };
        var toxin = new ToxinData(model);
        toxin.CreateRandomValues();
        return toxin;
    }

    private void CreateDummyAlarmData()
    {
        // 임시 알람 데이터 생성
        // "인천" 지역, "지역1" 관측소에 대한 경보 알람
        dummyAlarmData.Add(new LogData(
            obsid: 11, // 인천(1)의 지역1(1)
            boardid: 3,
            areaName: "인천",
            obsName: "지역1",
            hnsId: 1, // Temperature 센서
            hnsName: "Temperature",
            dt: DateTime.Now,
            status: 2, // 2: 경보 (Red)
            val: 999.0f,
            idx: 1001,
            serious: 555.0f,
            warning: 999.0f
        ));

        // 다른 상태의 알람도 추가하여 테스트 가능
        // 경계 알람
        dummyAlarmData.Add(new LogData(
            obsid: 12, // 인천(1)의 지역2(2)
            boardid: 3,
            areaName: "인천",
            obsName: "지역2",
            hnsId: 3, // BOD 센서
            hnsName: "BOD",
            dt: DateTime.Now.AddMinutes(-5),
            status: 1, // 1: 경계 (Yellow)
            val: 70.0f,
            idx: 1002,
            serious: 66.0f,
            warning: 103.0f
        ));

        // 설비 이상 알람
        dummyAlarmData.Add(new LogData(
            obsid: 21, // 평택/대산(2)의 지역1(1)
            boardid: 2,
            areaName: "평택/대산",
            obsName: "지역1",
            hnsId: 1, // 1,4-다이옥산
            hnsName: "1,4-다이옥산",
            dt: DateTime.Now.AddHours(-1),
            status: 0, // 0: 설비이상 (Purple)
            val: 0.0f,
            idx: 1003,
            serious: 9999.0f,
            warning: 9999.0f
        ));
    }
}