using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Onthesys;
using System.Collections;

/// <summary>
/// 최소한의 더미 데이터 제공자 - 컴파일 에러 없이 UI만 확인
/// </summary>
public class DummyDataProvider : MonoBehaviour, ModelProvider
{
    private List<ToxinData> dummyToxinData = new List<ToxinData>();
    private List<LogData> dummyAlarmData = new List<LogData>();
    private int currentObsId = 11; // 현재 선택된 관측소

    void Awake()
    {
        // 더미 센서 데이터 미리 생성
        CreateDummyToxinData();
        // 더미 알람 데이터 미리 생성
        CreateDummyAlarmData();
        Debug.Log("더미 데이터 프로바이더 활성화됨");

        // 관측소 선택 이벤트 등록
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        // 알람 선택 이벤트 등록 추가
        UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectAlarm);

        // ModelManager와 동일하게 모든 이벤트 호출
        StartCoroutine(InitializeAllEvents());

        StartCoroutine(BlinkingEffect());

        StartCoroutine(CycleSensorSteps());
    }

    private IEnumerator InitializeAllEvents()
    {
        yield return null; // 한 프레임 대기

        // ModelManager가 호출하는 모든 이벤트들을 동일하게 호출
        UiManager.Instance.Invoke(UiEventType.Initiate);
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmList);
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmMonthly);
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmYearly);
        UiManager.Instance.Invoke(UiEventType.ChangeSensorList);
        UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
        UiManager.Instance.Invoke(UiEventType.ChangeSensorStatus);
        UiManager.Instance.Invoke(UiEventType.ChangeSummary);
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmMap);
        //UiManager.Instance.Invoke(UiEventType.ChangeSensorStep, 5);
        UiManager.Instance.Invoke(UiEventType.ChangeSettingSensorList);
        UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (1, 1));

        Debug.Log("더미 데이터 - 모든 이벤트 호출 완료");
    }

    private IEnumerator BlinkingEffect()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f); // 1초마다

            // 알람이 있는 센서들에 대해 깜박임 이벤트 발생
            if (dummyAlarmData.Count > 0)
            {
                UiManager.Instance.Invoke(UiEventType.ChangeSensorStatus);
            }
        }
    }

    // 관측소 선택 이벤트 처리
    private void OnNavigateObs(object obj)
    {
        if (obj is int obsId)
        {
            currentObsId = obsId;
            AdjustSensorValuesForCurrentObs();
            UiManager.Instance.Invoke(UiEventType.ChangeSensorList);
            UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
            UiManager.Instance.Invoke(UiEventType.ChangeSettingSensorList);
        }
    }

    // ✅ 알람 선택 이벤트 처리 추가
    private void OnSelectAlarm(object obj)
    {
        if (obj is int logIdx)
        {
            // 선택된 알람에 해당하는 관측소로 이동
            LogData selectedLog = dummyAlarmData.FirstOrDefault(log => log.idx == logIdx);
            if (selectedLog != null)
            {
                currentObsId = selectedLog.obsId;
                AdjustSensorValuesForCurrentObs();

                // ✅ 즉시 여러 이벤트를 순차적으로 발생 (초기화 완료 보장)
                StartCoroutine(InitializeAlarmSelection());
            }
        }
    }

    // ✅ 알람 선택 시 순차적 초기화
    private IEnumerator InitializeAlarmSelection()
    {
        // 한 프레임 대기
        yield return null;

        // 센서 리스트 먼저 업데이트
        UiManager.Instance.Invoke(UiEventType.ChangeSensorList);

        // 한 프레임 더 대기
        yield return null;

        // 알람 센서 리스트 업데이트 (ToxinList2가 기다리는 이벤트)
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmSensorList);

        // 트렌드 라인도 업데이트
        UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
    }

    // 현재 관측소에 맞게 센서 값 조정
    private void AdjustSensorValuesForCurrentObs()
    {
        var tempSensor = dummyToxinData.FirstOrDefault(t => t.boardid == 3 && t.hnsid == 1); // Temperature
        var bodSensor = dummyToxinData.FirstOrDefault(t => t.boardid == 3 && t.hnsid == 3);   // BOD
        var toxinSensor = dummyToxinData.FirstOrDefault(t => t.boardid == 1 && t.hnsid == 1); // 독성도

        // 모든 센서 기본값으로 초기화
        ResetAllSensorsToNormal();

        if (currentObsId == 11) // 인천 지역1 - Temperature 경보
        {
            if (tempSensor != null)
            {
                for (int i = 0; i < tempSensor.values.Count; i++)
                    tempSensor.values[i] = UnityEngine.Random.Range(600f, 650f);
                tempSensor.status = ToxinStatus.Red;
            }
        }
        else if (currentObsId == 12) // 인천 지역2 - BOD 경계
        {
            if (bodSensor != null)
            {
                for (int i = 0; i < bodSensor.values.Count; i++)
                    bodSensor.values[i] = UnityEngine.Random.Range(70f, 80f);
                bodSensor.status = ToxinStatus.Yellow;
            }
        }
        else if (currentObsId == 21) // 평택/대산 지역1 - 독성도 설비이상
        {
            if (toxinSensor != null)
            {
                // ✅ 독성도 설비이상 시에도 0 (측정 불가)
                for (int i = 0; i < toxinSensor.values.Count; i++)
                    toxinSensor.values[i] = 0.0f;
                toxinSensor.status = ToxinStatus.Purple;
            }
        }
    }
    private IEnumerator CycleSensorSteps()
    {
        yield return new WaitForSeconds(3.0f);
        int currentStep = 1;

        while (true)
        {
            // X-Ray 상태 체크 추가
            if (currentObsId > 0 && IsXrayFullyActive())
            {
                UiManager.Instance.Invoke(UiEventType.ChangeSensorStep, currentStep);
                currentStep++;
                if (currentStep > 5) currentStep = 1;
            }

            yield return new WaitForSeconds(10.0f);
        }
    }
    private bool IsXrayFullyActive()
    {
        // TitleXrayButton의 static 변수들을 리플렉션으로 접근
        try
        {
            var titleXrayType = System.Type.GetType("TitleXrayButton");
            if (titleXrayType != null)
            {
                var structureField = titleXrayType.GetField("isStructureXrayActive",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var equipmentField = titleXrayType.GetField("isEquipmentXrayActive",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                if (structureField != null && equipmentField != null)
                {
                    bool structureActive = (bool)structureField.GetValue(null);
                    bool equipmentActive = (bool)equipmentField.GetValue(null);

                    Debug.Log($"Structure X-Ray: {structureActive}, Equipment X-Ray: {equipmentActive}");
                    return structureActive && equipmentActive;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"TitleXrayButton static 변수 접근 실패: {ex.Message}");
        }

        return false;
    }

    // 모든 센서를 정상 상태로 초기화
    private void ResetAllSensorsToNormal()
    {
        foreach (var toxin in dummyToxinData)
        {
            if (toxin.boardid == 1 && toxin.hnsid == 1) // 독성도
            {
                // ✅ 독성도는 정상일 때 무조건 0
                for (int i = 0; i < toxin.values.Count; i++)
                    toxin.values[i] = 0.0f;
            }
            else if (toxin.boardid == 3 && toxin.hnsid == 1) // Temperature
            {
                for (int i = 0; i < toxin.values.Count; i++)
                    toxin.values[i] = UnityEngine.Random.Range(0.4f, 0.5f);
            }
            else if (toxin.boardid == 3 && toxin.hnsid == 3) // BOD
            {
                for (int i = 0; i < toxin.values.Count; i++)
                    toxin.values[i] = UnityEngine.Random.Range(0.4f, 0.5f);
            }

            toxin.status = ToxinStatus.Green; // 모든 센서 정상 상태로
        }
    }

    // 지역 데이터
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
        if (dummyAlarmData.Any(log => log.obsId == obsId && log.status == 2)) return ToxinStatus.Red;
        if (dummyAlarmData.Any(log => log.obsId == obsId && log.status == 1)) return ToxinStatus.Yellow;
        if (dummyAlarmData.Any(log => log.obsId == obsId && log.status == 0)) return ToxinStatus.Purple;
        return ToxinStatus.Green;
    }
    public ToxinStatus GetAreaStatus(int areaId)
    {
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
        (1, 18), (6, 15), (3, 12), (4, 9), (8, 7)
    };

    // 연간 알람 발생 TOP5 (실제 지역명 기반)
    public List<(int, AlarmCount)> GetAlarmYearly() => new List<(int, AlarmCount)>
    {
        (1, new AlarmCount(95, 18, 8, 2)),
        (6, new AlarmCount(82, 15, 6, 1)),
        (3, new AlarmCount(68, 12, 5, 1)),
        (4, new AlarmCount(55, 9, 3, 0)),
        (8, new AlarmCount(42, 7, 2, 0))
    };

    public List<AlarmSummaryModel> GetAlarmSummary() => new List<AlarmSummaryModel>();
    public AlarmCount GetObsStatusCountByAreaId(int areaId)
    {
        switch (areaId)
        {
            case 1: // 인천 - 지역1,2가 경보/경계
                return new AlarmCount(1, 1, 1, 0); // 정상1, 경고1, 경보2, 설비이상0

            case 2: // 평택 - 지역1이 설비이상  
                return new AlarmCount(2, 0, 0, 1); // 정상2, 경고0, 경보0, 설비이상1

            case 3: // 고려 화학단지
                return new AlarmCount(3, 0, 0, 0); // 모두 정상

            default:
                return new AlarmCount(3, 0, 0, 0); // 기본값 모두 정상
        }
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

        // 유해물질 (BoardID: 2), 19개 센서
        string[] hnsNames = new string[] {
            "1,4-다이옥산", "클로로포름", "트리클로로에틸렌", "페놀", "메틸 에틸 케톤",
            "디클로로메탄", "헥산(모든이성질체)", "n-알칸(C10-C20)", "장쇄알카릴슬폰산바륨(C11-C50)", "포스포러스황화 폴리 올레핀바륨",
            "중크롬산나트륨 용액(70%이하)", "장연쇄(C17+) 알케이노산구리염", "자동차연료용 노킹억제 화합물", "메틸시클로펜타디엔일", "드릴링 염수(염화 아연 함유)",
            "디티오인산아연알카릴(C7-C16)", "카르복스아마이드아연알키닐", "디티오인산 아연알킬(C3-C14)", "브로모클로르메탄"
        };
        for (int i = 0; i < hnsNames.Length; i++)
        {
            var toxin = new ToxinData(new HnsResourceModel
            {
                boardidx = 2,
                hnsidx = i + 1,
                hnsnm = hnsNames[i],
                alahival = 9999,
                alahihival = 9999,
                useyn = "1",
                unit = "mg/L"
            });
            toxin.CreateRandomValues();
            dummyToxinData.Add(toxin);
        }

        // 수질 (BoardID: 3), 7개 센서
        dummyToxinData.Add(CreateToxin(3, 1, "Temperature", 555, 999, "1", "°C"));
        dummyToxinData.Add(CreateToxin(3, 2, "DO", 555, 999, "1", "mg/L"));
        dummyToxinData.Add(CreateToxin(3, 3, "BOD", 66, 103, "1", "mg/L"));
        dummyToxinData.Add(CreateToxin(3, 4, "Conductivity", 999, 999, "1", "μS/s"));
        dummyToxinData.Add(CreateToxin(3, 5, "pH", 999, 999, "1", ""));
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
        // 인천 지역1 - Temperature 경보
        dummyAlarmData.Add(new LogData(
            obsid: 11,
            boardid: 3,
            areaName: "인천",
            obsName: "지역1",
            hnsId: 1,
            hnsName: "Temperature",
            dt: DateTime.Now,
            status: 2,
            val: 999.0f,
            idx: 1001,
            serious: 555.0f,
            warning: 999.0f
        ));

        // 인천 지역2 - BOD 경계
        dummyAlarmData.Add(new LogData(
            obsid: 12,
            boardid: 3,
            areaName: "인천",
            obsName: "지역2",
            hnsId: 3,
            hnsName: "BOD",
            dt: DateTime.Now.AddMinutes(-5),
            status: 1,
            val: 70.0f,
            idx: 1002,
            serious: 66.0f,
            warning: 103.0f
        ));

        // 평택/대산 지역1 - 독성도 설비이상
        dummyAlarmData.Add(new LogData(
            obsid: 21,
            boardid: 1,  // 2 → 1 (독성도 보드)
            areaName: "평택/대산",
            obsName: "지역1",
            hnsId: 1,    // 독성도 센서
            hnsName: "독성도",  // "1,4-다이옥산" → "독성도"
            dt: DateTime.Now.AddHours(-1),
            status: 0,
            val: 0.0f,
            idx: 1003,
            serious: 0.0f,    // 9999.0f → 0.0f
            warning: 20.0f    // 9999.0f → 20.0f
        ));
    }
}