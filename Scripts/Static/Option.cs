using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;


namespace Onthesys
{

    public class Option : MonoBehaviour
    {
        static Option() => LoadStoredData();
        static void LoadStoredData()
        {
            //DB 주소
            string returnUrl = PlayerPrefs.GetString("dbAddress");
            if (returnUrl == null || returnUrl == "")
            {
                Debug.LogWarning("there is no db address in local storage");
                Option.url = "http://192.168.1.20:2000/";
            }
            else
            {
                Option.url = returnUrl;
            }

            //Debug.LogError("PlayerPrefs.GetInt(\"alarmThreshold\", -1) : " + PlayerPrefs.GetInt("alarmThreshold", -1));
            //alarm threshold 
            int returnStatus = PlayerPrefs.GetInt("alarmThreshold", -1);
            if (returnStatus == -1) 
            {
                Debug.LogWarning("there is no alarmThreshold in local storage");
                Option.alarmThreshold = ToxinStatus.Purple;
            }

            Option.alarmThreshold = (ToxinStatus)returnStatus;
        }

        public static bool ENABLE_DEBUG_CODE = false;

        /// <summary>
        /// 트렌드 UI가 실시간으로 데이터를 받아오는 주기입니다. 단위는 분(MIN)입니다.
        /// 짧아질수록 트렌드가 촘촘해집니다.
        /// </summary>
        public static int TREND_TIME_INTERVAL = 10;

        /// <summary>
        /// 트렌드 UI가 실시간으로 표시할 기간의 길이입니다. 단위는 시간(HOUR)입니다.
        /// </summary>
        public static int TREND_DURATION_REALTIME = 12;
        /// <summary>
        /// 트렌드 UI가 Log데이터에 맞춰 표시할 기간의 길이입니다. 단위는 시간(HOUR)입니다.
        /// </summary>
        public static int TREND_DURATION_LOG = 24;
        public static int TOTAL_TOXIN = 37;
        public static float TOXIN_STATUS_RED = 0.95f;
        public static float TOXIN_STATUS_YELLOW = 0.7f;
        public static float TOXIN_STATUS_GREEN = 0.5f;
        public static int TOXIN_RED_PERCENT = 200;   //200분의1
        public static int TOXIN_YELLOW_PERCENT = 100;   //200분의1

        public static ToxinStatus alarmThreshold = ToxinStatus.Purple;
        public static string url = "http://192.168.1.20:2000/";
    }
}