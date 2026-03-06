using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ModelsReform
{
    [System.Serializable]
    public class AlarmData
    {
        public int ALARM_IDX;
        public int OBS_IDX;
        public string BRD_MODEL_CODE;
        public int? SENSOR_IDX;
        public int ALARM_TYPE;
        public float? OCCURED_VALUE_MEAS;
        public float? OCCURED_VALUE_PRED;
        public float? OCCURED_TH_HI;
        public float? OCCURED_TH_LO;
        public string OCCURED_DT;
        public float? SOLVED_VALUE_MEAS;
        public float? SOLVED_VALUE_PRED;
        public float? SOLVED_TH_HI;
        public float? SOLVED_TH_LO;
        public string SOLVED_DT;
        public string OBS_NAME;
        public string OBS_ADDRESS_TEXT;
        public string SENSOR_NAME;
    }


    public enum AlarmState
    {
        SYSTEM_ERROR = -1,  // 시스템 오류

        NORMAL = 0,     // 정상범위 내, 이상 없음

        TH_HIGH = 1,    // 정상범위 초과(th_hi 초과)
        TH_HIGH_2 = 2,  //RESERVED FOR FUTURE USE
        TH_LOW = 3,     // 정상범위 미달(th_lo 미달)
        TH_LOW_2 = 4,   //RESERVED FOR FUTURE USE

        COM_ERROR = 5,  // 통신 오류
        LIVE_ERROR = 6, // 장비 고장 의심
        ETC_ERROR = 7,  // 기타 오류
    }
}
