using Assets.Scripts.Data;
using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;
//using static UnityEditor.Progress;
//using static UnityEngine.EventSystems.EventTrigger;
//using static UnityEngine.InputManagerEntry;

public class ModelManager : MonoBehaviour, ModelProvider
{
    #region [Singleton]
    public static ModelProvider Instance = null;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    #endregion [Singleton]

    #region [Instantiating]
    public DbManager dbManager;
    public UiManager uiManager;

    private void Start()
    {
        //Get Core Components
        dbManager = GetComponent<DbManager>();
        uiManager = GetComponent<UiManager>();

        //Load Datas
        dbManager.GetObss(obss => obss.ForEach(obs => this.obss.Add(obs)));
        dbManager.GetAreas(areas => 
        {
            areas.ForEach(area => this.areas.Add(area));

            dbManager.GetAlarmMonthly(monthModels =>
            {
                alarmMonthly = new();
                monthModels.ForEach(model => 
                    this.alarmMonthly.Add((GetAreaByName(model.areanm).areaId, model.cnt)));
            });

            dbManager.GetAlarmYearly(yearModels => 
            {
                alarmYearly = new();
                yearModels.ForEach(model => 
                    this.alarmYearly.Add((GetAreaByName(model.areanm).areaId, new(0, model.ala1, model.ala2, model.ala0))));
            });
        });
        dbManager.GetCurrentAlarms(logs => logs.ForEach(log => this.currentAlarms.Add(log)));
        dbManager.GetHistoricalAlarms(logs => logs.ForEach(log => this.historicalAlarms.Add(log)));



        //Register Events
        uiManager.Register(UiEventType.SelectAlarm, OnSelectAlarm);
        uiManager.Register(UiEventType.NavigateHome, OnNavigateHome);
        uiManager.Register(UiEventType.NavigateArea, OnNavigateArea);
        uiManager.Register(UiEventType.NavigateObs, OnNavigateObs);
        uiManager.Register(UiEventType.Initiate, OnInitiate);
        uiManager.Register(UiEventType.SelectSettingObs, OnSelectSettingObs);
        uiManager.Register(UiEventType.ChangeAlarmList, OnChangeAlarmList);
        uiManager.Register(UiEventType.CommitBoardFixing, OnCommitBoardFixing);
        uiManager.Register(UiEventType.CommitSensorUsing, OnCommitSensorUsing);
        uiManager.Register(UiEventType.CommitCctvUrl, OnCommitCctvUrl);
        uiManager.Register(UiEventType.UpdateThreshold, OnCommitThresholdUpdate);
        AwaitInitiating();
    }


