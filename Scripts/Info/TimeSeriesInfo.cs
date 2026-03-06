using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info
{
    public  class TimeSeriesInfo
    {
        public TimeSeriesInfo(List<HnsDataMeasData> data)
        {
            table = new List<(DateTime timestamp, List<(int idx, float val, bool isMissing)> valSet)>();
            foreach (var row in data)
            {
                //Debug.Log("" + row.MEAS_DT + "");
                DateTime timestamp = DateTime.ParseExact(
                    row.MEAS_DT,
                    "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture
                );

                List<(int idx, float val, bool isMissing)> valSet = new List<(int idx, float val, bool isMissing)>();
                for (int i = 1; i <= 59; i++)
                {
                    var property = typeof(HnsDataMeasData).GetField($"SENSOR_{i:000}");
                    if (property != null)
                    {
                        float? val = (float?)property.GetValue(row);
                        bool isMissing = val.HasValue ? false : true;


                        valSet.Add((i, isMissing ? -999f: val.Value, isMissing));
                        //Debug.Log("" + row.MEAS_DT + "(" + i + ") : " + (isMissing ? -999f : val.Value));
                    }
                }
                table.Add((timestamp, valSet));
            }

        }
        public TimeSeriesInfo(List<HnsDataPredData> data)
        {
            table = new List<(DateTime timestamp, List<(int idx, float val, bool isMissing)> valSet)>();
            foreach (var row in data)
            {
                //Debug.Log("" + row.MEAS_DT + "");
                DateTime timestamp = DateTime.ParseExact(
                    row.PRED_DT,
                    "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture
                );

                List<(int idx, float val, bool isMissing)> valSet = new List<(int idx, float val, bool isMissing)>();
                for (int i = 1; i <= 59; i++)
                {
                    var property = typeof(HnsDataPredData).GetField($"SENSOR_{i:000}");
                    if (property != null)
                    {
                        float? val = (float?)property.GetValue(row);
                        bool isMissing = val.HasValue ? false : true;


                        valSet.Add((i, isMissing ? -999f : val.Value, isMissing));
                        //Debug.Log("" + row.MEAS_DT + "(" + i + ") : " + (isMissing ? -999f : val.Value));
                    }
                }
                table.Add((timestamp, valSet));
            }

        }



        public DateTime StartDt => table.Min(row => row.timestamp);
        public DateTime EndDt => table.Max(row => row.timestamp);
        public TimeSpan Interval => (EndDt - StartDt)/table.Count;

        public List<(DateTime timestamp, List<(int idx, float val, bool isMissing)> valSet)> table;

        public bool isValidData() 
        {
            // 데이터 가공 예문
            //List<float> floats = 
            //    table.OrderBy(row => row.timestamp) // 시간순 정렬
            //        .Select(row => 
            //            row.valSet.Where(set => set.idx == 2).First().val // idx가 2인 값 선택
            //        ).ToList();

            //@TODO
            return true;
        }
    }
}
