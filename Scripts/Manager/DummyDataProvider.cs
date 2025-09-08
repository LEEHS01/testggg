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

    // 기본적인 빈 데이터 반환 - 컴파일 에러 방지용
    public List<ToxinData> GetToxins() => new List<ToxinData>();
    public List<ToxinData> GetToxinsInLog() => new List<ToxinData>();
    public List<ToxinData> GetToxinsSetting() => new List<ToxinData>();
    public ToxinData GetToxin(int boardId, int hnsId) => null;
    public List<LogData> GetAlarmsForDisplay() => new List<LogData>();
    public List<LogData> GetActiveAlarms() => new List<LogData>();
    public LogData GetAlarm(int alarmId) => null;

    // 나머지 기본값들
    public AreaData GetArea(int areaId) => GetAreas().FirstOrDefault(area => area.areaId == areaId);
    public ToxinStatus GetObsStatus(int obsId) => ToxinStatus.Green;
    public ToxinStatus GetAreaStatus(int areaId) => ToxinStatus.Green;
    public ObsData GetObsByName(string obsName) => GetObss().FirstOrDefault(obs => obs.obsName == obsName);
    public AreaData GetAreaByName(string areaName) => GetAreas().FirstOrDefault(area => area.areaName == areaName);

    // 월간 알람 발생 TOP5 (실제 지역명 기반)
    public List<(int, int)> GetAlarmMonthly() => new List<(int, int)>
    {
        (1, 18),  // 인천: 18건
        (6, 15),  // 부산: 15건  
        (3, 12),  // 고려 화학단지: 12건
        (4, 9),   // 동해 화학: 9건
        (8, 7)    // 여수/광양: 7건
    };

    // 연간 알람 발생 TOP5 (실제 지역명 기반)
    public List<(int, AlarmCount)> GetAlarmYearly() => new List<(int, AlarmCount)>
    {
        (1, new AlarmCount(95, 18, 8, 2)),   // 인천
        (6, new AlarmCount(82, 15, 6, 1)),   // 부산
        (3, new AlarmCount(68, 12, 5, 1)),   // 고려 화학단지
        (4, new AlarmCount(55, 9, 3, 0)),    // 동해 화학
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
}