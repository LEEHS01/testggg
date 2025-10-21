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
        public ToxinStatus status { get; set; } = ToxinStatus.Green;
        public List<DateTime> dateTimes; // 각 값의 실제 측정 시간
        public string unit;  // 단위 추가
        public string stcd = "00";  // STCD 상태코드 추가


        public ToxinData(HnsResourceModel model, string unit = "")
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
            this.unit = unit;  //단위 설정
            this.stcd = "00"; // 기본값: 정상
        }

        public void UpdateValue(CurrentDataModel model)
        {

            if (model != null)
            {
                this.serious = model.hi;
                this.warning = model.hihi;
                this.on = Convert.ToInt32(model.useyn) == 1;
                this.fix = Convert.ToInt32(model.fix) == 1;
                this.stcd = model.stcd ?? "00";

                DateTime now = DateTime.Now;
                DateTime roundedTime = new DateTime(
                    now.Year, now.Month, now.Day,
                    now.Hour, (now.Minute / 10) * 10, 0); // 10분 단위로 내림

                this.SetLastValue(model.val, roundedTime);

                // 상태도 여기서 계산 (옵션)
                if (!this.fix && model.val.HasValue)
                {
                    if (model.val >= model.hihi)
                        this.status = ToxinStatus.Red;
                    else if (model.val > model.hi)
                        this.status = ToxinStatus.Yellow;
                    else
                        this.status = ToxinStatus.Green;
                }
            }
        }

        private void SetLastValue(float? val, DateTime time)
        {
            int countExpected = Mathf.FloorToInt((Option.TREND_DURATION_REALTIME * 60f) / Option.TREND_TIME_INTERVAL);

            // 중복 시간 체크 - 같은 시간이면 덮어쓰기
            if (this.dateTimes.Count > 0 && this.dateTimes.Last() == time)
            {
                // 같은 시간의 데이터면 마지막 값만 갱신
                if (val.HasValue)
                {
                    this.values[this.values.Count - 1] = (float)val;
                }
                return; // 중복이므로 추가하지 않음
            }

            // 슬라이딩 윈도우
            if (this.values.Count >= countExpected)
            {
                this.values.RemoveAt(0);
                if (this.dateTimes.Count > 0)
                    this.dateTimes.RemoveAt(0);
            }

            // 새 값 추가
            if (val == null)
            {
                int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
                this.values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
            }
            else
            {
                this.values.Add((float)val);
            }

            this.dateTimes.Add(time);
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


