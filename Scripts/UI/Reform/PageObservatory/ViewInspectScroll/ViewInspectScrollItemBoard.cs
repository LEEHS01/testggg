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

namespace Assets.Scripts.UI.Reform.PageObservatory
{
    internal class ViewInspectScrollItemBoard : MonoBehaviour
    {
        [SerializeField]
        public BoardSpecInfo.BoardType boardType;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        TMP_Text txtTop, txtMiddle, txtBottom, txtTitle;
        Image imgSensor;

        Vector2 defaultSizeDelta;
        bool isInit = false;
        private void Start()
        {
            txtTop = transform.Find("txtTop").GetComponent<TMP_Text>();
            txtMiddle = transform.Find("txtMiddle").GetComponent<TMP_Text>();
            txtBottom = transform.Find("txtBottom").GetComponent<TMP_Text>();
            txtTitle = transform.Find("Title").Find("TxtTitle").GetComponent<TMP_Text>();
            imgSensor = transform.Find("ImgSensorSocket").Find("ImgSensor").GetComponent<Image>();

            defaultSizeDelta = GetComponent<RectTransform>().sizeDelta;

            UiManager.Instance.Register(UiEventType.SelectObs, OnSelectObs);
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        }

        private void OnInitiate(object obj)
        {
            isInit = true;
        }

        void OnSelectObs(object obj)
        {
            if (obj is not int obsIdx) return;

            var obs = modelProvider.GetObsByIdx(obsIdx);


            ObservatoryInfo.BoardInfo board = obs.boards.Find(kvp => kvp.type == boardType).info;
            BoardSpecInfo spec = modelProvider.GetBoardSpecs().Find(spec => spec.modelCode == board.modelCode);

            if (spec is not null)
            {
                // 상태 코드 해석 필요
                //TODO

                txtTitle.text = 
                    $"'{spec.nameText}' 보드 정보";
                txtTop.text = 
                    $"보드 이름 \t: {spec.nameText}\r\n" +
                    $"제조사 \t: {spec.manufacturer}";
                txtMiddle.text = 
                    $"모델 코드\t: {spec.modelCode}\r\n" +
                    $"활성화 여부\t: {(board.isInspecting? board.isUsing ? "활성화됨" : "비활성화됨" : "점검 중")}\r\n\r\n" +
                    $"작동 상태 (작동 / 연결 / 행정) : ({board.stateLife} / {board.stateCom} / {board.stateOp})\r\n" +
                    $"상태 코드 (작동 / 연결 / 행정) : ({board.stateLife} / {board.stateCom} / {board.stateOp})\r\n\r\n" +
                    $"상태 설명\t:  {"상태 설명을 확인할 수 없습니다..."} \r\n";
                txtBottom.text = $"- 살아있는 박테리아(Aliivibrio Fischeri)의 생태변화를 통해 물속의 독성물질을 감시하는 역할로 독성물질이 유출되면 알람을 통해 경보하는 장치\r\n.\r\n- 식수 및 폐수처리시설, 지하수, 해양오염, 저감시설 등에 적용";

                // 보드 이미지 설정 필요
                imgSensor.gameObject.SetActive(true);

                gameObject.SetActive(true);
                GetComponent<RectTransform>().sizeDelta = defaultSizeDelta;
            }
            else
            {
                txtTitle.text = 
                    $"-- 보드 정보";
                txtTop.text = 
                    $"보드 이름 \t: --\r\n" +
                    $"제조사 \t: --";
                txtMiddle.text = 
                    $"모델 코드\t: --\r\n" +
                    $"활성화 여부\t: --\r\n\r\n" +
                    $"작동 상태 (작동 / 연결 / 행정) : (-- / -- / --)\r\n" +
                    $"상태 코드 (작동 / 연결 / 행정) : (-- / -- / --)\r\n\r\n" +
                    $"상태 설명\t:  --- \r\n";
                txtBottom.text = $"-";

                imgSensor.gameObject.SetActive(!isInit);



                gameObject.SetActive(!isInit);
                GetComponent<RectTransform>().sizeDelta = new(0, 0);
            }
        }


    }
}
