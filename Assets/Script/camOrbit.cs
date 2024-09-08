using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform cubeTarget; // 指向立方体
    public Button stopButton1; // 场景 1 的停止按钮
    public Button stopButton2; // 场景 2 的停止按钮
    public GameObject[] triggerSpheres; // 用于存储多个触发球体

    private Vector3[] focusPositions; // 用于存储相机的三个特定位置
    private Quaternion[] focusRotations; // 用于存储相机的三个特定旋转
    private bool isRotating = true;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private GameObject lastClickedSphere = null;

    // 场景 1 和 场景 2 的特定位置
    private Vector3 scene1StopPosition = new Vector3(6, 5, -10);
    private Quaternion scene1StopRotation;

    private Vector3 scene2StopPosition = new Vector3(3, 4, -5);
    private Quaternion scene2StopRotation;

    void Start()
    {
        // 初始化场景 1 和场景 2 的特定位置和旋转
        scene1StopRotation = Quaternion.LookRotation(cubeTarget.position - scene1StopPosition);
        scene2StopRotation = Quaternion.LookRotation(cubeTarget.position - scene2StopPosition);

        // 初始化初始位置和旋转
        initialPosition = new Vector3(6, 5, -10);
        initialRotation = Quaternion.LookRotation(cubeTarget.position - initialPosition);

        // 初始化三个特定位置和旋转
        focusPositions = new Vector3[3];
        focusRotations = new Quaternion[3];
        focusPositions[0] = new Vector3(0.5f, 1, -4f);
        focusRotations[0] = Quaternion.LookRotation(cubeTarget.position - focusPositions[0]);

        focusPositions[1] = new Vector3(3, 4, -5);
        focusRotations[1] = Quaternion.LookRotation(cubeTarget.position - focusPositions[1]);

        focusPositions[2] = new Vector3(7, 2, -6);
        focusRotations[2] = Quaternion.LookRotation(cubeTarget.position - focusPositions[2]);

        // 按钮点击事件绑定
        if (stopButton1 != null)
            stopButton1.onClick.AddListener(OnStopButton1Click); // 绑定场景 1 的停止按钮
        if (stopButton2 != null)
            stopButton2.onClick.AddListener(OnStopButton2Click); // 绑定场景 2 的停止按钮
    }

    void Update()
    {
        if (isRotating)
        {
            // 每帧更新相机的旋转，使其始终面向立方体
            transform.LookAt(cubeTarget.position);
            // 围绕立方体旋转
            transform.RotateAround(cubeTarget.position, Vector3.up, 20 * Time.deltaTime);
        }

        // 鼠标点击检测
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < triggerSpheres.Length; i++)
                {
                    if (hit.collider.gameObject == triggerSpheres[i])
                    {
                        if (lastClickedSphere == triggerSpheres[i])
                        {
                            // 如果再次点击同一个球体，摄像机回到初始位置
                            SetCameraPosition(initialPosition, initialRotation, cubeTarget.position);
                            lastClickedSphere = null; // 重置
                        }
                        else
                        {
                            // 移动摄像机到新的位置，并对准点击的球体
                            SetCameraPosition(focusPositions[i], focusRotations[i], triggerSpheres[i].transform.position);
                            lastClickedSphere = triggerSpheres[i]; // 记录最后点击的球体
                        }
                        break;
                    }
                }
            }
        }
    }

    void OnStopButton1Click()
    {
        // 场景 1 的停止按钮功能：移动摄像机到场景 1 的特定位置
        isRotating = false;
        SetCameraPosition(scene1StopPosition, scene1StopRotation, cubeTarget.position);
    }

    void OnStopButton2Click()
    {
        // 场景 2 的停止按钮功能：移动摄像机到场景 2 的特定位置
        isRotating = false;
        SetCameraPosition(scene2StopPosition, scene2StopRotation, cubeTarget.position);
    }

    void SetCameraPosition(Vector3 position, Quaternion rotation, Vector3 lookAtPosition)
    {
        StartCoroutine(MoveCameraToPosition(position, rotation, lookAtPosition));
    }

    IEnumerator MoveCameraToPosition(Vector3 position, Quaternion rotation, Vector3 lookAtPosition)
    {
        float duration = 2.0f; // 移动和旋转的持续时间
        float elapsed = 0.0f;

        Vector3 startingPosition = transform.position;
        Quaternion startingRotation = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 平滑移动和旋转
            transform.position = Vector3.Lerp(startingPosition, position, t);
            transform.rotation = Quaternion.Slerp(startingRotation, rotation, t);
            transform.LookAt(lookAtPosition);

            yield return null;
        }

        // 确保相机最终位置和旋转正确
        transform.position = position;
        transform.rotation = rotation;
        transform.LookAt(lookAtPosition);
    }
}
