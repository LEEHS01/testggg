using NUnit.Framework;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ModelProvider
{
    public ObsData GetObs(int obsId);
    public List<ObsData> GetObss();
    public List<ObsData> GetObssByAreaId(int areaId);
    public ToxinStatus GetObsStatus(int obsId);
    public ObsData GetObsByName(string obsName);

    public List<AreaData> GetAreas();
    public AreaData GetArea(int areaId);
    public ToxinStatus GetAreaStatus(int areaId);
    public AreaData GetAreaByName(string areaName);

    public ToxinData GetToxin(int boardId, int hnsId);
    public List<ToxinData> GetToxins();
    public List<ToxinData> GetToxinsInLog();
    public List<ToxinData> GetToxinsSetting();

    /*public List<LogData> GetAlarms();*/
    public List<LogData> GetAlarmsForDisplay();  // UI용
    public List<LogData> GetActiveAlarms();      // 상태 계산용
    public LogData GetAlarm(int alarmId);

    public List<(int areaId, int count)> GetAlarmMonthly();
    public List<(int areaId, AlarmCount counts)> GetAlarmYearly();
    public List<AlarmSummaryModel> GetAlarmSummary();

    public AlarmCount GetObsStatusCountByAreaId(int areaId);

    public ToxinStatus GetSensorStatus(int obsId, int boardId, int hnsId);

    public DateTime GetCurrentChartEndTime();

    public int GetCurrentObsId();
}