    int initTryCount = 0;
    private void AwaitInitiating()
    {
        initTryCount++;
        if (initTryCount > 5)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, new Exception("DB 연결에 실패했습니다. 환경설정에서 DB 주소를 확인해십시오. 그럼에도 계속 문제가 발생할 시엔 관리자에게 연락하십시오."));
            return;
        }

        bool isInitiated = obss.Count != 0 && areas.Count != 0 && alarmMonthly != null && alarmYearly != null;

        Debug.Log("AwaitInitiating 작동 중 ");
        if (!isInitiated)
            DOVirtual.DelayedCall(1f, AwaitInitiating);
        else
            UiManager.Instance.Invoke(UiEventType.Initiate);
    }

    #endregion [Instantiating]

    #region [Processing]
    void GetTrendValueProcess() 
    {
        Debug.Log($"=== GetTrendValueProcess 시작 ===");
        Debug.Log($"currentObsId: {currentObsId}");
        Debug.Log($"기존 toxins 개수: {toxins.Count}");
        dbManager.GetToxinValueLast(currentObsId, currents =>
        {
            /*foreach (var item in currents)
            {
                Debug.Log($"{item.boardidx}/{item.hnsidx} : {item.useyn} {item.fix}");
            }*/
            if (currents.Count != toxins.Count) Debug.LogWarning("ModelManager - GetTrendValueProcess : currents와 toxins 간의 길이 불일치.");


            toxins.ForEach(toxin =>
            {
                var curr = currents.Find(cur => cur.boardidx == toxin.boardid && cur.hnsidx == toxin.hnsid);
                if (curr == null) throw new Exception("Cant find!");

                toxin.UpdateValue(curr);
            });


            //for (int i = 0; i < toxins.Count; i++) 
            //{
            //    ToxinData toxin = toxins[i];
            //    CurrentDataModel current = currents[i];


            //    if (current.hnsidx == 4 && current.boardidx == 3)
            //    {
            //        Debug.LogError($"GetToxinValueLast {current.GetHashCode()} {toxin.on} {current.useyn}");

            //        Debug.Log($"{toxin.boardid}/{toxin.hnsid} : {toxin.on} {toxin.fix}");
            //        Debug.Log($"{current.boardidx}/{current.hnsidx} : {current.useyn} {toxin.fix}");
            //    }
            //        toxin.UpdateValue(current);
            //}
            uiManager.Invoke(UiEventType.ChangeTrendLine);
        });

        //지속적으로 재귀 호출
        //DOVirtual.DelayedCall(Option.ENABLE_DEBUG_CODE? 1 : Option.TREND_TIME_INTERVAL * 60, GetTrendValueProcess);
        DOVirtual.DelayedCall(Option.TREND_TIME_INTERVAL * 60, GetTrendValueProcess);
    }

    //List<float> seps = new();
    DateTime pastTimestamp = DateTime.Now;
    void GetAlarmChangedProcess()
    {
        DateTime newTimestamp = DateTime.Now;

        dbManager.GetAlarmLogsChangedInRange(pastTimestamp, newTimestamp, changedList =>
        {
            changedList = changedList.OrderBy(x => x.alacode).ToList();

            // 신규/해제 알람 구분
            List<AlarmLogModel> toAddModels = changedList.Where(changed =>
                Convert.ToDateTime(changed.aladt) > pastTimestamp).ToList();

            List<AlarmLogModel> toRemoveModels = changedList.Where(changed =>
                changed.turnoff_flag != null && Convert.ToDateTime(changed.turnoff_dt) > pastTimestamp).ToList();

            // currentAlarms 업데이트 (활성 알람만)
            currentAlarms.AddRange(toAddModels.Where(model => string.IsNullOrEmpty(model.turnoff_flag))
                .Select(toAdd => LogData.FromAlarmLogModel(toAdd)));

            IEnumerable<int> toRemoveIndexes = toRemoveModels.Select(toRemove => toRemove.alaidx);
            currentAlarms.RemoveAll(logData => toRemoveIndexes.Contains(logData.idx));

            // historicalAlarms 업데이트 (모든 알람)
            historicalAlarms.AddRange(toAddModels.Select(toAdd => LogData.FromAlarmLogModel(toAdd)));
            DateTime oneWeekAgo = DateTime.Now.AddDays(-7);
            int removedOldCount = historicalAlarms.RemoveAll(logData => logData.time < oneWeekAgo);

            pastTimestamp = newTimestamp;

            //  수정: 알람이 실제로 변경됐을 때만 센서값 업데이트
            bool hasAlarmChanges = toAddModels.Count > 0 || toRemoveModels.Count > 0;

            if (hasAlarmChanges && currentObsId > 0)
            {
                dbManager.GetToxinValueLast(currentObsId, currents =>
                {
                    // 값과 상태를 동시에 업데이트
                    toxins.ForEach(toxin =>
                    {
                        var curr = currents.Find(cur => cur.boardidx == toxin.boardid && cur.hnsidx == toxin.hnsid);
                        if (curr != null)
                        {
                            toxin.UpdateValue(curr);
                        }
                    });

                    // 활성 알람 기반 상태 보정
                    currentAlarms.Where(log => log.obsId == currentObsId).ToList().ForEach(log =>
                    {
                        var toxin = toxins.Find(t => t.boardid == log.boardId && t.hnsid == log.hnsId);
                        if (toxin != null)
                        {
                            ToxinStatus alarmStatus = log.status == 0 ? ToxinStatus.Purple :
                                                      log.status == 1 ? ToxinStatus.Yellow :
                                                      ToxinStatus.Red;

                            if ((int)alarmStatus > (int)toxin.status)
                            {
                                toxin.status = alarmStatus;
                            }
                        }
                    });

                    //  알람 변화가 있을 때만 이벤트 발생
                    uiManager.Invoke(UiEventType.ChangeSensorStatus);
                });
            }

            // 알람 변화가 있을 때만 알람 리스트 업데이트
            if (changedList.Count != 0)
            {
                Debug.Log($"알람 변화 발생: 신규 {toAddModels.Count}, 해제 {toRemoveModels.Count}");
                uiManager.Invoke(UiEventType.ChangeAlarmList);
            }
        });

        // 6초 후 재실행
        DOVirtual.DelayedCall(6, GetAlarmChangedProcess);
    }

    void GetStepProcess() 
    {
        if (currentObsId < 1) 
        {
            DOVirtual.DelayedCall(1, GetStepProcess);
            return;
        }

        dbManager.GetSensorStep(currentObsId, step =>
        {
            uiManager.Invoke(UiEventType.ChangeSensorStep, step);
            /*if(Option.ENABLE_DEBUG_CODE)
                uiManager.Invoke(UiEventType.ChangeSensorStep, (DateTime.Now.Second / 5) % 5 + 1);
            else
                uiManager.Invoke(UiEventType.ChangeSensorStep, step);*/
            //DEBUG! 25초 주기로 1~5 순회

        });

        DOVirtual.DelayedCall(1, GetStepProcess);
    }
    #endregion [Processing]

    #region [EventListener]

    private void OnNavigateHome(object obj)
    {
        currentObsId = -1;
        this.logToxins.Clear();
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmSensorList);
    }

    private void OnNavigateArea(object obj)
    {
        if (obj is not int areaId) return;

        currentObsId = -1;
        this.logToxins.Clear();
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmSensorList);

        alarmSummarys.Clear();

        dbManager.GetAlarmSummary(areaId, summarys =>
        {
            summarys.ForEach(summary => alarmSummarys.Add(summary));

            UiManager.Instance.Invoke(UiEventType.ChangeSummary);
        });

    }
    
    private DateTime currentChartEndTime;
    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;
        this.logToxins.Clear();
        UiManager.Instance.Invoke(UiEventType.ChangeAlarmSensorList);
        //Debug.LogError($"[1단계] ObsId 전달: {obsId}");
        //Debug.Log($"=== OnNavigateObs 디버그 ===");

        currentObsId = obsId;
       //Debug.Log($"새로운 currentObsId: {currentObsId}");
        dbManager.GetToxinData(obsId, toxins =>
        {
            //Debug.LogError($"[2단계] ToxinData 받음: {toxins.Count}개");
            DateTime endTime = Option.ENABLE_DEBUG_CODE ? DateTime.Now.AddDays(20) : DateTime.Now;
            endTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour,(endTime.Minute / 10) * 10, 0);

            DateTime startTime = endTime.AddHours(-12);
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, (startTime.Minute / 10) * 10, 0);

            //Debug.LogError($"[3단계] 차트 데이터 요청: {startTime:HH:mm} ~ {endTime:HH:mm}");
            dbManager.GetChartValue(obsId, startTime, endTime, Option.TREND_TIME_INTERVAL, chartDatas =>
            {
                // 실제 DB 데이터의 최신 시간을 currentChartEndTime으로 설정
                if (chartDatas.Count > 0)
                {
                    var actualLatestTime = chartDatas
                        .Select(d => DateTime.Parse(d.obsdt))
                        .Max(); 

                    currentChartEndTime = actualLatestTime; // 실제 데이터 시간 사용!
                }
                else
                {
                    currentChartEndTime = endTime; // 데이터가 없으면 기존 방식
                }
                //Debug.LogError($"[4단계] 차트 데이터 받음: {chartDatas.Count}개");
                toxins.ForEach(model =>
                {
                    if (chartDatas.Count <= 0) Debug.LogWarning("OnNavigateObs : 얻은 데이터의 원소 수가 0입니다. 차트를 정상적으로 표시할 수 없습니다. \nDB에 존재하지 않는 값이나 잘못된 범위를 지정했습니다.");

                    // 해당 센서의 차트 데이터만 필터링
                    var chartDataForSensor = chartDatas
                        .Where(t => t.boardidx == model.boardid && t.hnsidx == model.hnsid)
                        .OrderBy(t => DateTime.Parse(t.obsdt))
                        .ToList();

                    // 기존 측정값
                    var values = chartDataForSensor.Select(t => t.val).ToList();

                    // AI값 추가
                    var aiValues = chartDataForSensor.Select(t => t.aival).ToList();

                    // 편차값 추가 (측정값 - AI값의 절댓값)
                    var diffValues = chartDataForSensor.Select(t => Math.Abs(t.val - t.aival)).ToList();

                    int nodeCount = (int)((endTime - startTime) / TimeSpan.FromMinutes(Option.TREND_TIME_INTERVAL));

                    // 측정 시간 값 저장
                    var dateTimes = chartDataForSensor.Select(t => DateTime.Parse(t.obsdt)).ToList();
                    // 부족한 데이터는 0으로 채우기
                    while (values.Count < nodeCount)
                    {
                        values.Insert(0, 0f);
                        aiValues.Insert(0, 0f);
                        diffValues.Insert(0, 0f);
                    }

                    // 모델에 저장
                    model.values = values;
                    model.aiValues = aiValues;
                    model.diffValues = diffValues;
                    model.dateTimes = dateTimes; 
                });
                //Debug.LogError($"[5단계] Values 설정 완료");
                var alarmCount = currentAlarms.Where(t => t.obsId == obsId).Count();
                try
                {
                    currentAlarms.Where(t => t.obsId == obsId).ToList().ForEach(ala =>
                    {
                        //Debug.LogError($"[6-1] 알람 처리 중: status={ala.status}, boardId={ala.boardId}, hnsId={ala.hnsId}");

                        if (ala.status == 0)
                        {
                            var targetToxins = toxins.Where(t => t.boardid == ala.boardId && t.status != ToxinStatus.Red).ToList();
                            //Debug.LogError($"[6-2] 설비이상 대상 센서: {targetToxins.Count}개");
                            targetToxins.ForEach(t => {
                                if (t != null) t.status = ToxinStatus.Yellow;
                            });
                        }
                        else
                        {
                            var targetToxin = toxins.FirstOrDefault(t => t.boardid == ala.boardId && t.hnsid == ala.hnsId);
                            if (targetToxin != null)
                            {
                                //Debug.LogError($"[6-3] 센서 알람 설정: Board{ala.boardId}, HNS{ala.hnsId}");
                                targetToxin.status = ToxinStatus.Red;
                            }
                            else
                            {
                                //Debug.LogError($"[6-3] 센서 찾기 실패: Board{ala.boardId}, HNS{ala.hnsId}");
                            }
                        }
                    });

                    //Debug.LogError($"[6-완료] 알람 상태 반영 완료");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[6-오류] 알람 상태 반영 실패: {ex.Message}");
                }
                /*logDataList.Where(t => t.obsId == obsId).ToList().ForEach(ala =>
                {
                    if (ala.status == 0)
                    {
                        toxins
                        .Where(t => t.boardid == ala.boardId && t.status != ToxinStatus.Red).ToList()
                        .ForEach(t => t.status = ToxinStatus.Yellow);
                    }
                    else
                    {
                        toxins
                        .FirstOrDefault(t => t.boardid == ala.boardId && t.hnsid == ala.hnsId)
                        .status = ToxinStatus.Red;
                    }
                });*/

                dbManager.GetToxinValueLast(obsId, currents =>
                {
                    //Debug.LogError($"[7단계] 실시간 값 받음: {currents.Count}개");
                    int updateCount = 0;
                    toxins.ForEach(toxin =>
                    {
                        var curr = currents.Find(cur => cur.boardidx == toxin.boardid && cur.hnsidx == toxin.hnsid);
                        if (curr != null)
                        {
                            toxin.UpdateValue(curr);
                            updateCount++;
                        }
                    });

                    //Debug.LogError($"[8단계] 실시간 값 업데이트: {updateCount}개");
                    toxins.ForEach(toxin =>
                    {
                        var curr = currents.Find(cur => cur.boardidx == toxin.boardid && cur.hnsidx == toxin.hnsid);
                        if (curr != null) toxin.UpdateValue(curr);
                    });

                    // 전역 저장은 실시간 값 업데이트 후에
                    this.toxins.Clear();
                    this.toxins.AddRange(toxins);
                    //Debug.LogError($"[9단계] 전역 저장 완료: {this.toxins.Count}개");
                    // UI 업데이트
                    UiManager.Instance.Invoke(UiEventType.ChangeSensorList);
                    UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
                    //Debug.LogError($"[10단계] UI 이벤트 발생 완료");
                    var defaultSensor = toxins.FirstOrDefault(t => t.boardid == 1);
                    if (defaultSensor != null)
                    {
                        //Debug.LogError($"[11단계] 기본센서 선택: Board{defaultSensor.boardid}, HNS{defaultSensor.hnsid}");
                        UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (defaultSensor.boardid, defaultSensor.hnsid));
                    }
                });

                /*  currentObsId = obsId;

                  this.toxins.Clear();
                  this.toxins.AddRange(toxins);

                  UiManager.Instance.Invoke(UiEventType.ChangeSensorList);
                  UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);*/

                /* var defaultSensor = toxins.FirstOrDefault(t => t.boardid == 1);
                 if (defaultSensor != null)
                 {
                     UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (defaultSensor.boardid, defaultSensor.hnsid));
                 }*/
            });


        });

    }

    private void OnSelectAlarm(object obj)
    {
        if (obj is not int alarmId) return;

        Debug.Log($"OnSelectAlarm 호출됨: alarmId={alarmId}");

        LogData log = historicalAlarms.Find(logData => logData.idx == alarmId);

        if (log == null) throw new Exception($"ModelManager - OnSelectAlarm : 해당 로그의 정보를 찾지 못했습니다. alarm.idx : ({alarmId})");

        //Debug.Log($"=== OnSelectAlarm 시작 ===");
        //Debug.Log($"알람 시간: {log.time}");
        //Debug.Log($"관측소 ID: {log.obsId}");

        dbManager.GetToxinData(log.obsId, toxins => {
            DateTime endTime = log.time;
            endTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, (endTime.Minute / 10) * 10, 0);
            DateTime startTime = endTime.AddHours(-12);
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, (startTime.Minute / 10) * 10, 0);

            Debug.Log($"OnSelectAlarm 차트 데이터 요청: {startTime} ~ {endTime}");
            dbManager.GetChartValue(log.obsId, startTime, endTime, Option.TREND_TIME_INTERVAL, chartDatas =>
            {

                // 보드별 그룹화
                var boardGroups = chartDatas.GroupBy(d => d.boardidx).ToList();
                //Debug.Log($"보드 종류 개수: {boardGroups.Count}");

                foreach (var boardGroup in boardGroups)
                {
                    // 각 보드 내에서 HNS별 그룹화
                    var hnsGroups = boardGroup.GroupBy(d => d.hnsidx).ToList();

                    foreach (var hnsGroup in hnsGroups)
                    {
                        var firstData = hnsGroup.First();
                    }
                }
                int countExpected = Mathf.FloorToInt((Option.TREND_DURATION_LOG * 60f) / Option.TREND_TIME_INTERVAL);

                toxins.ForEach(model =>
                {
                    if (chartDatas.Count <= 0) Debug.LogWarning("ModelManager - OnSelectAlarm : 해당 범위에서 차트에 표출한 데이터들을 찾지 못했습니다.");

                    var lval = chartDatas.Where(t => t.boardidx == model.boardid && t.hnsidx == model.hnsid).OrderBy(e => e.obsdt).Select(e => new
                    {
                        dt = DateTime.Parse(e.obsdt),
                        e.val,
                        e.aival,
                        difval = Math.Abs(e.val - e.aival)
                    }).ToList();

                    for (int i = 1; i <= countExpected; i++)
                    {
                        var time = Truncate(startTime.AddMinutes(Option.TREND_TIME_INTERVAL * i), TimeSpan.FromMinutes(1.0));

                        var values = lval.FirstOrDefault(t => t.dt == time);

                        if (values == null)
                        {
                            model.values.Add(0f);
                            model.aiValues.Add(0f);
                            model.diffValues.Add(0f);
                        }
                        else
                        {
                            model.values.Add(values.val);
                            model.aiValues.Add(values.aival);
                            model.diffValues.Add(values.difval);
                        }
                    }
                });

                var alarmSensor = toxins.FirstOrDefault(t =>
                t.boardid == log.boardId && t.hnsid == log.hnsId);

                if (alarmSensor != null && alarmSensor.values.Count > 0 && log.value.HasValue)
                {
                    alarmSensor.values[alarmSensor.values.Count - 1] = log.value.Value;
                }
                // 상태 초기화
                toxins.ForEach(t => t.status = ToxinStatus.Green);

                // 모든 센서의 마지막 값으로 임계값 기반 상태 계산
                toxins.ForEach(toxin =>
                {
                    if (toxin.values.Count > 0)
                    {
                        float lastValue = toxin.GetLastValue();

                        // ToxinData의 올바른 임계값 필드 사용
                        if (lastValue >= toxin.warning)  // 경보 임계값
                            toxin.status = ToxinStatus.Red;
                        else if (lastValue >= toxin.serious) // 경계 임계값
                            toxin.status = ToxinStatus.Yellow;

                    }

                });

                /*try
                {
                    Debug.LogError($"OnSelectAlarm - 알람 상태 반영 시작");

                    currentAlarms.Where(t => t.obsId == log.obsId).ToList().ForEach(ala =>
                    {
                        if (ala.hnsName.Contains("Temperature"))
                        {
                            Debug.Log($"Temperature 처리 중: ala.status={ala.status}");
                        }

                        if (ala.status == 0)
                        {
                            // 설비이상 처리
                        }
                        else
                        {
                            var targetToxin = toxins.FirstOrDefault(t => t.boardid == ala.boardId && t.hnsid == ala.hnsId);
                            if (targetToxin != null)
                            {
                                if (ala.hnsName.Contains("Temperature"))
                                {
                                    Debug.Log($"Temperature switch 전 상태: {targetToxin.status}");
                                    Debug.Log($"Temperature ala.status: {ala.status}");
                                }

                                switch (ala.status)
                                {
                                    case 1: // 경계
                                        targetToxin.status = ToxinStatus.Yellow;
                                        if (ala.hnsName.Contains("Temperature"))
                                            Debug.Log($"Temperature case 1: Yellow 설정됨");
                                        break;
                                    case 2: // 경보
                                        targetToxin.status = ToxinStatus.Red;
                                        if (ala.hnsName.Contains("Temperature"))
                                            Debug.Log($"Temperature case 2: Red 설정됨");
                                        break;
                                    default:
                                        if (ala.hnsName.Contains("Temperature"))
                                            Debug.Log($"Temperature default case: ala.status={ala.status}");
                                        break;
                                }

                                if (ala.hnsName.Contains("Temperature"))
                                {
                                    Debug.Log($"Temperature switch 후 상태: {targetToxin.status}");
                                }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ OnSelectAlarm - 알람 상태 반영 실패: {ex.Message}");
                }
*/
                currentObsId = log.obsId;

                this.logToxins.Clear();
                this.logToxins.AddRange(toxins);

                //UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, 0);
                UiManager.Instance.Invoke(UiEventType.ChangeAlarmSensorList);

                /*var defaultSensor = toxins.FirstOrDefault(t => t.boardid == 1);
                if (defaultSensor != null)
                {
                    UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (defaultSensor.boardid, defaultSensor.hnsid));
                }*/
            });
        });
    }

    private void OnInitiate(object obj)
    {
        GetTrendValueProcess();
        GetAlarmChangedProcess();
        GetStepProcess(); 

    }

    private void OnSelectSettingObs(object obj) 
    {
        if (obj is not int obsId) return;

        //currentObsId = obsId;

        dbManager.GetToxinData(obsId, toxins =>
        {
            this.settingToxins.Clear();
            this.settingToxins.AddRange(toxins);

            UiManager.Instance.Invoke(UiEventType.ChangeSettingSensorList);
        });
    }

    private void OnChangeAlarmList(object obj) 
    {
        //연간 알람 갱신
        dbManager.GetAlarmMonthly(monthModels => {
            alarmMonthly.Clear();
            alarmMonthly.AddRange(
                monthModels.Select(model => 
                (GetAreaByName(model.areanm).areaId, model.cnt)));

            monthModels.ForEach(item => Debug.Log(item.areanm + " - " + item.cnt));

            uiManager.Invoke(UiEventType.ChangeAlarmMonthly);
        });

        //월간 알람 갱신
        dbManager.GetAlarmYearly(yearModels => {
            alarmYearly.Clear();

            alarmYearly.AddRange(
                yearModels.Select(model =>
                    (GetAreaByName(model.areanm).areaId, new AlarmCount(0, model.ala1, model.ala2, model.ala0))));

            uiManager.Invoke(UiEventType.ChangeAlarmYearly);
        });

        //센서 상태값 갱신
        if (currentObsId <= 0) return;

        // *** 핵심: 알람 변경 시 즉시 최신 센서값 가져오기 ***
        dbManager.GetToxinValueLast(currentObsId, currents =>
        {
            var c = currents.FirstOrDefault(x => x.boardidx == 3 && x.hnsidx == 1);
            Debug.Log($"[CURRENT] obs=1, board=3, hns=1 val={c?.val}");
            // 1. 센서 상태 초기화
            toxins.ForEach(t => t.status = ToxinStatus.Green);

            // 2. 최신 센서값 업데이트
            toxins.ForEach(toxin =>
            {
                var curr = currents.Find(cur => cur.boardidx == toxin.boardid && cur.hnsidx == toxin.hnsid);
                if (curr != null)
                {
                    toxin.UpdateValue(curr); // 최신값으로 업데이트
                }
            });

            // 3. 알람 상태 반영
            //수정전
            List<LogData> logsInCurrentObs = currentAlarms.Where(log => log.obsId == currentObsId).ToList();

            // 수정 후 - 활성 알람만 필터링
            /*List<LogData> logsInCurrentObs = logDataList.Where(log =>
                log.obsId == currentObsId &&
                !log.isCancelled  // 해제되지 않은 알람만
            ).ToList();*/
            logsInCurrentObs.ForEach(log => {
                ToxinStatus logStatus = log.status == 0 ? ToxinStatus.Purple : (ToxinStatus)log.status;
                ToxinData toxin = toxins.Find(t => t.boardid == log.boardId && (logStatus == ToxinStatus.Purple || t.hnsid == log.hnsId));

                if (toxin != null)
                {
                    toxin.status = (ToxinStatus)Math.Max((int)logStatus, (int)toxin.status);
                }
            });

            // 4. UI 업데이트 (상태 + 값 모두)
            uiManager.Invoke(UiEventType.ChangeSensorStatus);
            //uiManager.Invoke(UiEventType.ChangeTrendLine);
        });

        /*//현재 센서들을 정상으로 초기화
        toxins.ForEach(t => t.status = ToxinStatus.Green);

        //현 관측소 알람만 
        List<LogData> logsInCurrentObs = logDataList.Where(log => log.obsId == currentObsId).ToList();
        logsInCurrentObs.ForEach(log => {
            ToxinStatus logStatus =  log.status == 0? ToxinStatus.Purple : (ToxinStatus)log.status;

            //같은 보드이고, 신규 상태가 설비 이상이거나 센서값이 같을 때
            ToxinData toxin = toxins.Find(t => t.boardid == log.boardId && (logStatus == ToxinStatus.Purple) || t.hnsid == log.hnsId);

            if (toxin == null) return;
            //가장 높은 단계의 상태를 표시
            toxin.status = (ToxinStatus)Math.Max((int)logStatus, (int)toxin.status);

        });

        uiManager.Invoke(UiEventType.ChangeSensorStatus);*/
    }

    private void OnCommitBoardFixing(object obj)
    {
        if (obj is not (int obsId, int boardId, bool isFixing)) return;

        //환경설정 센서 갱신
        GetToxinsSetting()
            .Where(item => item.boardid == boardId).ToList()
            .ForEach(item => item.fix = isFixing);

        //모니터링 센서 갱신
        if (currentObsId == obsId)
            GetToxins()
                .Where(item => item.boardid == boardId).ToList()
                .ForEach(item => item.fix = isFixing);

        dbManager.SetBoardFixing(obsId, boardId, isFixing, () => 
            dbManager.GetBoardFixing(obsId,boardId, result =>
                Debug.Log($"CommitBoardFixing - called : {isFixing} / result : {result}")));
    }

    private void OnCommitSensorUsing(object obj)
    {
        if (obj is not (int obsId, int boardId, int sensorId, bool isUsing)) return;

        //환경설정 센서 갱신
        GetToxinsSetting()
            .Find(item => item.boardid == boardId && item.hnsid == sensorId)
            .fix = isUsing;

        //모니터링 센서 갱신
        if (currentObsId == obsId)
            GetToxin(boardId, sensorId).fix = isUsing;

        dbManager.SetSensorUsing(obsId, boardId, sensorId, isUsing, () =>
            dbManager.GetSensorUsing(obsId, boardId, sensorId, result =>
                Debug.Log($"CommitSensorUsing - called : {isUsing} / result : {result}")));
    }
    private void OnCommitCctvUrl(object obj)
    {
        if (obj is not (int obsId, CctvType cctvType, string url)) return;

        ObsData obs = GetObs(obsId);
        switch (cctvType)
        {
            case CctvType.EQUIPMENT:
                obs.src_video1 = url;
                break;
            case CctvType.OUTDOOR:
                obs.src_video2 = url;
                break;
        }

        dbManager.SetObsCctv(obsId, cctvType, url, () => Debug.Log($"CommitCctvUrl - completed"));
    }

    private void OnCommitThresholdUpdate(object obj)
    {
        if (obj is not (int obsId, int boardId, int hnsId, string column, float value)) return;

        // 환경설정 센서 갱신
        var toxin = GetToxinsSetting()
            .Find(item => item.boardid == boardId && item.hnsid == hnsId);

        if (toxin == null) return;

        // 로컬 데이터 업데이트
        if (column == "ALAHIVAL") toxin.serious = value;
        else if (column == "ALAHIHIVAL") toxin.warning = value;

        // 모니터링 센서도 갱신 (현재 관측소인 경우)
        if (currentObsId == obsId)
        {
            var monitoringToxin = GetToxin(boardId, hnsId);
            if (monitoringToxin != null)
            {
                if (column == "ALAHIVAL") monitoringToxin.serious = value;
                else if (column == "ALAHIHIVAL") monitoringToxin.warning = value;
            }
        }

        // DB 업데이트
        UpdateColumn updateColumn = column == "ALAHIVAL" ? UpdateColumn.ALAHIVAL : UpdateColumn.ALAHIHIVAL;
        dbManager.SetToxinDataProperty(obsId, toxin, updateColumn, () =>
        {
            Debug.Log($"CommitThresholdUpdate - {column} = {value} 완료");
            // UI 새로고침 이벤트 발생
            uiManager.Invoke(UiEventType.ChangeSettingSensorList);
        });
    }
    #endregion [EventListener]

    #region [DataStructs]

    int currentObsId = -1;

    List<ObsData> obss = new();
    List<ToxinData> toxins = new();
    List<ToxinData> logToxins = new();
    List<ToxinData> settingToxins = new();
    //List<LogData> logDataList = new(); //삭제프로시저 변경
    List<LogData> currentAlarms = new();     // 활성 알람만 (상태 계산용)
    List<LogData> historicalAlarms = new();  // 7일치 모든 알람 (UI 표시용)
    List<AreaData> areas = new();

    List<AlarmSummaryModel> alarmSummarys = new();
    List<(int areaId, int count)> alarmMonthly;
    List<(int areaId, AlarmCount counts)> alarmYearly;

    #endregion [DataStructs]

    #region [ModelProvider]
    public ObsData GetObs(int obsId) => obss.Find(obs => obs.id == obsId);

    public List<ObsData> GetObss() => obss;

    public List<ObsData> GetObssByAreaId(int areaId) => obss.FindAll(obs => obs.areaId == areaId);

    public ToxinStatus GetObsStatus(int obsId)
    {
        ToxinStatus status = ToxinStatus.Green;

        //해당 관측소의 로그를 모두 가져옴
        List<LogData> obsLogs = currentAlarms.FindAll(log => log.obsId == obsId);
        List<ToxinStatus> transitionCondition;

        //로그를 순회하며 가장 높은 경고 단계를 탐색
        foreach (var log in obsLogs)
            switch (log.status)
            {
                case 1: //경고
                    transitionCondition = new() { ToxinStatus.Green };
                    if (transitionCondition.Contains(status))
                        status = ToxinStatus.Yellow;
                    break;
                case 2: //경보
                    transitionCondition = new() { ToxinStatus.Green, ToxinStatus.Yellow };
                    if (transitionCondition.Contains(status))
                        status = ToxinStatus.Red;
                    break;
                case 0: //설비이상
                    transitionCondition = new() { ToxinStatus.Green, ToxinStatus.Yellow, ToxinStatus.Red };
                    if (transitionCondition.Contains(status))
                        status = ToxinStatus.Purple;
                    break;
                default:
                    throw new Exception("ModelManager - GetObsStatus : 사전에 정의되지 않은 에러 코드를 사용하고 있습니다. 오류 코드는 다음의 범위 안에 있어야 합니다. (0,1,2) \n 입력된 오류 코드:" + log.status);
            }

        //반환
        return status;
    }

    public List<AreaData> GetAreas() => areas;

    public AreaData GetArea(int areaId) => areas.Find(area => area.areaId == areaId);
    public ToxinStatus GetAreaStatus(int areaId)
    {
        ToxinStatus highestStatus = ToxinStatus.Green;

        //지역 내 관측소들을 순회하며 가장 높은 수준의 알람을 탐색
        var obssInArea = GetObssByAreaId(areaId);
        obssInArea.ForEach(obs =>
            highestStatus = (ToxinStatus)Math.Max((int)highestStatus, (int)GetObsStatus(obs.id))
        );

        return highestStatus;
    }

    //hnsId는 각 센서의 고유한 값을 나타내지 못함. 1.4 다이옥신(보드2, id1)과 독성도(보드1, id1)간의 중복이 발생함. 때문에 ToxinData를 int로 특정할 때, 독성도의 id는 0을 사용하게끔 수정
    public ToxinData GetToxin(int boardId, int hnsId)
    {
        return this.toxins.Find(t => t.boardid == boardId && t.hnsid == hnsId);
    }

    public List<ToxinData> GetToxins() => toxins;

    public List<ToxinData> GetToxinsInLog() => logToxins;
    public List<ToxinData> GetToxinsSetting() => settingToxins;

    public List<LogData> GetAlarmsForDisplay() => historicalAlarms;  // UI용
    public List<LogData> GetActiveAlarms() => currentAlarms;        // 상태 계산용

    public LogData GetAlarm(int alarmId) => historicalAlarms.Find(log => log.idx == alarmId); // UI에서 사용

    //public List<LogData> GetAlarms() => logDataList;

    //public LogData GetAlarm(int alarmId) => logDataList.Find(log => log.idx == alarmId);

    public List<(int areaId, int count)> GetAlarmMonthly() => alarmMonthly;

    public List<(int areaId, AlarmCount counts)> GetAlarmYearly() => alarmYearly;

    public List<AlarmSummaryModel> GetAlarmSummary() => alarmSummarys;

    public AlarmCount GetObsStatusCountByAreaId(int areaId)
    {
        AlarmCount obsCounts = new(0, 0, 0, 0);

        //지역 내 관측소들을 순회하며 갯수를 세기
        var obssInArea = GetObssByAreaId(areaId);
        obssInArea.ForEach(obs => {
            ToxinStatus obsStatus = GetObsStatus(obs.id);
            switch (obsStatus)
            {
                case ToxinStatus.Green: obsCounts.green++; break;
                case ToxinStatus.Yellow: obsCounts.yellow++; break;
                case ToxinStatus.Red: obsCounts.red++; break;
                case ToxinStatus.Purple: obsCounts.purple++; break;
            }
        });

        return obsCounts;
    }

    public ObsData GetObsByName(string obsName) => obss.Find(obs => obs.obsName == obsName);

    public AreaData GetAreaByName(string areaName) => areas.Find(area => area.areaName == areaName);

    public ToxinStatus GetSensorStatus(int obsId, int boardId, int hnsId)
    {
        ToxinStatus highestStatus = ToxinStatus.Green;

        //지역 내 관측소들을 순회하며 해당 센서의 가장 높은 수준의 알람을 탐색
        ObsData obs = GetObs(obsId);

        // 해제된 알람 출력 하면서 -> 기존에 모든 알람으로 하다보니 모니터A상태값이 해제된알람의 상태값으로 표현되는 문제
        //List<LogData> sensorLogs = GetAlarms().FindAll(log => log.hnsId == hnsId && log.obsId == obsId && log.boardId == boardId);

        List<LogData> sensorLogs = currentAlarms.FindAll(log =>
        log.hnsId == hnsId &&
        log.obsId == obsId &&
        log.boardId == boardId
    );

        sensorLogs.ForEach(log =>
        {

            ToxinStatus logStatus = log.status != 0 ? (ToxinStatus)log.status : ToxinStatus.Purple;

            highestStatus = (ToxinStatus)Math.Max((int)highestStatus, (int)logStatus);
        }
        );
        return highestStatus;
    }

    public DateTime GetCurrentChartEndTime()
    {
        return currentChartEndTime;
    }
    #endregion [ModelProvider]

    DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero)
            return dateTime;
        if (dateTime == DateTime.MinValue ||
            dateTime == DateTime.MaxValue)
            return dateTime;
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

}
