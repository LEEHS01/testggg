using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageObservatory
{
    public class ViewObsSensorList : MonoBehaviour
    {
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageObservatory/ViewObsSensorItem");
        GameObject splitterPrefab => Resources.Load<GameObject>("Reform/PageObservatory/ViewObsSensorSplitter");
        ModelProvider modelProvider => UiManager.Instance.modelProvider;





        RectTransform listPanel;
        ObservatoryInfo obsInfo = null;


        Button btnSwitch;
        DG.Tweening.Tween switchTween; //전환 애니메이션용 트윈
        bool isViewMinimized = false; //최소화 상태인지 여부

        private void Start()
        {
            listPanel = transform.Find("ScrollSensors").Find("Content").GetComponent<RectTransform>();
            btnSwitch = transform.Find("TopBar").Find("BtnSwitch").GetComponent<Button>();
            btnSwitch.onClick.AddListener(OnClickSwitch);

            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
            UiManager.Instance.Register(UiEventType.ObsUpdate, OnObsUpdate);

        }

        private void OnObsUpdate(object obj)
        {
            if (obsInfo == null) return;

            //관측소 내 알람 활성화된 것들만
            var alarmsActivated = modelProvider.GetAlarmsActivated().Where(alarm => alarm.obsIdx == obsInfo.obsIdx);

            //관측소 정보
            obsInfo = modelProvider.GetObsByIdx(obsInfo.obsIdx);

            listPanel.GetComponentsInChildren<ViewObsSensorItem>().ToList().ForEach(vosi =>
            {
                (int obsIdx, int sensorIdx) sensorAddress = vosi.sensorAddress;

                ObservatoryInfo.SensorInfo sensorInfo = obsInfo.sensors.Find(sensor => sensor.idx == sensorAddress.sensorIdx).info;

                List<AlarmInfo> alarmsActivatedSelSensor = alarmsActivated.Where(alarm => alarm.sensorIdx == sensorAddress.sensorIdx).ToList();

                vosi.SetValue(
                    sensorAddress, 
                    sensorInfo,
                    new TimeSeriesInfo(new List<ModelsReform.HnsDataMeasData>()), //TODO : 센서의 측정값 시계열 정보 넣어주기
                    alarmsActivatedSelSensor.FirstOrDefault() ?? null    //alarmsActivatedSelSensor가 1 또는 0개라는 전제
                    );
            });
        }

        private void OnClickSwitch()
        {
            Vector2 sizeMaximize = new Vector2(1368, 872) * Screen.width / 1920;
            Vector2 sizeMinimize = new Vector2(430, 40) * Screen.width / 1920;

            if (switchTween != null) return;

            Vector2 fromVec = isViewMinimized ? sizeMinimize : sizeMaximize;
            Vector2 toPos = isViewMinimized ? sizeMaximize : sizeMinimize;
            float duration = 0.5f;



            switchTween = DOTween.To(() => fromVec, x => fromVec = x, toPos, duration).OnUpdate(() =>
            {
                GetComponent<RectTransform>().anchoredPosition = new Vector2(-fromVec.x / 2f, fromVec.y / 2f);
                GetComponent<RectTransform>().sizeDelta = fromVec;
            });
            switchTween.onComplete += () => switchTween = null;
            switchTween.onComplete += () => isViewMinimized = !isViewMinimized;
        }

        void OnSelectObs(object obj) 
        {
            if (obj is not int obsIdx) return;

            foreach (RectTransform item in listPanel) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in listPanel) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in listPanel) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in listPanel) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in listPanel) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in listPanel) { item.parent = null; Destroy(item.gameObject); }

            //관측소 정보
            obsInfo = modelProvider.GetObsByIdx(obsIdx);

            //관측소 내 알람 활성화된 것들만
            var alarmsActivated = modelProvider.GetAlarmsActivated().Where(alarm => alarm.obsIdx == obsIdx);

            //구분자 매핑 > sibling index 조정을 위해 필요
            Dictionary<BoardSpecInfo.BoardType, ViewObsSensorSplitter> boardTypeSplitterMap = new();

            //구분자(보드) 생성
            var boardsExists = obsInfo.boards.Where(brd => brd.info.modelCode != "" && brd.info.modelCode != null).ToList(); //모델 코드 없는 보드는 구분자 생성하지 않음
            boardsExists.ForEach(kvp => 
            {
                ObservatoryInfo.BoardInfo board = kvp.info;
                //modelProvider.GetBoardSpecs().ForEach(spec => Debug.Log($"보드 모델 코드 : {spec.modelCode}, 보드 모델명 : {spec.nameText}"));

                BoardSpecInfo spec = modelProvider.GetBoardSpecs().Find(spec => spec.modelCode == board.modelCode);
                //Debug.Log($"!!! 보드 모델 코드 : {board.modelCode}, 보드 모델명 : {spec?.nameText}");

                GameObject instantSplitter = Instantiate(splitterPrefab, listPanel);
                //Debug.Log($"스플리터 컴포넌트 {instantSplitter.GetComponent<ViewObsSensorSplitter>().name}");
                instantSplitter.GetComponent<ViewObsSensorSplitter>().SetValue(spec.nameText, board);//board.type
                boardTypeSplitterMap.Add(kvp.type, instantSplitter.GetComponent<ViewObsSensorSplitter>());

            });

            //요소(센서) 생성
            obsInfo.sensors.ForEach(sensor =>
            {
                if (!sensor.info.isUsing) return;//활성화된 센서만 생성

                //알람 수집
                List<AlarmInfo> alarmsActivatedSelSensor = alarmsActivated.Where(alarm => alarm.sensorIdx == sensor.idx).ToList();

                //센서의 소속 보드 확인
                var tBoards = obsInfo.boards.Where(kvp =>
                {
                    ObservatoryInfo.BoardInfo board = kvp.info;
                    BoardSpecInfo spec = modelProvider.GetBoardSpecs().Find(spec => spec.modelCode == board.modelCode);

                    return spec.sensorsDefinitionMap.TryGetValue(sensor.idx, out var sensorDef);
                }).ToList();
                BoardSpecInfo.BoardType? boardType = tBoards.Count == 1? tBoards.First().type : null;

                if (!boardType.HasValue) return;//소속 보드가 없는 센서는 생성하지 않음

                //생성
                GameObject instantItem = Instantiate(itemPrefab, listPanel);
                instantItem
                .GetComponent<ViewObsSensorItem>()
                .SetValue(
                    (obsIdx, sensor.idx),
                    sensor.info,
                    new TimeSeriesInfo(new List<ModelsReform.HnsDataMeasData>()), //TODO : 센서의 측정값 시계열 정보 넣어주기
                    alarmsActivatedSelSensor.FirstOrDefault() ?? null    //alarmsActivatedSelSensor가 1 또는 0개라는 전제
                    );

                //생성 후 구분자 바로 다음으로 이동
                int splitterSiblingIndex = boardTypeSplitterMap[boardType.Value].transform.GetSiblingIndex();
                int childCount = listPanel.childCount;

                instantItem.transform.SetSiblingIndex(splitterSiblingIndex+1);
            });



        }
    }
}
