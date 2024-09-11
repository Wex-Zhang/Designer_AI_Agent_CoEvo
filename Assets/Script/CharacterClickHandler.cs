using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net.Sockets; // 用于UDP通信
using System.Text;

public class CharacterInteraction : MonoBehaviour
{
    public Transform[] characters;
    public Transform cameraTransform;
    public Vector3 cameraOffset;
    public GameObject objectToDisable;
    public GameObject objectToDisableTwo;
    public GameObject uiPanel;
    public Button exitButton;
    public Button sendButton;
    public InputField inputField;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool isMovingBack = false;
    private bool isPaused = false;
    private int AgentNum = -1;

    private UdpClient udpClient;
    public string serverIp = "127.0.0.1"; // 后端的IP地址
    public int serverPort = 3000; // 后端的端口号

    void Start()
    {
        udpClient = new UdpClient();
        uiPanel.SetActive(false);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked);
        sendButton.onClick.AddListener(OnExitButtonClicked);
    }

    void Update()
    {
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
                        originalCameraPosition = cameraTransform.position;
                        originalCameraRotation = cameraTransform.rotation;

                        AgentNum = i;

                        Vector3 characterScreenPos = Camera.main.WorldToScreenPoint(characters[i].position);
                        float screenWidth = Screen.width;
                        float offsetX = screenWidth * 0.25f;

                        Vector3 screenTargetPos = new Vector3(offsetX, characterScreenPos.y, characterScreenPos.z);
                        targetPosition = Camera.main.ScreenToWorldPoint(screenTargetPos) + cameraOffset;

                        targetRotation = Quaternion.LookRotation(characters[i].position - targetPosition);

                        isMoving = true;
                        break;
                    }
                }
            }
        }

        if (isMoving)
        {
            objectToDisableTwo.SetActive(false);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(cameraTransform.position, targetPosition) < 0.1f && Quaternion.Angle(cameraTransform.rotation, targetRotation) < 1f)
            {
                cameraTransform.position = targetPosition;
                cameraTransform.rotation = targetRotation;
                isMoving = false;

                objectToDisable.SetActive(false);
                uiPanel.SetActive(true);

                Time.timeScale = 0.01f;
                isPaused = true;
            }
        }

        if (isMovingBack)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(cameraTransform.position, targetPosition) < 0.1f && Quaternion.Angle(cameraTransform.rotation, targetRotation) < 1f)
            {
                cameraTransform.position = targetPosition;
                cameraTransform.rotation = targetRotation;
                isMovingBack = false;
            }
        }
    }

    // 当点击发送数据按钮时，发送完整的JSON数据
    void OnSendButtonClicked()
    {
        string userInput = inputField.text.Trim();
        string jsonData = "{\"command\": \"add_memory\", \"content\": {\"agent_number\": \"" + AgentNum + "\", \"memory\": \"" + userInput + "\"}}";

        // 启动协程发送UDP消息
        StartCoroutine(SendUdpMessage(jsonData));

        // 延迟0.5秒发送第二条消息
        StartCoroutine(SendDelayedMessage());
    }

    IEnumerator SendUdpMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        // 发送消息到服务器
        udpClient.Send(data, data.Length, serverIp, serverPort);

        yield return null;
    }

    IEnumerator SendDelayedMessage()
    {
        // 延迟0.5秒
        yield return new WaitForSeconds(0.5f);

        string startCommand = "{\"command\":\"start\"}";

        // 发送start命令
        StartCoroutine(SendUdpMessage(startCommand));
    }

    void OnExitButtonClicked()
    {
        targetPosition = originalCameraPosition;
        targetRotation = originalCameraRotation;

        isMovingBack = true;

        objectToDisable.SetActive(true);
        objectToDisableTwo.SetActive(true);
        uiPanel.SetActive(false);

        Time.timeScale = 1;
        isPaused = false;
    }

    void OnApplicationQuit()
    {
        udpClient.Close(); // 关闭UDP客户端
    }
}
