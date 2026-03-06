using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{
    internal class SensorData
    {
        public DateTime Timestamp { get; set; }
        public ToxinData ToxinInfo { get; set; }

        public SensorData(DateTime timestamp, ToxinData toxinInfo)
        {
            Timestamp = timestamp;
            ToxinInfo = toxinInfo;
        }
    }
}
