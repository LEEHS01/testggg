using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

namespace Onthesys
{
    public class ToxinData
    {
        public int boardid;
        public int hnsid;
        public string hnsName;
        public float serious;
        public float warning;
        public float duration;
        public List<float> values;
        public List<float> aiValues;
        public List<float> diffValues;
        public bool on = true;
        public bool fix = false;
        public ToxinStatus status = ToxinStatus.Green;

        

        public ToxinData(HnsResourceModel model)
        {
            this.boardid = model.boardidx;
            this.hnsid = model.hnsidx;
            this.hnsName = model.hnsnm;
            this.serious = model.alahival == null ? 0 : (float)model.alahival;
            this.warning = model.alahihival == null ? 0 : (float)model.alahihival;
            this.duration = model.alahihisec == null ? 0 : (float)model.alahihisec;
            this.on = Convert.ToInt32(model.useyn) == 1;
            this.fix = Convert.ToInt32(model.inspectionflag) == 1;
            this.status = ToxinStatus.Green;
            this.values = new List<float>();
            this.aiValues = new List<float>();
            this.diffValues = new List<float>();
        }

        public void UpdateValue(CurrentDataModel model)
        {
            //Debug.Log("ToxinData.UpdateValue");
            if (model != null)
            {
                //Debug.LogError($" 실시간 값 확인: board={model.boardidx}, hns={model.hnsidx}, val={model.val}");
                /*if (hnsid == 4 && boardid == 3)
                    Debug.LogError($"UpdateValue bef{this.on} {model.useyn}");*/
                this.serious = model.hi;
                this.warning = model.hihi;
                this.on = Convert.ToInt32(model.useyn) == 1;
                this.fix = Convert.ToInt32(model.fix) == 1;
                this.SetLastValue(model.val);
                /*if (hnsid == 4 && boardid == 3)
                    Debug.LogError($"UpdateValue aft {this.on} {model.useyn}");*/
            }
        }

        private void SetLastValue(float? val)
        {
            //Chart가 24Point임 ---- 파악내용
            int countExpected = Mathf.FloorToInt((Option.TREND_DURATION_REALTIME * 60f) / Option.TREND_TIME_INTERVAL);
            //Debug.Log("countExpected : " + countExpected);
            if (this.values.Count >= countExpected)
            {
                this.values.RemoveAt(0);
            }

            //값이 없다면 무작위값을 추가.
            //실제로 값 들어오고 있음 ex. ToxinData.SetLastValue.val == 3.99
            //Debug.Log("ToxinData.SetLastValue.val == " + val.ToString());
            if (val == null)
            {
                int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
                this.values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
            }
            else
            {
                this.values.Add((float)val);
            }
        }
        public void CreateRandomValues()
        {
            int countExpected = Mathf.FloorToInt((Option.TREND_DURATION_REALTIME * 60f) / Option.TREND_TIME_INTERVAL);
            for (int i = 0; i < countExpected; i++)
            {
                int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
                values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
            }
        }

        public void CreateRandomValue(DateTime dt)
        {
            int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
            values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
        }

        public ToxinStatus GetStatus(string cd)
        {
            if (this.values.Count > 0)
            {
                var value = this.values.Last();
                if (value >= warning && !fix)
                {
                    return ToxinStatus.Red;
                }
                else if (serious > 0 && value >= serious && !fix)
                {
                    return ToxinStatus.Red;
                }
                else
                {
                    if (cd.Trim().Equals("0"))
                        return ToxinStatus.Green;
                    else
                        return fix ? ToxinStatus.Green : ToxinStatus.Yellow;
                }
            }
            return ToxinStatus.Green;
        }

        public float GetLastValue()
        {
            return values.Count > 0? values.Last() : 0f;
        }

        public float GetLastValuePercent()
        {
            return this.values.Last() / this.warning;
        }

        public ToxinStatus GetStatus()
        {
            if ((this.values.Last() >= Option.TOXIN_STATUS_RED) && fix == false) 
                return ToxinStatus.Red;
            else if (this.values.Last() >= Option.TOXIN_STATUS_YELLOW) 
                return ToxinStatus.Yellow;
            else 
                return ToxinStatus.Green;
        }

    }

    public enum ToxinStatus
    {
        Green,
        Yellow,
        Red,
        Purple//수정한 부분
    }
}


