using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


internal class ChartBar : MonoBehaviour
{
    public UILineRenderer2 line;
    public List<TMP_Text> hours;
    public List<TMP_Text> verticals;


    [Header("차트 포인트들")]
    public Transform pointsParent; // Point들의 부모 (Chart_Dots 등)

    public List<RectTransform> dots = new List<RectTransform>();
    public List<UnityEngine.UI.Image> dotImages = new List<UnityEngine.UI.Image>();



    public void CreatAxis(DateTime dt, float max)
    {
        SetMins(dt);
        SetVertical(max);
    }
    void SetMins(DateTime dt)
    {
        DateTime endDt = dt;
        DateTime startDt = endDt.AddHours(-12);
        var interval = (endDt - startDt).TotalMinutes / (this.hours.Count - 1);

        for (int i = 0; i < this.hours.Count; i++)
        {
            var t = startDt.AddMinutes(interval * i);
            this.hours[i].text = t.ToString("MM-dd\nHH:mm");
        }
    }
    /*void SetMins(DateTime dt)
    {
        DateTime startDt = dt.AddHours(-12);
        var turm = (dt - startDt).TotalMinutes / this.hours.Count;
        Debug.LogError("hours.Count = " + this.hours.Count);
        for (int i = 0; i < this.hours.Count; i++)
        {
            var t = dt.AddMinutes(-(turm * i));
            this.hours[i].text = t.ToString("MM-dd HH:mm");
        }
    }
*/
    void SetVertical(float max)
    {
        var verticalMax = ((max + 1) / (verticals.Count - 1));

        for (int i = 0; i < this.verticals.Count; i++)
        {
            this.verticals[i].text = Math.Round((verticalMax * i), 2).ToString();
        }
    }
    /// <summary>
    /// 모든 차트 포인트들을 찾아서 리스트에 저장 (TrendLineMeasure 방식 참고)
    /// </summary>
    public void FindAllChartPoints()
    {
        dots.Clear();
        dotImages.Clear();

        if (pointsParent == null)
        {
            // pointsParent가 없으면 자동으로 찾기
            pointsParent = transform.Find("Chart_Dots") ?? transform.Find("Points") ?? transform.Find("Dots");
        }

        if (pointsParent != null)
        {
            // TrendLineMeasure와 동일한 방식으로 RectTransform 수집
            dots = pointsParent.GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(pointsParent.GetComponent<RectTransform>()); // 부모 자신은 제거

            // 각 점의 Image 컴포넌트 수집
            foreach (var dot in dots)
            {
                UnityEngine.UI.Image dotImage = dot.GetComponent<UnityEngine.UI.Image>();
                dotImages.Add(dotImage);
            }

            Debug.Log($"찾은 차트 포인트 개수: {dots.Count}");
        }
        else
        {
            Debug.LogWarning("차트 포인트들의 부모를 찾을 수 없습니다. pointsParent를 수동으로 할당해주세요.");
        }
    }

    /// <summary>
    /// 이상값 위치의 포인트들을 빨간색으로 변경
    /// </summary>
    /// <param name="anomalousIndices">이상값 인덱스 리스트</param>
    public void HighlightAnomalousPoints(List<int> anomalousIndices)
    {
        // 차트 포인트들이 없으면 다시 찾기
        if (dots.Count == 0)
        {
            FindAllChartPoints();
        }

        // 모든 포인트를 원래 색깔로 초기화
        ResetAllPointColors();

        if (anomalousIndices == null || anomalousIndices.Count == 0)
            return;

        // 이상값 인덱스에 해당하는 포인트들을 빨간색으로 변경
        foreach (int index in anomalousIndices)
        {
            if (index >= 0 && index < dotImages.Count && dotImages[index] != null)
            {
                dotImages[index].color = Color.red;
                Debug.Log($"포인트 {index}를 빨간색으로 변경");
            }
        }
    }

