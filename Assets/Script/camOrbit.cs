using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraOrbit : MonoBehaviour
{
    public Transform cubeTarget; // 指向立方体
    public Button startButton;


    private bool isRotating = true;
    private Vector3 targetPosition = new Vector3(6, 5, -10);
    private Quaternion targetRotation;

    void Start()
    {
        // 初始化目标旋转
        targetRotation = Quaternion.LookRotation(cubeTarget.position - targetPosition);

        // 按钮点击事件绑定
        startButton.onClick.AddListener(OnStartButtonClick);


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
    }

    void OnStartButtonClick()
    {
        // 停止旋转并开始协程
        isRotating = false;
        StartCoroutine(MoveCameraToPosition());
    }

    IEnumerator MoveCameraToPosition()
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
            transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, t);

            yield return null;
        }

        // 确保相机最终位置和旋转正确
        transform.position = targetPosition;
        transform.rotation = targetRotation;


    }
}
