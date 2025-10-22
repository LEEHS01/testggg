using Assets.Scripts.Data;
using Onthesys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;


internal class PopupDetailToxin : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    LogData log;
    private ToxinData data;

    public TMP_Text txtName;
    public TMP_Text txtCurrent;
    public TMP_Text txtTotal;
    public List<ChartBar> bars;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectAlarm);
        UiManager.Instance.Register(UiEventType.Popup_AiTrend, OnSelectChart);
        gameObject.SetActive(false);
    }

    private void OnSelectAlarm(object obj)
    {
        if (obj is not int logIdx) return;

        LogData log = modelProvider.GetAlarm(logIdx);

        this.log = log;

    }


    public void OnSelectChart(object data)
    {
        gameObject.SetActive(true);

        if (data is not ValueTuple<int, int> tuple) return;

        int boardId = tuple.Item1;
        int hnsId = tuple.Item2;

        ToxinData toxin = modelProvider.GetToxin(boardId, hnsId);
        // modelProvider.GetToxinsInLog().Find(toxin => toxin.boardid == boardId && toxin.hnsid == hnsId);

        if (toxin == null)
        {
            return;
        }

        this.data = toxin;
        this.txtName.text = toxin.hnsName.Replace("\n", string.Empty);
        //현재값, 임계값 삭제
        //this.txtCurrent.text = Math.Round(toxin.GetLastValue(), 2).ToString();
        //this.txtTotal.text = Math.Round(toxin.warning, 2).ToString();

        // 그래프 바가 제대로 활성화되지 않으면 활성화
        bars.ForEach(bar => bar.gameObject.SetActive(true));

        UpdateChart();

    }

    /// <summary>
    /// 이상값인지 확인하는 함수
    /// </summary>
    /// <param name="value">확인할 값</param>
    /// <returns>이상값이면 true</returns>
    private bool IsAnomalousValue(float value)
    {
        return value == -9999f || value == 9999f || Math.Abs(value) > 9998f;
    }

    /// <summary>
    /// 이상값을 직전값으로 대체하는 함수
    /// </summary>
    /// <param name="originalValues">원본 데이터</param>
    /// <returns>이상값이 대체된 데이터</returns>
    private List<float> ProcessAnomalousValues(List<float> originalValues)
    {
        List<float> processedValues = new List<float>();

        for (int i = 0; i < originalValues.Count; i++)
        {
            if (IsAnomalousValue(originalValues[i]))
            {
                // 직전값 사용 (첫 번째 값이 이상값이면 0 사용)
                float replacementValue = 0f;
                processedValues.Add(replacementValue);

                Debug.Log($"이상값 감지 및 대체: index={i}, original={originalValues[i]}, replacement={replacementValue}");
            }
            else
            {
                processedValues.Add(originalValues[i]);
            }
        }

        return processedValues;
    }

    /// <summary>
    /// 이상값의 인덱스들을 반환하는 함수
    /// </summary>
    /// <param name="originalValues">원본 데이터</param>
    /// <returns>이상값 인덱스 리스트</returns>
    private List<int> GetAnomalousIndices(List<float> originalValues)
    {
        List<int> anomalousIndices = new List<int>();

        for (int i = 0; i < originalValues.Count; i++)
        {
            if (IsAnomalousValue(originalValues[i]))
            {
                anomalousIndices.Add(i);
            }
        }

        return anomalousIndices;
    }
    // PopupDetailToxin.cs의 UpdateChart() 메서드 수정
    // AI값과 측정값이 동일한 y축 스케일을 사용하도록 변경

    private void UpdateChart()
    {
        DateTime endTime = modelProvider.GetCurrentChartEndTime();
        Debug.LogWarning("차트 업데이트 시작");

        /*if (this.data.aiValues != null)
        {
            Debug.Log($"========== AI값 전체 ({this.data.aiValues.Count}개) ==========");
            Debug.Log(string.Join(", ", this.data.aiValues));
        }
        else
        {
            Debug.LogError("AI값이 null입니다!");
        }*/

        // 그래프 데이터 추출기 정의 (aiValues, values, diffValues)
        List<Func<ToxinData, List<float>>> valuesExtractors = new()
    {
        toxinData => toxinData.chartAiValues,
        toxinData => toxinData.chartValues,
        toxinData => toxinData.chartDiffValues,
    };
        string[] dataNames = { "AI값", "측정값", "편차값" };

        if (bars.Count != valuesExtractors.Count)
        {
            Debug.LogWarning($"bars.Count({bars.Count})와 valuesExtractors.Count({valuesExtractors.Count})가 일치하지 않음");
            return;
        }

        // 1단계: 모든 데이터 전처리하여 공통 max 값 계산
        List<List<float>> allProcessedValues = new List<List<float>>();
        List<List<int>> allAnomalousIndices = new List<List<int>>();

        for (int i = 0; i < valuesExtractors.Count; i++)
        {
            List<float> originalValues = valuesExtractors[i](this.data);
            List<int> anomalousIndices = GetAnomalousIndices(originalValues);
            List<float> processedValues = ProcessAnomalousValues(originalValues);

            allProcessedValues.Add(processedValues);
            allAnomalousIndices.Add(anomalousIndices);
        }

        // 2단계: 공통 max 값 계산
        float commonMaxForAiAndMeasured = 0f;

        // 측정값의 최댓값을 AI값에도 동일하게 적용
        if (allProcessedValues.Count >= 2)
        {
            float measuredMax = allProcessedValues[1].Max(); // 측정값의 최댓값

            // ⭐ 이 부분만 추가하면 됨!
            if (measuredMax <= 0)
            {
                measuredMax = 100f; // 독성도 기본값
            }

            commonMaxForAiAndMeasured = measuredMax;
        }

        if (commonMaxForAiAndMeasured <= 0)
        {
            commonMaxForAiAndMeasured = 2f;
        }

        // 3단계: 각 차트별로 적절한 max 값 적용
        for (int i = 0; i < Mathf.Min(bars.Count, valuesExtractors.Count); i++)
        {
            List<float> processedValues = allProcessedValues[i];
            List<int> anomalousIndices = allAnomalousIndices[i];

            float max;

            if (i == 0 || i == 1) // AI값(0), 측정값(1) - 동일한 스케일 사용
            {
                max = commonMaxForAiAndMeasured;
            }
            else // 편차값(2) - 기존 로직 유지
            {
                bool allZero = processedValues.All(v => v == 0f);
                if (allZero)
                {
                    max = 2f; // 0, 1, 2, 3
                }
                else
                {
                    max = Mathf.Max(this.data.warning, processedValues.Max());
                }
            }

            var chartValues = processedValues.Select(value => value / max).ToList();

            bars[i].line.UpdateControlPoints(chartValues);
            bars[i].CreatAxis(endTime, max);
            bars[i].SetNormalizedValues(chartValues);

            // 차트 포인트 찾기 - 대기 없이 바로 실행
            EnsureChartPointsReady(bars[i]);

            // 이상값 위치를 빨간색으로 표시
            bars[i].HighlightAnomalousPoints(anomalousIndices);
            Debug.LogWarning($"그래프 {i} ({dataNames[i]}) 완료: max={max}, {anomalousIndices.Count}개 빨간점");
        }

        Debug.Log("모든 차트 업데이트 완료");
    }


    private void EnsureChartPointsReady(ChartBar bar)
    {
        int attempts = 0;
        int maxAttempts = 5;

        while (attempts < maxAttempts)
        {
            bar.FindAllChartPoints();

            if (bar.dots != null && bar.dots.Count > 0)
            {
                Debug.Log($"차트 포인트 준비 완료: {bar.dots.Count}개");
                break;
            }

            Debug.Log($"차트 포인트 준비 중... 시도 {attempts + 1}/{maxAttempts}");
            attempts++;
            // yield return new WaitForSeconds(0.05f); 제거
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("차트 포인트 준비 실패 - 최대 시도 횟수 초과");
        }
    }
}

