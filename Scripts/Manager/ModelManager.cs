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
        dbManager.GetAlarmLogsActivated(logs => logs.ForEach(log => this.logDataList.Add(log)));


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
        //DateTime TEST_DT;

        //Debug.Log("CHECKTIME Request start :" + DateTime.Now.ToString("ss.fff"));
        //TEST_DT = DateTime.Now;
        dbManager.GetAlarmLogsChangedInRange(pastTimestamp, newTimestamp, changedList =>
        {
            //Debug.Log("CHECKTIME Request end :" + DateTime.Now.ToString("ss.fff"));
            //seps.Add((float)(DateTime.Now - TEST_DT).TotalSeconds);
            //Debug.Log("CHECKTIME Average :" + seps.Average());
            //float variance = seps.Select(v => (v - seps.Average()) * (v - seps.Average())).Average();
            //float stdDev = (float)Math.Sqrt(variance);
            //Debug.Log("CHECKTIME stdDev :" + stdDev);

            //변경사항들을 신규 알람과 해제된 알람으로 구분
            List<AlarmLogModel> toAddModels = changedList.Where(changed => Convert.ToDateTime(changed.aladt) > pastTimestamp).ToList();
            List<AlarmLogModel> toRemoveModels = changedList.Where(changed => changed.turnoff_flag != null && Convert.ToDateTime(changed.turnoff_dt) > pastTimestamp).ToList();

            //신규 알람을 파싱한 뒤 리스트에 추가
            logDataList.AddRange(toAddModels.Select(toAdd => LogData.FromAlarmLogModel(toAdd)));

            //해제된 알람의 idx를 가진 로그데이터들을 제거
            IEnumerable<int> toRemoveIndexes = toRemoveModels.Select(toRemove => toRemove.alaidx);
            logDataList.RemoveAll(logData => toRemoveIndexes.Contains(logData.idx));

            pastTimestamp = newTimestamp;

            //알람 로그 리스트에 변화가 발생 시
            if (changedList.Count != 0)
            {
                Debug.Log($"ModelManager - GetAlarmChangedProcess : 알람 로그 리스트에 변화가 발생했습니다 \n" +
                    $"기존 : {logDataList.Count + toRemoveModels.Count - toAddModels.Count} 신규 : {toAddModels.Count} 해제 : {toRemoveIndexes.ToList().Count} 현재 : {logDataList.Count}");
                //ChangeAlarmList 이벤트
                uiManager.Invoke(UiEventType.ChangeAlarmList);
            }
            else 
            {
                Debug.Log($"ModelManager - GetAlarmChangedProcess : 알람 로그 리스트에 변화가 없습니다.\n" +
                    $"기존 : {logDataList.Count + toRemoveModels.Count - toAddModels.Count} 신규 : {toAddModels.Count} 해제 : {toRemoveIndexes.ToList().Count} 현재 : {logDataList.Count}");
            }
        });

        DOVirtual.DelayedCall(30, GetAlarmChangedProcess);
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

    /*private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        currentObsId = obsId;

        dbManager.GetToxinData(obsId, toxins =>
        {
            DateTime endTime = Option.ENABLE_DEBUG_CODE ? DateTime.Now.AddDays(20) : DateTime.Now;
            DateTime startTime = endTime.AddDays(-1);

            dbManager.GetChartValue(obsId, startTime, endTime, Option.TREND_TIME_INTERVAL, chartDatas =>
            {
                toxins.ForEach(model =>
                {
                    if (chartDatas.Count <= 0) Debug.LogWarning("OnNavigateObs : 얻은 데이터의 원소 수가 0입니다. 차트를 정상적으로 표시할 수 없습니다. \nDB에 존재하지 않는 값이나 잘못된 범위를 지정했습니다.");

                    var values = chartDatas
                        .Where(t => t.boardidx == model.boardid && t.hnsidx == model.hnsid)
                        .Select(t => t.val).ToList();

                    int nodeCount = (int)((endTime - startTime) / TimeSpan.FromMinutes(Option.TREND_TIME_INTERVAL));

                    while (values.Count < nodeCount)
                        values.Insert(0, 0f);

                    model.values = values;
                });

                // 추가: 실시간 값도 즉시 가져오기
                dbManager.GetToxinValueLast(obsId, currents =>
                {
                    foreach (ToxinData toxin in toxins)
                    {
                        CurrentDataModel current = currents.FirstOrDefault(c =>
                            c.boardidx == toxin.boardid && c.hnsidx == toxin.hnsid);

                        if (current != null)
                        {
                            toxin.UpdateValue(current); // 실시간 값 추가
                            Debug.Log($"초기 실시간 값 설정: 보드{toxin.boardid} 센서{toxin.hnsid} {toxin.hnsName} = {current.val}");
                        }
                        else
                        {
                            Debug.LogWarning($"초기 실시간 값 없음: 보드{toxin.boardid} 센서{toxin.hnsid} {toxin.hnsName}");
                        }
                    }

                    // 알람 상태 반영
                    logDataList.Where(t => t.obsId == obsId).ToList().ForEach(ala =>
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
                    });

                    currentObsId = obsId;

                    this.toxins.Clear();
                    this.toxins.AddRange(toxins);

                    // UI 업데이트
                    UiManager.Instance.Invoke(UiEventType.ChangeSensorList);
                    UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);

                    var defaultSensor = toxins.FirstOrDefault(t => t.boardid == 1);
                    if (defaultSensor != null)
                    {
                        UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (defaultSensor.boardid, defaultSensor.hnsid));
                    }
                });
            });
        });
    }*/
    
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

            currentChartEndTime = endTime;
            //Debug.LogError($"[3단계] 차트 데이터 요청: {startTime:HH:mm} ~ {endTime:HH:mm}");
            dbManager.GetChartValue(obsId, startTime, endTime, Option.TREND_TIME_INTERVAL, chartDatas =>
            {
                //Debug.LogError($"[4단계] 차트 데이터 받음: {chartDatas.Count}개");
                toxins.ForEach(model =>
                {
                    if (chartDatas.Count <= 0) Debug.LogWarning("OnNavigateObs : 얻은 데이터의 원소 수가 0입니다. 차트를 정상적으로 표시할 수 없습니다. \nDB에 존재하지 않는 값이나 잘못된 범위를 지정했습니다.");

                    // 해당 센서의 차트 데이터만 필터링
                    var chartDataForSensor = chartDatas
                        .Where(t => t.boardidx == model.boardid && t.hnsidx == model.hnsid)
                        .ToList();

                    // 기존 측정값
                    var values = chartDataForSensor.Select(t => t.val).ToList();

                    // AI값 추가
                    var aiValues = chartDataForSensor.Select(t => t.aival).ToList();

                    // 편차값 추가 (측정값 - AI값의 절댓값)
                    var diffValues = chartDataForSensor.Select(t => Math.Abs(t.val - t.aival)).ToList();

                    int nodeCount = (int)((endTime - startTime) / TimeSpan.FromMinutes(Option.TREND_TIME_INTERVAL));

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
                });
                //Debug.LogError($"[5단계] Values 설정 완료");
                var alarmCount = logDataList.Where(t => t.obsId == obsId).Count();
                try
                {
                    logDataList.Where(t => t.obsId == obsId).ToList().ForEach(ala =>
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

        LogData log = logDataList.Find(logData => logData.idx == alarmId);

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
                Debug.Log($"=== 126개 데이터 종류 분석 ===");
                Debug.Log($"총 데이터 개수: {chartDatas.Count}");

                // 보드별 그룹화
                var boardGroups = chartDatas.GroupBy(d => d.boardidx).ToList();
                Debug.Log($"보드 종류 개수: {boardGroups.Count}");

                foreach (var boardGroup in boardGroups)
                {
                    Debug.Log($"\n--- Board {boardGroup.Key} ---");
                    Debug.Log($"데이터 개수: {boardGroup.Count()}");

                    // 각 보드 내에서 HNS별 그룹화
                    var hnsGroups = boardGroup.GroupBy(d => d.hnsidx).ToList();
                    Debug.Log($"HNS 종류 개수: {hnsGroups.Count}");

                    foreach (var hnsGroup in hnsGroups)
                    {
                        var firstData = hnsGroup.First();
                        Debug.Log($"  HNS {hnsGroup.Key}: {hnsGroup.Count()}개 데이터");
                        Debug.Log($"    샘플 값: val={firstData.val}, aival={firstData.aival}");
                        Debug.Log($"    시간 범위: {hnsGroup.Min(d => d.obsdt)} ~ {hnsGroup.Max(d => d.obsdt)}");
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

                try
                {
                    Debug.LogError($"OnSelectAlarm - 알람 상태 반영 시작");

                    logDataList.Where(t => t.obsId == log.obsId).ToList().ForEach(ala =>
                    {
                        //Debug.LogError($"OnSelectAlarm - 알람 처리: status={ala.status}, boardId={ala.boardId}, hnsId={ala.hnsId}");

                        if (ala.status == 0)
                        {
                            var targetToxins = toxins.Where(t => t.boardid == ala.boardId && t.status != ToxinStatus.Red).ToList();
                            //Debug.LogError($"OnSelectAlarm - 설비이상 대상: {targetToxins.Count}개");
                            targetToxins.ForEach(t => {
                                if (t != null) t.status = ToxinStatus.Yellow;
                            });
                        }
                        else
                        {
                            var targetToxin = toxins.FirstOrDefault(t => t.boardid == ala.boardId && t.hnsid == ala.hnsId);
                            if (targetToxin != null)
                            {
                                //Debug.LogError($"OnSelectAlarm - 센서 알람 설정: Board{ala.boardId}, HNS{ala.hnsId}");
                                targetToxin.status = ToxinStatus.Red;
                            }
                            else
                            {
                                //Debug.LogError($"OnSelectAlarm - 센서 찾기 실패: Board{ala.boardId}, HNS{ala.hnsId}");
                            }
                        }
                    });

                   // Debug.LogError($"OnSelectAlarm - 알람 상태 반영 완료");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ OnSelectAlarm - 알람 상태 반영 실패: {ex.Message}");
                }

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
        
        //현재 센서들을 정상으로 초기화
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

        uiManager.Invoke(UiEventType.ChangeSensorStatus);
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

    #endregion [EventListener]

    #region [DataStructs]

    int currentObsId = -1;

    List<ObsData> obss = new();
    List<ToxinData> toxins = new();
    List<ToxinData> logToxins = new();
    List<ToxinData> settingToxins = new();
    List<LogData> logDataList = new();
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
        List<LogData> obsLogs = logDataList.FindAll(log => log.obsId == obsId);
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
    public List<LogData> GetAlarms() => logDataList;

    public LogData GetAlarm(int alarmId) => logDataList.Find(log => log.idx == alarmId);

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
        List<LogData> sensorLogs = GetAlarms().FindAll(log => log.hnsId == hnsId && log.obsId == obsId && log.boardId == boardId);

        sensorLogs.ForEach(log => {

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
