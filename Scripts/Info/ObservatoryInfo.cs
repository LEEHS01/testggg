using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info
{
    public class ObservatoryInfo
    {
        public ObservatoryInfo(ObservatoryData obsData)
        {
            //Debug.Log($"Creating ObservatoryInfo for OBS_IDX: {obsData.OBS_IDX}, NAME: {obsData.NAME}");
            obsIdx = obsData.OBS_IDX;
            nameText = obsData.NAME;
            groupIdx = obsData.GROUP_IDX;
            coordinate = new Vector2(obsData.DP_POS_LON, obsData.DP_POS_LAT);
            addrText = obsData.ADDRESS_TEXT;
            //Debug.Log($"Creating ObservatoryInfo Sensors init");
            var tSensors = new List<(int idx, SensorInfo info)>();
            for (int i = 1; i <= 59; i++)
            {

                string name = (string)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_NAME").GetValue(obsData);
                string unit = (string)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_UNIT").GetValue(obsData);
                float? pvNullable = (float?)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_PV").GetValue(obsData);
                float pv = pvNullable ?? 0f; // null일 경우 0으로 대체
                int? alarmTypeNullable = (int?)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_ALARM_TYPE").GetValue(obsData);
                int alarmType = alarmTypeNullable ?? -1; // null일 경우 0으로 대체
                bool isMissing = (pvNullable == null) ? true : false;
                float th_hi = (float)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_TH_HI").GetValue(obsData);
                float th_lo = (float)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_TH_LO").GetValue(obsData);
                bool isUsing = ((string)typeof(ObservatoryData).GetField($"SENSOR_{i:000}_USE_YN").GetValue(obsData)).Contains('Y')? true : false;

                SensorInfo info = new SensorInfo(name, unit, pv, alarmType, isMissing, th_hi, th_lo, isUsing);
                tSensors.Add((i, info));
            }
            sensors.AddRange(tSensors);
            //tSensors.ForEach(sensor => sensors.Add(sensor));

            CctvInfo cctvInfo1 = new CctvInfo(
                obsData.CCTV01_ENDPOINT,
                obsData.CCTV01_LOCATION_TEXT,
                obsData.CCTV01_ENDPOINT != "0.0.0.0:0" ? true : false,
                new Dictionary<string, bool>
                {
                    { "ZOOM_IN", obsData.CCTV01_FUNC_BITMASK[0] == '1'? true : false },
                    { "ZOOM_OUT", obsData.CCTV01_FUNC_BITMASK[1] == '1' ? true : false },
                    { "LOOK_LEFT", obsData.CCTV01_FUNC_BITMASK[2] == '1' ? true : false },
                    { "LOOK_RIGHT", obsData.CCTV01_FUNC_BITMASK[3] == '1' ? true : false },
                    { "LOOK_UP", obsData.CCTV01_FUNC_BITMASK[4] == '1' ? true : false },
                    { "LOOK_DOWN", obsData.CCTV01_FUNC_BITMASK[5] == '1' ? true : false },
                }
            );
            cctvs.Add(cctvInfo1);

            CctvInfo cctvInfo2 = new CctvInfo(
                obsData.CCTV02_ENDPOINT,
                obsData.CCTV02_LOCATION_TEXT,
                obsData.CCTV02_ENDPOINT != "0.0.0.0:0" ? true : false,
                new Dictionary<string, bool>
                {
                    { "ZOOM_IN", obsData.CCTV02_FUNC_BITMASK[0] == '1'? true : false },
                    { "ZOOM_OUT", obsData.CCTV02_FUNC_BITMASK[1] == '1' ? true : false },
                    { "LOOK_LEFT", obsData.CCTV02_FUNC_BITMASK[2] == '1' ? true : false },
                    { "LOOK_RIGHT", obsData.CCTV02_FUNC_BITMASK[3] == '1' ? true : false },
                    { "LOOK_UP", obsData.CCTV02_FUNC_BITMASK[4] == '1' ? true : false },
                    { "LOOK_DOWN", obsData.CCTV02_FUNC_BITMASK[5] == '1' ? true : false },
                }
            );
            cctvs.Add(cctvInfo2);

            BoardInfo toxinBoard = new BoardInfo(
                obsData.BRD_TOXIN_MODEL_CODE,
                
                obsData.BRD_TOXIN_LIFE_STATE,
                obsData.BRD_TOXIN_OP_STATE,
                obsData.BRD_TOXIN_COM_STATE,
                
                obsData.BRD_TOXIN_USE_YN == "Y" ? true : false,
                obsData.BRD_TOXIN_INSPECT_YN == "Y" ? true : false,

                obsData.BRD_TOXIN_MEAS_TEMP.HasValue? obsData.BRD_TOXIN_MEAS_TEMP.Value : 0f,
                obsData.BRD_TOXIN_MEAS_TEMP.HasValue,
                obsData.BRD_TOXIN_MEAS_PH.HasValue ? obsData.BRD_TOXIN_MEAS_PH.Value : 0f,
                obsData.BRD_TOXIN_MEAS_PH.HasValue,
                obsData.BRD_TOXIN_MEAS_EC.HasValue ? obsData.BRD_TOXIN_MEAS_EC.Value : 0f,
                obsData.BRD_TOXIN_MEAS_EC.HasValue
            );
            boards.Add((BoardSpecInfo.BoardType.TOXIN, toxinBoard));
            
            BoardInfo hnsBoard = new BoardInfo(
                obsData.BRD_HNS_MODEL_CODE,

                obsData.BRD_HNS_LIFE_STATE,
                obsData.BRD_HNS_OP_STATE,
                obsData.BRD_HNS_COM_STATE,

                obsData.BRD_HNS_USE_YN == "Y" ? true : false,
                obsData.BRD_HNS_INSPECT_YN == "Y" ? true : false,

                obsData.BRD_HNS_MEAS_TEMP.HasValue ? obsData.BRD_HNS_MEAS_TEMP.Value : 0f,
                obsData.BRD_HNS_MEAS_TEMP.HasValue,
                obsData.BRD_HNS_MEAS_PH.HasValue ? obsData.BRD_HNS_MEAS_PH.Value : 0f,
                obsData.BRD_HNS_MEAS_PH.HasValue,
                obsData.BRD_HNS_MEAS_EC.HasValue ? obsData.BRD_HNS_MEAS_EC.Value : 0f,
                obsData.BRD_HNS_MEAS_EC.HasValue
            );
            boards.Add((BoardSpecInfo.BoardType.HNS, hnsBoard));
            
            BoardInfo wqBoard = new BoardInfo(
                obsData.BRD_WQ_MODEL_CODE,

                obsData.BRD_WQ_LIFE_STATE,
                obsData.BRD_WQ_OP_STATE,
                obsData.BRD_WQ_COM_STATE,

                obsData.BRD_WQ_USE_YN == "Y" ? true : false,
                obsData.BRD_WQ_INSPECT_YN == "Y" ? true : false,

                obsData.BRD_WQ_MEAS_TEMP.HasValue ? obsData.BRD_WQ_MEAS_TEMP.Value : 0f,
                obsData.BRD_WQ_MEAS_TEMP.HasValue,
                obsData.BRD_WQ_MEAS_PH.HasValue ? obsData.BRD_WQ_MEAS_PH.Value : 0f,
                obsData.BRD_WQ_MEAS_PH.HasValue,
                obsData.BRD_WQ_MEAS_EC.HasValue ? obsData.BRD_WQ_MEAS_EC.Value : 0f,
                obsData.BRD_WQ_MEAS_EC.HasValue
            );
            boards.Add((BoardSpecInfo.BoardType.WQ, wqBoard));
        }


        public int obsIdx;
        public string nameText;
        public int? groupIdx;
        public Vector2 coordinate;
        public string addrText;

        public List<(int idx, SensorInfo info)> sensors = new();
        public List<(BoardSpecInfo.BoardType type, BoardInfo info)> boards = new();
        public List<CctvInfo> cctvs = new();

        public class SensorInfo 
        {
            public SensorInfo(string name, string unit, float pv, int alarmType, bool isMissing, float thresholdHigh, float thresholdLow, bool isUsing)
            {
                this.name = name;
                this.unit = unit;
                this.pv = pv;
                this.alarmType = alarmType;
                this.isMissing = isMissing;
                this.thresholdHigh = thresholdHigh;
                this.thresholdLow = thresholdLow;
                this.isUsing = isUsing;
            }
            public string name;
            public string unit;
            public float pv;
            public bool isMissing;
            public int alarmType;
            public float thresholdHigh;
            public float thresholdLow;
            public bool isUsing;
        }

        public class BoardInfo
        {
            public BoardInfo(string modelCode,
                string? stateLife, string? stateOp, string? stateCom,
                bool isUsing, bool isInspecting,
                float measuredTemp, bool isMissingTemp, float measuredPh, bool isMissingPh, float measuredEc, bool isMissingEc)
            {
                this.modelCode = modelCode;

                this.stateLife = stateLife;
                this.stateOp = stateOp;
                this.stateCom = stateCom;

                this.isUsing = isUsing;
                this.isInspecting = isInspecting;

                this.measuredTemp = measuredTemp;
                this.isMissingTemp = isMissingTemp;
                this.measuredPh = measuredPh;
                this.isMissingPh = isMissingPh;
                this.measuredEc = measuredEc;
                this.isMissingEc = isMissingEc;
            }

            public string modelCode;

            public string? stateLife;//?
            public string? stateOp;//?
            public string? stateCom;//?

            public bool isUsing;
            public bool isInspecting;

            public float measuredTemp;
            public bool isMissingTemp;
            public float measuredPh;
            public bool isMissingPh;
            public float measuredEc;
            public bool isMissingEc;
        }

        public class CctvInfo
        {
            public CctvInfo(string endpoint, string locationText, bool isValid, Dictionary<string, bool> funcMap)
            {
                this.endpoint = endpoint;
                this.locationText = locationText;
                this.isValid = isValid;
                this.functionMap = funcMap;
            }

            public string? endpoint;
            public string? locationText;
            public bool isValid;
            public Dictionary<string, bool>? functionMap;
        }


    }
}