    /// <summary>
    /// 모든 포인트 색깔을 기본색으로 초기화
    /// </summary>
    private void ResetAllPointColors()
    {
        foreach (var dotImage in dotImages)
        {
            if (dotImage != null)
            {
                // 기본 색깔로 복원 (청록색)
                dotImage.color = new Color(0, 1, 1); // TrendLineMeasure의 기본 색상과 동일
            }
        }
    }

    /// <summary>
    /// 정규화된 값들을 저장 (기존 코드와의 호환성을 위해 유지)
    /// </summary>
    /// <param name="normalizedValues">0-1 사이 정규화된 값들</param>
    public void SetNormalizedValues(List<float> normalizedValues)
    {
        // TrendLineMeasure 방식에서는 필요없지만 기존 코드 호환성을 위해 유지
        // 실제로는 UILineRenderer2.UpdateControlPoints()가 점들의 위치를 설정함
    }

    /*/// <summary>
    /// 모든 차트 포인트들을 찾아서 리스트에 저장 (TrendLineMeasure 방식 참고)
    /// </summary>
    private void FindAllChartPoints()
    {
        dots.Clear();
        dotImages.Clear();

        if (pointsParent == null)
        {
            // pointsParent가 없으면 자동으로 찾기
            pointsParent = transform.Find("Chart_Dots") ?? transform.Find("Points") ?? transform.Find("Dots");
        }

        if (pointsParent != null)
        {
            // TrendLineMeasure와 동일한 방식으로 RectTransform 수집
            dots = pointsParent.GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(pointsParent.GetComponent<RectTransform>()); // 부모 자신은 제거

            // 각 점의 Image 컴포넌트 수집
            foreach (var dot in dots)
            {
                UnityEngine.UI.Image dotImage = dot.GetComponent<UnityEngine.UI.Image>();
                dotImages.Add(dotImage);
            }

            Debug.Log($"찾은 차트 포인트 개수: {dots.Count}");
        }
        else
        {
            Debug.LogWarning("차트 포인트들의 부모를 찾을 수 없습니다. pointsParent를 수동으로 할당해주세요.");
        }
    }

    /// <summary>
    /// 이상값 위치의 포인트들을 빨간색으로 변경
    /// </summary>
    /// <param name="anomalousIndices">이상값 인덱스 리스트</param>
    public void HighlightAnomalousPoints(List<int> anomalousIndices)
    {
        // 차트 포인트들이 없으면 다시 찾기
        if (dots.Count == 0)
        {
            FindAllChartPoints();
        }

        // 모든 포인트를 원래 색깔로 초기화
        ResetAllPointColors();

        if (anomalousIndices == null || anomalousIndices.Count == 0)
            return;

        // 이상값 인덱스에 해당하는 포인트들을 빨간색으로 변경
        foreach (int index in anomalousIndices)
        {
            if (index >= 0 && index < dotImages.Count && dotImages[index] != null)
            {
                dotImages[index].color = Color.red;
                Debug.Log($"포인트 {index}를 빨간색으로 변경");
            }
        }
    }

    /// <summary>
    /// 모든 포인트 색깔을 기본색으로 초기화
    /// </summary>
    private void ResetAllPointColors()
    {
        foreach (var dotImage in dotImages)
        {
            if (dotImage != null)
            {
                // 기본 색깔로 복원 (청록색)
                dotImage.color = new Color(0, 1, 1); // TrendLineMeasure의 기본 색상과 동일
            }
        }
    }

    /// <summary>
    /// 정규화된 값들을 저장 (기존 코드와의 호환성을 위해 유지)
    /// </summary>
    /// <param name="normalizedValues">0-1 사이 정규화된 값들</param>
    public void SetNormalizedValues(List<float> normalizedValues)
    {
        // TrendLineMeasure 방식에서는 필요없지만 기존 코드 호환성을 위해 유지
        // 실제로는 UILineRenderer2.UpdateControlPoints()가 점들의 위치를 설정함
    }*/
}

