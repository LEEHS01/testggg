using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Onthesys;
using System;
using UnityEngine.UI.TableUI;

/// <summary>
/// 센서 데이터를 표로 표시하는 팝업
/// </summary>
public class PopupTableData : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    [Header("UI References")]
    public GameObject panelTable;           // PanelTable 오브젝트
    public TableUI table;                   // Table 컴포넌트
    public Button btnClose;                 // 닫기 버튼
    public TMP_Text txtSensorName;          // 센서 이름 표시
    public TMP_Text txtQueryTime;           // 조회 시간 표시 

    private ToxinData currentToxinData;     // 현재 표시 중인 센서 데이터

    void Start()
    {
        // 초기에는 팝업 숨김
        panelTable.SetActive(false);

        // 닫기 버튼 이벤트 연결
        if (btnClose != null)
            btnClose.onClick.AddListener(ClosePopup);
    }

    /// <summary>
    /// 팝업 열기 및 데이터 채우기
    /// </summary>
    public void ShowPopup(ToxinData toxinData)
    {
        if (toxinData == null)
        {
            Debug.LogError("ToxinData가 null입니다!");
            return;
        }

        currentToxinData = toxinData;

        // 팝업 활성화
        panelTable.SetActive(true);

        // 센서 이름 표시
        if (txtSensorName != null)
            txtSensorName.text = $"{toxinData.hnsName}";

        // 조회 시간 표시
        UpdateQueryTime();

        // 표에 데이터 채우기
        FillTableData();
    }

    /// <summary>
    /// 조회 시간 업데이트
    /// </summary>
    private void UpdateQueryTime()
    {
        if (txtQueryTime == null) return;

        DateTime endTime = modelProvider.GetCurrentChartEndTime();
        DateTime startTime = endTime.AddHours(-12); // 12시간 전

        // 또는 실제 데이터의 시작/끝 시간 사용
        if (currentToxinData.dateTimes != null && currentToxinData.dateTimes.Count > 0)
        {
            startTime = currentToxinData.dateTimes[0];
            endTime = currentToxinData.dateTimes[currentToxinData.dateTimes.Count - 1];
        }

        txtQueryTime.text = $"{startTime:yyyy-MM-dd HH:mm} ~ {endTime:yyyy-MM-dd HH:mm}";
    }

    /// <summary>
    /// 표에 데이터 채우기 (최신 데이터가 위로)
    /// </summary>
    private void FillTableData()
    {
        if (table == null || currentToxinData == null)
        {
            Debug.LogError("Table 또는 ToxinData가 null입니다!");
            return;
        }

        // 헤더 설정 (row 0)
        table.GetCell(0, 0).text = "시간";
        table.GetCell(0, 1).text = $"측정값({currentToxinData.unit})";

        // 데이터 개수 확인
        int dataCount = Mathf.Min(
            currentToxinData.values.Count,
            currentToxinData.dateTimes != null ? currentToxinData.dateTimes.Count : 0
        );

        Debug.Log($"데이터 개수: {dataCount}");

        // 역순으로 데이터 채우기 (최신 데이터가 위로)
        for (int i = 0; i < dataCount && i < 72; i++)
        {
            int rowIndex = i + 1; // row1부터 시작
            int dataIndex = dataCount - 1 - i; // ← 역순 인덱스!

            // 시간 데이터
            if (currentToxinData.dateTimes != null && dataIndex < currentToxinData.dateTimes.Count)
            {
                DateTime time = currentToxinData.dateTimes[dataIndex];
                table.GetCell(rowIndex, 0).text = time.ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                table.GetCell(rowIndex, 0).text = "-";
            }

            // 측정값 데이터
            if (dataIndex < currentToxinData.values.Count)
            {
                float value = currentToxinData.values[dataIndex];
                table.GetCell(rowIndex, 1).text = value.ToString("F2");
            }
            else
            {
                table.GetCell(rowIndex, 1).text = "-";
            }
        }

        // 나머지 빈 행은 비우기
        for (int i = dataCount; i < 72; i++)
        {
            int rowIndex = i + 1;
            table.GetCell(rowIndex, 0).text = "";
            table.GetCell(rowIndex, 1).text = "";
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void ClosePopup()
    {
        panelTable.SetActive(false);
    }
}