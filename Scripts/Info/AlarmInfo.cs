using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info
{
    public class AlarmInfo
    {
        public AlarmInfo() { }
        public AlarmInfo(AlarmData alarmData)
        {
            alarmIdx = alarmData.ALARM_IDX;
            obsIdx = alarmData.OBS_IDX;
            obsNameText = alarmData.OBS_NAME;
            obsAddrText = alarmData.OBS_ADDRESS_TEXT;
            alarmType = (AlarmState)alarmData.ALARM_TYPE;
            boardModelCode = alarmData.BRD_MODEL_CODE;
            sensorIdx = alarmData.SENSOR_IDX;
            sensorName = alarmData.SENSOR_NAME;


            occured = new Snapshot
            {
                valMeas = alarmData.OCCURED_VALUE_MEAS.Value,
                valPred = alarmData.OCCURED_VALUE_PRED,
                thresholdHigh = alarmData.OCCURED_TH_HI.Value,
                thresholdLow = alarmData.OCCURED_TH_LO.Value,
                timestamp = DateTime.ParseExact(alarmData.OCCURED_DT,
                        "yyyyMMddHHmmss",
                        System.Globalization.CultureInfo.InvariantCulture)
            };
            if (!string.IsNullOrEmpty(alarmData.SOLVED_DT))
            {
                solved = new Snapshot
                {
                    valMeas = alarmData.SOLVED_VALUE_MEAS.Value,
                    valPred = alarmData.SOLVED_VALUE_PRED,
                    thresholdHigh = alarmData.SOLVED_TH_HI.Value,
                    thresholdLow = alarmData.SOLVED_TH_LO.Value,
                    timestamp = DateTime.ParseExact(alarmData.SOLVED_DT,
                        "yyyyMMddHHmmss",
                        System.Globalization.CultureInfo.InvariantCulture)
                };
            }
            else
            {
                solved = null;
            }
        }

        public int alarmIdx;
        public int obsIdx;
        public string sensorName;
        public string obsNameText;
        public string obsAddrText;
        public AlarmState alarmType;

        public string boardModelCode;
        public int? sensorIdx;

        public Snapshot occured;
        public Snapshot? solved;

        public struct Snapshot 
        {
            public float valMeas;
            public float? valPred;
            public float thresholdHigh;
            public float thresholdLow;
            public DateTime timestamp;
        }
    }
}
