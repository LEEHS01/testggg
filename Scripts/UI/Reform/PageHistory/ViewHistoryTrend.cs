using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageHistory
{
    internal class ViewHistoryTrend : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;


        UILineRenderer2 trendMeas, trendPred;
        Button btnQuery;

        (TMP_InputField from, TMP_InputField to) txbDatetime;
        DateTime dtFrom, dtTo;
        TMP_Dropdown ddlRegion, ddlObservatory, ddlSensor;



        List<GroupInfo> groups;
        List<ObservatoryInfo> obss;
        List<(int idx, ObservatoryInfo.SensorInfo info)> sensors;

        int groupIdx = -1, obsIdx = -1, sensorIdx = -1;


        //Func
        public DateTime DatetimeFrom
        {
            get => dtFrom;
            set
            {
                dtFrom = value;
                txbDatetime.from.SetTextWithoutNotify(dtFrom.ToString("yyyy-MM-dd"));
                dtFrom = dtFrom.AddHours(-dtFrom.Hour);
                dtFrom = dtFrom.AddMinutes(-dtFrom.Minute);
                dtFrom = dtFrom.AddSeconds(-dtFrom.Second);

            }
        }
        public DateTime DateTimeTo
        {
            get => dtTo;
            set
            {
                dtTo = value;
                txbDatetime.to.SetTextWithoutNotify(dtTo.ToString("yyyy-MM-dd"));

                if (dtTo.Date != DateTime.Now.Date)
                {
                    dtTo = dtTo.AddHours(23 - dtTo.Hour);
                    dtTo = dtTo.AddMinutes(59 - dtTo.Minute);
                    dtTo = dtTo.AddSeconds(59 - dtTo.Second);
                }
                else
                {
                    dtTo = DateTime.Now;
                }
            }
        }


        private void Start()
        {
            trendMeas = transform.Find("PanelTrendCharts").Find("TrendChartPv").Find("Chart_Dots").GetComponent<UILineRenderer2>();
            trendPred = transform.Find("PanelTrendCharts").Find("TrendChartAi").Find("Chart_Dots").GetComponent<UILineRenderer2>();
            btnQuery = transform.Find("PanelControl").Find("panelTimeRange").Find("btnQuery").GetComponent<Button>();
            btnQuery.onClick.AddListener(OnClickQuery);

            txbDatetime.from = transform.Find("PanelControl").Find("panelTimeRange").Find("tbxDtFrom").GetComponent<TMP_InputField>();
            txbDatetime.to = transform.Find("PanelControl").Find("panelTimeRange").Find("tbxDtTo").GetComponent<TMP_InputField>();

            ddlRegion = transform.Find("PanelControl").Find("ddlRegion").GetComponent<TMP_Dropdown>();
            ddlObservatory = transform.Find("PanelControl").Find("ddlObservatory").GetComponent<TMP_Dropdown>();
            ddlSensor = transform.Find("PanelControl").Find("ddlSensor").GetComponent<TMP_Dropdown>();

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.UpdateHistoryTimeSeries,OnUpdateHistoryTimeSeries);

            ddlRegion.onValueChanged.AddListener(OnChangeRegion);
            ddlObservatory.onValueChanged.AddListener(OnChangeObservatory);
            ddlSensor.onValueChanged.AddListener(OnChangeSensor);




            txbDatetime.from.onValueChanged.AddListener(value => OnChangeDateTime(true, value));
            txbDatetime.from.onEndEdit.AddListener(value => OnEndEditDateTime(true, value));
            txbDatetime.from.text = DateTime.Now.ToString("yyyy-MM-dd");

            txbDatetime.to.onValueChanged.AddListener(value => OnChangeDateTime(false, value));
            txbDatetime.to.onEndEdit.AddListener(value => OnEndEditDateTime(false, value));
            txbDatetime.to.text = DateTime.Now.ToString("yyyy-MM-dd");

        }

        private void OnUpdateHistoryTimeSeries(object obj)
        {
            List<(DateTime timestamp, (float val, bool isMissing) value)> timeSeries = modelProvider.GetHistoryTimeSeriesInfo();


            List<float> trendDots = new();

            float maxVal = timeSeries.Max(row => row.value.val);

            int i = 24, maxC = 24; ;
            
            if (timeSeries.Count < maxC)
                throw new Exception($"시계열 데이터 개수 부족 - {timeSeries.Count}개 (최소 {maxC}개 필요)");


            while (i --> 0)
            {
                float ratio = (float)(i) / maxC;
                int idx = Mathf.RoundToInt(timeSeries.Count * ratio);
                float val = timeSeries[idx].value.val;
                trendDots.Add(val / ratio);
            }

            trendMeas.UpdateControlPoints(trendDots);
        }

        private void OnChangeDateTime(bool isFrom, string value)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //미래 기록 조회 불가
                if (dt.Date > DateTime.Now.Date)
                    dt = DateTime.Now;

                //From < To가 참이게끔 교정
                if (!isFrom && dt < dtFrom)
                    DatetimeFrom = dt;

                if (isFrom && dt > dtTo)
                    DateTimeTo = dt;

                //저장
                if (isFrom) DatetimeFrom = dt; else DateTimeTo = dt;
            }
            catch { }
        }
        private void OnEndEditDateTime(bool isFrom, string value)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //미래 기록 조회 불가
                if (dt.Date > DateTime.Now.Date)
                    dt = DateTime.Now;

                //From < To가 참이게끔 교정
                if (!isFrom && dt < dtFrom)
                    DatetimeFrom = dt;

                if (isFrom && dt > dtTo)
                    DateTimeTo = dt;

                //저장
                if (isFrom) DatetimeFrom = dt; else DateTimeTo = dt;
            }
            finally
            {
                txbDatetime.from.SetTextWithoutNotify(dtFrom.ToString("yyyy-MM-dd"));
                txbDatetime.to.SetTextWithoutNotify(dtTo.ToString("yyyy-MM-dd"));
            }
        }

        private void OnClickQuery()
        {
            Debug.Log($"group {groupIdx} / obsIdx {obsIdx} / sensorIdx {sensorIdx}");
            UiManager.Instance.Invoke(UiEventType.RequestHistoryTimeSeries, (groupIdx, obsIdx, sensorIdx, dtFrom, dtTo));
        }

        private void OnChangeSensor(int arg0)
        {

            string tOption = ddlSensor.options[arg0].text;
            int idx = int.Parse(tOption.Split(':')[0]);
            var tList = sensors.FindAll(g => g.idx == idx);
            
            if (tList.Count ==0) 
            {
                Debug.LogError($"센서 정보 없음 - {idx}");
                return;
            }

            (int idx, ObservatoryInfo.SensorInfo info) info = tList.First();
            sensorIdx = info.idx;
        }

        private void OnChangeObservatory(int arg0)
        {
            string tOption = ddlObservatory.options[arg0].text;
            ObservatoryInfo info = obss.FirstOrDefault(g => g.nameText == tOption);

            if (info == null)
            {
                Debug.LogError($"관측소 정보 없음 - {info}");
                return;
            }

            obsIdx = info.obsIdx;
            sensors = info.sensors;

            ddlSensor.options = info.sensors.Where(sen => sen.info.isUsing).Select(sen => new TMP_Dropdown.OptionData($"{sen.idx}:{sen.info.name}")).ToList();

            ddlSensor.enabled = true;
            if (ddlSensor.options.Count == 0)
            {
                ddlSensor.enabled = false;
                ddlSensor.options.Add(new TMP_Dropdown.OptionData("(비어있음)"));
            }
            else 
                ddlSensor.value = 0;
        }

        private void OnChangeRegion(int arg0)
        {
            string tOption = ddlRegion.options[arg0].text;
            GroupInfo info = groups.FirstOrDefault(g => g.groupName == tOption);

            if(info == null) { 
                Debug.LogError($"지역 정보 없음 - {tOption}");
                return;
            }

            groupIdx = info.groupIdx;

            ddlObservatory.options = obss.Where(obs => obs.groupIdx == info.groupIdx).Select(obs => new TMP_Dropdown.OptionData(obs.nameText)).ToList();

            ddlObservatory.enabled = true;
            if (ddlObservatory.options.Count == 0)
            {
                ddlSensor.enabled = false;
                ddlObservatory.enabled = false;
                ddlObservatory.options.Add(new TMP_Dropdown.OptionData("(비어있음)"));
            }
            else
                ddlObservatory.value = 0;
        }

        private void OnInitiate(object obj)
        {
            groups = modelProvider.GetGroups();
            obss = modelProvider.GetObss();
    
            //지역 드롭다운 초기화
            ddlRegion.options = groups.Select(group => new TMP_Dropdown.OptionData(group.groupName)).ToList();
            ddlRegion.value = 0;




        }
    }
}
