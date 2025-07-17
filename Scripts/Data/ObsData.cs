using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
//using static UnityEditor.Progress;
//using static UnityEditor.Progress;
//using static UnityEngine.Rendering.DebugUI;

namespace Onthesys
{
    public class ObsData
    {
        public int id;
        public AreaData.AreaType type;
        public int areaId;
        public string areaName;
        public string obsName;
        public int step;

        public string src_video1 = "rtsp://admin:HNS_qhdks_!Q@W3@192.168.1.108:554/video1?profile=high";//"rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42/video1?profile=high";
        public string src_video2 = "rtsp://admin:HNS_qhdks_!Q@W3@192.168.1.108:554/video1?profile=high";//"C:\\Users\\onthesys\\Downloads\\happyCat.mp4";//"rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42/video1?profile=high";
        public string src_video_up = "";
        public string src_video_down = "";
        public string src_video_left = "";
        public string src_video_right = "";

        public static ObsData FromObsModel(ObservatoryModel model)
            => new ObsData(model.areanm, model.areaidx, model.obsnm, (AreaData.AreaType)model.areatype, model.obsidx, model.in_cctvUrl, model.out_cctvUrl);

        public ObsData(string areaName, int areaidx, string obsName, AreaData.AreaType type, int id, string src_video1, string src_video2)
        {
            this.areaName = areaName;
            this.areaId = areaidx;
            this.obsName = obsName;
            this.type = type;
            this.id = id;
            this.step = UnityEngine.Random.Range(0, 5);

            this.src_video1 = src_video1;
            this.src_video2 = src_video2;
        }

        private void UpdateStep(string step)
        {
            if (step != null)
            {
                switch (step.Trim())
                {
                    case "0020":
                        this.step = 1;
                        break;
                    case "0021":
                        this.step = 2;
                        break;
                    case "0023":
                        this.step = 3;
                        break;
                    case "0024":
                        this.step = 4;
                        break;
                    case "0025":
                        this.step = 5;
                        break;
                    default:
                        this.step = 5;
                        break;
                }
            }
        }
    }


    public enum ToolStatus
    {
        STAY_0,
        START_1,
        PRE_2,
        WORK_3,
        WASH_4
    }

    public enum CctvType 
    {
        OUTDOOR,
        EQUIPMENT,
    }
    

}


