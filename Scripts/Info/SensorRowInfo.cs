using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info
{
    public  class SensorRowInfo
    {
        public SensorRowInfo(HnsDataMeasData row)
        {
            table = new List<(int idx, float val, bool isMissing)>();
          
            //Debug.Log("" + row.MEAS_DT + "");
            timestamp = DateTime.ParseExact(
                row.MEAS_DT,
                "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture
            );

            for (int i = 1; i <= 59; i++)
            {
                var property = typeof(HnsDataMeasData).GetField($"SENSOR_{i:000}");
                if (property != null)
                {
                    float? val = (float?)property.GetValue(row);
                    bool isMissing = val.HasValue ? false : true;


                    table.Add((i, isMissing ? -999f: val.Value, isMissing));
                    //Debug.Log("" + row.MEAS_DT + "(" + i + ") : " + (isMissing ? -999f : val.Value));
                }
            }
            

        }
        
        public DateTime timestamp;

        public new List<(int idx, float val, bool isMissing)> table;

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
