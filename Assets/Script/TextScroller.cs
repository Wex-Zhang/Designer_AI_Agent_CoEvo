using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using Newtonsoft.Json.Linq;

public class TextScroller : MonoBehaviour
{
    public string jsonFilePath = "../AgentVisData/vis_data.json";
    public GameObject contentPanel;
    public GameObject textPrefab;
    public ScrollRect scrollRect;
    public GameObject viewPortController;
    public InputField inputField;
    public Text placeholderText;
    public Button focusButton;
    public GameObject uiPanel;

    private List<string> proposals = new List<string>();
    private int currentProposalIndex = 0;
    private bool isLoaded = false;
    private int clickCount = 0;

    // Define the UDP server address and port
    private string udpServerIp = "127.0.0.1"; // Replace with your UDP server's IP
    private int udpServerPort = 3000; // Replace with your UDP server's port

    private UdpClient udpClient;

    void Start()
    {
        // Initialize UDP client
        udpClient = new UdpClient();

        placeholderText.text = "Make Proposal_>";
        placeholderText.fontSize = 80;

        if (viewPortController != null)
        {
            viewPortController.AddComponent<SphereCollider>().isTrigger = true;
        }

        scrollRect.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }

        inputField.onEndEdit.AddListener(HandleUserInput);
        focusButton.onClick.AddListener(OnFocusButtonClick);
        AddEventTriggerListener(inputField, EventTriggerType.Select, OnInputFieldSelected);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == viewPortController)
                {
                    clickCount++;
                    ToggleScrollRectVisibility();
                }
            }
        }
    }

    void OnInputFieldSelected(BaseEventData eventData)
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }
        Time.timeScale = 0.01f;
    }

    void HandleUserInput(string inputText)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                AddTextToScrollRect(inputText);

                // Create JSON data
                JObject jsonData = new JObject();
                jsonData["command"] = "chat_in_round";
                jsonData["content"] = inputText;

                // 启动协程发送 UDP 请求并延迟 0.5 秒后发送第二条命令
                StartCoroutine(SendUdpRequest(jsonData.ToString()));

                inputField.text = "";
                placeholderText.text = "Make Proposal_>";
                inputField.placeholder.GetComponent<Text>().fontSize = 80;

                ScrollToBottom();

                if (uiPanel != null)
                {
                    uiPanel.SetActive(false);
                }
                Time.timeScale = 1f;
            }
        }
    }

    void AddTextToScrollRect(string newText)
    {
        GameObject newTextObject = Instantiate(textPrefab, contentPanel.transform);
        newTextObject.GetComponent<Text>().text = newText;

        GameObject blankLine1 = Instantiate(textPrefab, contentPanel.transform);
        blankLine1.GetComponent<Text>().text = "";

        GameObject blankLine2 = Instantiate(textPrefab, contentPanel.transform);
        blankLine2.GetComponent<Text>().text = "";
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void ToggleScrollRectVisibility()
    {
        if (clickCount % 2 == 1)
        {
            scrollRect.gameObject.SetActive(true);
            inputField.gameObject.SetActive(true);
        }
        else
        {
            scrollRect.gameObject.SetActive(false);
            inputField.gameObject.SetActive(false);
        }
    }

    void OnFocusButtonClick()
    {
        if (!isLoaded)
        {
            StartCoroutine(DelayedLoadProposals());
            isLoaded = true;
        }
    }

    IEnumerator DelayedLoadProposals()
    {
        yield return new WaitForSeconds(5f);
        StartCoroutine(LoadProposals());
    }

    IEnumerator LoadProposals()
    {
        string path = Path.Combine(Application.dataPath, jsonFilePath);
        string jsonData = File.ReadAllText(path);
        JObject data = JObject.Parse(jsonData);

        foreach (var round in data["runtime"])
        {
            foreach (var proposal in round["proposals"])
            {
                string proposalText = proposal["proposal"].ToString();
                proposals.Add(proposalText);
            }
        }

        StartCoroutine(UpdateTextContent());

        yield return null;
    }

    IEnumerator UpdateTextContent()
    {
        yield return new WaitForSeconds(10f);

        while (currentProposalIndex < proposals.Count)
        {
            string currentProposal = proposals[currentProposalIndex];
            AddTextToScrollRect(currentProposal);
            ScrollToBottom();
            yield return new WaitForSeconds(10f);
            currentProposalIndex++;
        }
    }

    // 修改后的 UDP 请求发送协程
    IEnumerator SendUdpRequest(string jsonData)
    {
        // 发送第一个 JSON 数据
        bool success = SendUdpMessage(jsonData);
        if (success)
        {
            Debug.Log("UDP message sent: " + jsonData);
        }
        else
        {
            Debug.LogError("Failed to send UDP message: " + jsonData);
        }

        // 等待 0.5 秒
        yield return new WaitForSeconds(0.5f);

        // 构建并发送第二个 JSON 数据 {"command":"start"}
        string startCommand = "{\"command\":\"start\"}";
        success = SendUdpMessage(startCommand);
        if (success)
        {
            Debug.Log("UDP message sent: " + startCommand);
        }
        else
        {
            Debug.LogError("Failed to send UDP message: " + startCommand);
        }
    }

    // 不使用 yield 的函数来处理 UDP 消息发送
    bool SendUdpMessage(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, udpServerIp, udpServerPort);
            return true; // 表示发送成功
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending UDP message: " + ex.Message);
            return false; // 表示发送失败
        }
    }

    private void AddEventTriggerListener(InputField input, EventTriggerType eventType, System.Action<BaseEventData> callback)
    {
        EventTrigger trigger = input.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = input.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => { callback.Invoke(eventData); });
        trigger.triggers.Add(entry);
    }

    void OnDestroy()
    {
        // Close UDP client when the object is destroyed
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
