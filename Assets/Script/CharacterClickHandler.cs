using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking; // 用于发送HTTP请求
using System.Collections;

public class CharacterInteraction : MonoBehaviour
{
    public Transform[] characters; // 三个人物的Transform
    public Transform cameraTransform; // 摄像头的Transform
    public Vector3 cameraOffset; // 摄像头的偏移位置，用于调整到人物正前方
    public GameObject objectToDisable; // 需要关闭的物体
    public GameObject objectToDisableTwo; // 需要关闭的物体2
    public GameObject uiPanel; // 需要打开的UI界面
    public Button exitButton; // UI界面的Exit按钮
    public Button sendButton; // 发送数据的按钮
    public InputField inputField; // 输入框，用于获取输入文本
    public float moveSpeed = 5f; // 摄像头移动速度
    public float rotateSpeed = 5f; // 摄像头旋转速度

    private Vector3 originalCameraPosition; // 记录摄像头的初始位置
    private Quaternion originalCameraRotation; // 记录摄像头的初始旋转
    private Vector3 targetPosition; // 摄像头目标位置
    private Quaternion targetRotation; // 摄像头目标旋转
    private bool isMoving = false; // 判断摄像头是否正在移动
    private bool isMovingBack = false; // 判断摄像头是否正在移动
    private bool isPaused = false; // 判断游戏是否暂停
    private int AgentNum = -1; // 用于区分点击的人物编号，初始为-1表示未选择

    void Start()
    {
        // 初始化UI界面和按钮
        uiPanel.SetActive(false);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked); // 绑定发送按钮点击事件
        sendButton.onClick.AddListener(OnExitButtonClicked);
    }

    void Update()
    {
        // 检测鼠标点击
        if (Input.GetMouseButtonDown(0) && !isPaused && !isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < characters.Length; i++)
                {
                    if (hit.transform == characters[i])
                    {
                        // 记录摄像头的初始位置和旋转
                        originalCameraPosition = cameraTransform.position;
                        originalCameraRotation = cameraTransform.rotation;

                        // 设置 AgentNum，区分点击的人物
                        AgentNum = i;

                        // 计算目标位置，使人物在画面的右侧
                        Vector3 characterScreenPos = Camera.main.WorldToScreenPoint(characters[i].position);
                        float screenWidth = Screen.width;
                        float offsetX = screenWidth * 0.25f; // 让人物保持在屏幕右侧四分之一处

                        // 转换屏幕空间到世界空间的偏移量
                        Vector3 screenTargetPos = new Vector3(offsetX, characterScreenPos.y, characterScreenPos.z);
                        targetPosition = Camera.main.ScreenToWorldPoint(screenTargetPos) + cameraOffset;

                        // 计算目标旋转，使摄像头始终面向人物
                        targetRotation = Quaternion.LookRotation(characters[i].position - targetPosition);

                        // 开始移动摄像头
                        isMoving = true;

                        // 发送初始的JSON请求，state为"input"，其他为空
                        SendInitialRequest();

                        break;
                    }
                }
            }
        }

        // 摄像头平滑移动和旋转
        if (isMoving)
        {
            objectToDisableTwo.SetActive(false);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // 判断是否到达目标位置和旋转
            if (Vector3.Distance(cameraTransform.position, targetPosition) < 0.1f && Quaternion.Angle(cameraTransform.rotation, targetRotation) < 1f)
            {
                // 停止移动并执行其他操作
                cameraTransform.position = targetPosition;
                cameraTransform.rotation = targetRotation;
                isMoving = false;

                // 关闭物体并打开UI界面
                objectToDisable.SetActive(false);
                uiPanel.SetActive(true);

                // 暂停游戏
                Time.timeScale = 0.01f;
                isPaused = true;
            }
        }

        if (isMovingBack)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // 判断是否到达目标位置和旋转
            if (Vector3.Distance(cameraTransform.position, targetPosition) < 0.1f && Quaternion.Angle(cameraTransform.rotation, targetRotation) < 1f)
            {
                // 停止移动并执行其他操作
                cameraTransform.position = targetPosition;
                cameraTransform.rotation = targetRotation;
                isMovingBack = false;

            }
        }
    }

    // 发送初始的空JSON请求
    void SendInitialRequest()
    {
        // 创建初始JSON数据
        string jsonData = "{\"state\": \"input\", \"num\": \"" + AgentNum + "\", \"word\": \"\"}";

        // 启动协程发送POST请求
        StartCoroutine(SendPostRequest("http://localhost:3000/api/receive-data", jsonData));
    }

    // 当点击发送数据按钮时，发送完整的JSON数据
    void OnSendButtonClicked()
    {
        // 获取InputField的文本
        string userInput = inputField.text.Trim();

        // 创建最终的JSON数据
        string jsonData = "{\"state\": \"sent\", \"num\": \"" + AgentNum + "\", \"word\": \"" + userInput + "\"}";

        // 启动协程发送POST请求
        StartCoroutine(SendPostRequest("http://localhost:3000/api/receive-data", jsonData));
    }

    IEnumerator SendPostRequest(string url, string jsonData)
    {
        // 创建请求
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 发送请求并等待响应
        yield return request.SendWebRequest();

        // 处理响应
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("POST request succeeded: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("POST request failed: " + request.error);
        }
    }

    void OnExitButtonClicked()
    {
        // 恢复摄像头的目标位置和旋转为初始位置
        targetPosition = originalCameraPosition;
        targetRotation = originalCameraRotation;

        // 开始移动摄像头回原位
        isMovingBack = true;

        // 重新打开被关闭的物体并关闭UI界面
        objectToDisable.SetActive(true);
        objectToDisableTwo.SetActive(true);
        uiPanel.SetActive(false);

        // 重新开始游戏
        Time.timeScale = 1;
        isPaused = false;
    }
}
