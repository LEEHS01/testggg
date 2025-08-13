using UnityEngine;

public class DisplayActivator : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"[DisplayActivator] Before: displays.Length={Display.displays.Length}");
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
        Debug.Log($"[DisplayActivator] After: displays.Length={Display.displays.Length}");

        // 각 디스플레이 상태 출력
        for (int i = 0; i < Display.displays.Length; i++)
        {
            var d = Display.displays[i];
            Debug.Log($"[Display {i}] system={d.systemWidth}x{d.systemHeight}, rendering={d.renderingWidth}x{d.renderingHeight}");
        }
    }

}
