using UnityEngine;

public class CameraControl2 : MonoBehaviour
{
    public Camera mainCamera;
    public Transform targetCube;
    public GameObject triggerSphere;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector3 sideViewPosition;
    private Quaternion sideViewRotation;

    private int clickCount = 0;

    void Start()
    {
        // 初始化相机的初始位置和旋转（正视图）
        initialPosition = new Vector3(6, 5, -10);
        initialRotation = Quaternion.LookRotation(targetCube.position - initialPosition);

        // 计算侧视图的位置和旋转
        sideViewPosition = new Vector3(0.5f, 1, -4f);
        sideViewRotation = Quaternion.LookRotation(targetCube.position - sideViewPosition);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == triggerSphere)
                {
                    clickCount++;
                    switch (clickCount)
                    {
                        case 1:
                            // 设置相机到正视图
                            SetCameraPosition(sideViewPosition, sideViewRotation);
                            break;
                        case 2:
                            // 设置相机到侧视图并靠近正方体
                            SetCameraPosition(initialPosition, initialRotation);
                            clickCount = 0; // 重置点击计数
                            break;
                        case 3:
                            // 设置相机回到正视图
                            SetCameraPosition(initialPosition, initialRotation);
                            clickCount = 0; // 重置点击计数
                            break;
                    }
                }
            }
        }
    }

    void SetCameraPosition(Vector3 position, Quaternion rotation)
    {
        StartCoroutine(TransitionToPosition(position, rotation));
    }

    System.Collections.IEnumerator TransitionToPosition(Vector3 position, Quaternion rotation)
    {
        float duration = 1f; // 过渡持续时间
        float elapsed = 0f;

        Vector3 startingPos = mainCamera.transform.position;
        Quaternion startingRot = mainCamera.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainCamera.transform.position = Vector3.Lerp(startingPos, position, elapsed / duration);
            mainCamera.transform.rotation = Quaternion.Lerp(startingRot, rotation, elapsed / duration);
            yield return null;
        }

        mainCamera.transform.position = position;
        mainCamera.transform.rotation = rotation;
    }
}
