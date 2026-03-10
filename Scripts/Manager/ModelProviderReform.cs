using Assets.Scripts.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Manager
{
    public interface ModelProvider
    {
        public int GetCurrentObsIdx();
        public List<ObservatoryInfo> GetObss();
        public ObservatoryInfo GetObsByIdx(int obsIdx);

        public List<AlarmInfo> GetAlarmsWhole();
        public List<AlarmInfo> GetAlarmsActivated();
        public List<GroupInfo> GetGroups();
        public List<BoardSpecInfo> GetBoardSpecs();
        public TimeSeriesInfo GetCurrentTimeSeriesInfo();
        public List<(DateTime timestamp, (float val, bool isMissing) value)> GetHistoryTimeSeriesInfo();
        public GroupInfo GetGroupByIdx(int groupIdx);
        public AlarmInfo GetAlarmByIdx(int alarmIdx);

        public ((int year, int month, List<int> cnts) prev, (int year, int month, List<int> cnts) cur) GetEventsComparisonInfo();
    }
}
