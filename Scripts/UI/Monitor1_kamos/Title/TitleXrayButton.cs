using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class TitleXrayButton : MonoBehaviour
{
    public enum XrayType 
    {
        Equipment,  //장비
        Structure,  //건물 
    }
    public XrayType xrayType;

    Button btnXray;
    TMP_Text lblText;

    private static bool isStructureXrayActive = false;
    private static bool isEquipmentXrayActive = false;


    private void Start()
    {
        btnXray = GetComponentInChildren<Button>();
        btnXray.onClick.AddListener(OnClick);

        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);

        this.gameObject.SetActive(false);
    }
    private void OnValidate()
    {
        lblText = GetComponentInChildren<TMP_Text>();
        lblText.text = xrayType == XrayType.Structure ? "건물 X-Ray" : "장비 X-Ray";
    }

    void OnClick() 
    {
        //Debug.Log($"X-Ray 버튼 클릭: {xrayType}");

        GameObject tObj = GetXrayTarget(xrayType);
        if (tObj == null) return;

        //tObj.SetActive(!tObj.activeSelf);
        bool newState = !tObj.activeSelf;
        tObj.SetActive(newState);

        // 현재 X-Ray 상태 업데이트
        if (xrayType == XrayType.Structure)
        {
            isStructureXrayActive = !newState; // SetActive(false) = X-Ray 활성화
        }
        else if (xrayType == XrayType.Equipment)
        {
            isEquipmentXrayActive = !newState; // SetActive(false) = X-Ray 활성화
        }

        // 센서 깜박임 제어: 둘 다 X-Ray 활성화일 때만 보임
        ControlSensorVisibility();

        //Debug.Log($"X-Ray 상태 변경: {tObj.name} → {newState}");
    }
    void ControlSensorVisibility()
{
    try
    {
        // 둘 다 X-Ray 활성화일 때만 센서들이 보이도록 함
        bool shouldShowSensors = isStructureXrayActive && isEquipmentXrayActive;

        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        GameObject area3D = rootObjects.ToList().Find(go => go.name == "Area_3D");

        if (area3D != null)
        {
            Transform observatory = area3D.transform.Find("Observatory");
            if (observatory != null)
            {
                Transform sensors = observatory.Find("Sensors");
                if (sensors != null)
                {
                    //Debug.Log($"센서 표시 제어: {shouldShowSensors}");

                    // 각 센서 GameObject의 활성화 상태 제어 (가장 간단하고 확실한 방법)
                    foreach (Transform sensor in sensors)
                    {
                        if (sensor.name.StartsWith("Sensor_"))
                        {
                            sensor.gameObject.SetActive(shouldShowSensors);
                        }
                    }

                    //Debug.Log($"모든 센서 활성화 상태: {shouldShowSensors}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"센서 표시 제어 실패: {ex.Message}");
        Debug.LogException(ex);
    }
}
    /*void ControlStepIndicators()
    {
        try
        {
            // 둘 다 X-Ray 활성화되었을 때만 센서 깜박임 보임
            bool shouldShowSensors = isStructureXrayActive && isEquipmentXrayActive;

            ObsSensorStepAnimator stepAnimator = FindObjectOfType<ObsSensorStepAnimator>();
            if (stepAnimator != null)
            {
                Transform stepCanvas = stepAnimator.transform.Find("Canvas");
                if (stepCanvas != null)
                {
                    stepCanvas.gameObject.SetActive(shouldShowSensors);
                    Debug.Log($"센서 깜박임 표시: {shouldShowSensors}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"스텝 표시 제어 실패: {ex.Message}");
        }
    }*/

    private void OnNavigateArea(object obj)
    {
        ResetXrayState();
        this.gameObject.SetActive(false);
    }
    private void OnNavigateHome(object obj)
    {
        ResetXrayState();
        this.gameObject.SetActive(false);
    }
    private void OnNavigateObs(object obj)
    {
        ResetXrayState();
        this.gameObject.SetActive(true);

        // X-Ray 초기 상태: 모든 오브젝트 보이게 설정
        GameObject structureObj = GetXrayTarget(XrayType.Structure);
        GameObject equipmentObj = GetXrayTarget(XrayType.Equipment);

        if (structureObj != null) structureObj.SetActive(true);
        if (equipmentObj != null) equipmentObj.SetActive(true);

        // 센서 깜박임 숨김
        ControlSensorVisibility();

        /*GameObject tObj = GetXrayTarget(xrayType);
        tObj.SetActive(true);*/
    }

    private void ResetXrayState()
    {
        isStructureXrayActive = false;
        isEquipmentXrayActive = false;
    }

    GameObject GetXrayTarget(XrayType xrayType)
    {
        try
        {
            //해당 코드는 대상 3D 모델에 직접 접근해 SetActive를 호출하는 과정임.
            //하지만 UiManager의 이벤트 호출로 이를 제어하는게 일관적인 처리로 기대됨.
            //때문에 후에UiManager에 ToggleXray같은 이벤트를 추가하는 것이 좋아보임.

            Scene currentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = currentScene.GetRootGameObjects();
            GameObject area3D = rootObjects.ToList().Find(go => go.name == "Area_3D");
            Transform observatory = area3D.transform.Find("Observatory");

            if (xrayType == XrayType.Equipment)
            {
                return observatory.Find("Equipments").gameObject;
            }
            else if (xrayType == XrayType.Structure)
            {
                return observatory.Find("OuterWall").gameObject;
            }
            return null;
        }
        catch (Exception ex) 
        {
            Debug.LogError("TitleXrayButton - GetXrayTarget() - error occured!");
            Debug.LogException(ex);
            
        }

        return null;
    }



}
