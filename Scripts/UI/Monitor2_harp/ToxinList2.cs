using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


internal class ToxinList2 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private LogData log;                      // 알람 로그 데이터
    private List<ToxinBar2> bars = new();      // 독성 바 목록
    public RectTransform scrollContainer;      // 스크롤 컨테이너
    //public GameObject barPrefab;               // Bar 프리팹
    public Transform barParent;                // Bar가 추가될 부모 객체
    public TMP_Text txtName;                   // 이름 텍스트
    public SetTime txtTime;                    // 로그 시간 텍스트 1
    public SetTime txtTime2;                   // 로그 시간 텍스트 2

    int obsId;
    Vector3 defaultPos;


    void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectLog);
        UiManager.Instance.Register(UiEventType.ChangeAlarmSensorList, OnLoadAlarmData);


        var barComps = scrollContainer.GetComponentsInChildren<ToxinBar2>();
        bars.AddRange(barComps);
        bars.ForEach(bar => bar.gameObject.SetActive(false));

        defaultPos = transform.position;
        gameObject.SetActive(false);
        //InitializeBars();
    }

    //private void InitializeBars()
    //{
    //    // 기존 바 제거 및 리스트 초기화
    //    foreach (Transform child in barParent)
    //    {
    //        if(child.gameObject != barPrefab)
    //            Destroy(child.gameObject);
    //    }
    //    bars.Clear();
    //}


    private void OnSelectLog(object data)
    {
        if (data is not int logIdx) return;

        log = modelProvider.GetAlarm(logIdx);
        this.obsId = log.obsId;
        txtName.text = $"{log.areaName} - {log.obsName}";
        txtTime.SetText(log.time);       
        txtTime2.SetText(log.time);

        transform.position = defaultPos;
        gameObject.SetActive(true);
    }

    /*  private void OnLoadAlarmData(object obj)
      {
          Debug.Log("=== ToxinList2 OnLoadAlarmData 시작 ===");

          List<ToxinData> toxinDatas = modelProvider.GetToxinsInLog();

          Debug.Log($"받은 전체 센서 데이터 수: {toxinDatas.Count}");

          // 각 센서의 상세 정보 로그
          for (int i = 0; i < toxinDatas.Count; i++)
          {
              var toxin = toxinDatas[i];
              Debug.Log($"센서[{i}]: {toxin.hnsName} (board:{toxin.boardid}, hns:{toxin.hnsid})");
              Debug.Log($"  - on: {toxin.on}, warning: {toxin.warning}");
              Debug.Log($"  - values count: {toxin.values?.Count ?? 0}");
              if (toxin.values != null && toxin.values.Count > 0)
              {
                  Debug.Log($"  - last value: {toxin.values[toxin.values.Count - 1]}");
              }
          }

          if (toxinDatas.Count == 0)
          {
              Debug.LogError("알람 센서 데이터가 없습니다!");
              return;
          }

          List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;
          toxinBoard = toxinDatas.Where(item => item.boardid == 1).ToList();
          chemicalBoard = toxinDatas.Where(item => item.boardid == 2).ToList();
          qualityBoard = toxinDatas.Where(item => item.boardid == 3).ToList();

          Debug.Log($"보드별 센서 수 - 독성도:{toxinBoard.Count}, 화학:{chemicalBoard.Count}, 수질:{qualityBoard.Count}");

          List<ToxinData> sortedToxins = new();
          sortedToxins.AddRange(toxinBoard);
          sortedToxins.AddRange(qualityBoard);
          sortedToxins.AddRange(chemicalBoard);

          Debug.Log($"bars.Count: {bars.Count}, sortedToxins.Count: {sortedToxins.Count}");

          float scrollHeight = 0;

          for (int i = 0; i < sortedToxins.Count; i++)
          {
              if (i >= bars.Count)
              {
                  Debug.LogWarning($"[ToxinList2] Bar index {i} is out of range");
                  continue;
              }

              ToxinData toxin = sortedToxins[i];
              ToxinBar2 bar = bars[i];

              Debug.Log($"Bar[{i}] 설정 시작: {toxin.hnsName}");

              // 센서 상태 가져오기
              ToxinStatus status = modelProvider.GetSensorStatus(obsId, toxin.boardid, toxin.hnsid);

              bar.SetToxinData(obsId, toxin, status);
              bar.gameObject.SetActive(toxin.on);

              Debug.Log($"Bar[{i}] 설정 완료: active={toxin.on}");

              if (toxin.on)
                  scrollHeight += bar.GetComponent<RectTransform>().rect.height;
          }

          RectTransform rect = this.scrollContainer.GetComponent<RectTransform>();
          rect.DOSizeDelta(new Vector2(rect.rect.width, scrollHeight + 76), 0);

          LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollContainer);

          Debug.Log($"=== ToxinList2 OnLoadAlarmData 완료: scrollHeight={scrollHeight} ===");
      }*/

    private void OnLoadAlarmData(object obj)
    {
        List<ToxinData> toxinDatas = modelProvider.GetToxinsInLog();

        List<ToxinData> toxinBoard, chemicalBoard, qualityBoard;
        toxinBoard = toxinDatas.Where(item => item.boardid == 1).ToList();
        chemicalBoard = toxinDatas.Where(item => item.boardid == 2).ToList();
        qualityBoard = toxinDatas.Where(item => item.boardid == 3).ToList();

        List<ToxinData> sortedToxins = new();
        sortedToxins.AddRange(toxinBoard);
        sortedToxins.AddRange(qualityBoard);
        sortedToxins.AddRange(chemicalBoard);


        float scrollHeight = 0;

        for (int i = 0; i < sortedToxins.Count; i++)
        {
            if (i >= bars.Count)
            {
                Debug.LogWarning($"[ToxinList2] Bar index {i} is out of range");
                continue;
            }

            ToxinData toxin = sortedToxins[i];
            ToxinBar2 bar = bars[i];

            bar.SetToxinData(obsId, toxin, toxin.status);
            bar.gameObject.SetActive(toxin.on);

            if (toxin.on)
                scrollHeight += bar.GetComponent<RectTransform>().rect.height;
        }


        RectTransform rect = this.scrollContainer.GetComponent<RectTransform>();
        rect.DOSizeDelta(new Vector2(rect.rect.width, scrollHeight + 76), 0);

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollContainer);
    }


}

