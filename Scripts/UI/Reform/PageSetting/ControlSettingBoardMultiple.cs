using Assets.Scripts.Data;
using Assets.Scripts.Info;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.PageSetting
{
    internal class ControlSettingBoardMultiple : MonoBehaviour
    {
        GameObject itemPrefab => Resources.Load<GameObject>("Reform/PageSetting/ViewSettingSensorItem");
        ModelProvider modelProvider => UiManager.Instance.modelProvider;
        public BoardSpecInfo.BoardType boardType;

        Transform itemContainer;
        Button btnBoardIsInspect, btnEraseSearch;
        TMP_InputField inputSearch;

        List<ObservatoryInfo> obss;
        List<GroupInfo> groups;
        List<BoardSpecInfo> boardSpecs;


        private void Start()
        {
            itemContainer = transform.Find("ScrollSensors").Find("Content");
            inputSearch = transform.Find("txtSearchSensor").GetComponent<TMP_InputField>();
            inputSearch.onValueChanged.AddListener(OnChangeSearch);

            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);

        }

        private void OnChangeSearch(string arg0)
        {
            itemContainer.GetComponentsInChildren<ControlSettingSensorItem>(true).ToList().ForEach(item =>
            {
                bool isMatch = item.sensor.name.Contains(arg0);
                item.gameObject.SetActive(isMatch);
            });


        }

        private void OnInitiate(object obj)
        {
            obss = modelProvider.GetObss();
            groups = modelProvider.GetGroups();
                boardSpecs = modelProvider.GetBoardSpecs();
        }
        private void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) return;

            ObservatoryInfo obs = obss.Find(obs => obs.obsIdx == obsIdx);

            //전체 삭제
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }
            foreach (RectTransform item in itemContainer) { item.parent = null; Destroy(item.gameObject); }

            var boardFound = obs.boards.FindAll(board => board.type == boardType);
            ObservatoryInfo.BoardInfo board = boardFound.Count != 0? boardFound.First().info : null;

            //보드 제품 번호를 찾아서 보드 매핑 불러오기
            if (board.modelCode == null) return;

            if(boardSpecs.Find(brd => brd.modelCode == board.modelCode) == null) throw new Exception($"Board Spec not found for model code {board.modelCode}");
            BoardSpecInfo boardSpec = boardSpecs.Find(brd => brd.modelCode == board.modelCode);

            //보드에 따른 센서 목록 생성
            foreach (int sensorIdx in boardSpec.sensorsDefinitionMap.Keys)
            {
                GameObject itemObj = Instantiate(itemPrefab, itemContainer);
                ControlSettingSensorItem item = itemObj.GetComponent<ControlSettingSensorItem>();

                ObservatoryInfo.SensorInfo sensorInfo = obs.sensors.Find(sensor => sensor.idx == sensorIdx).info;
                item.SetValue(obs, sensorInfo);
            }


        }
    }
}
