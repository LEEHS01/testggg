using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace Onthesys
{
    public class LogData
    {
        public int obsId;
        public int boardId;
        public DateTime time;
        public string areaName;
        public string obsName;
        public int hnsId;
        public string hnsName;
        public int status;
        public float? value;
        public int idx;
        public float serious, warning;
        public bool isCancelled;

        public LogData(int obsid, int boardid, string areaName, string obsName, int hnsId, string hnsName, DateTime dt, int status, float? val, int idx, float serious, float warning, bool isCancelled = false)
        {
            this.obsId = obsid;
            this.boardId = boardid;
            this.areaName = areaName;
            this.obsName = obsName;
            this.hnsId = hnsId;
            this.hnsName = hnsName;
            this.time = dt;
            this.status = status;
            this.value = val;
            this.idx = idx;
            this.serious = serious;
            this.warning = warning;
            this.isCancelled = isCancelled;
        }

        internal static LogData FromAlarmLogModel(AlarmLogModel item) => 
            new LogData(
                item.obsidx, 
                item.boardidx, 
                item.areanm, 
                item.obsnm, 
                item.hnsidx, 
                item.hnsnm, 
                Convert.ToDateTime(item.aladt), 
                item.alacode,
                item.currval.HasValue ? item.currval.Value : 0f,
                item.alaidx,
                item.alahival.HasValue ? item.alahival.Value : 0f,
                item.alahihival.HasValue ? item.alahihival.Value : 0f,
                !string.IsNullOrEmpty(item.turnoff_flag)
             );
    }

}


