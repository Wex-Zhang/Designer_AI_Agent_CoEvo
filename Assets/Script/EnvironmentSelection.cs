using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.Networking;  // 用于发送HTTP请求
using System.Collections;

public class EnvironmentSelection : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public VideoPlayer videoPlayer1;
    public VideoPlayer videoPlayer2;
    public GameObject gameObject1;
    public GameObject gameObject2;
    public Button nextButton;
    private Button selectedButton;

    void Start()
    {
        // 准备视频以显示第一帧
        PrepareVideo(videoPlayer1);
        PrepareVideo(videoPlayer2);

        // 添加悬停和移出事件
        AddHoverEvent(button1, videoPlayer1);
        AddHoverEvent(button2, videoPlayer2);

        // 添加点击事件
        button1.onClick.AddListener(() => SelectButton(button1, videoPlayer1));
        button2.onClick.AddListener(() => SelectButton(button2, videoPlayer2));

        // Next按钮事件
        nextButton.onClick.AddListener(OnNextClicked);
    }

    void AddHoverEvent(Button button, VideoPlayer videoPlayer)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // 悬停事件：播放视频
        EventTrigger.Entry entryHover = new EventTrigger.Entry();
        entryHover.eventID = EventTriggerType.PointerEnter;
        entryHover.callback.AddListener((eventData) => PlayVideo(videoPlayer));
        trigger.triggers.Add(entryHover);

        // 鼠标移出事件：停止视频
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) =>
        {
            // 如果当前按钮没有被选中，移出后停止视频
            if (selectedButton != button)
            {
                StopVideo(videoPlayer);
            }
        });
        trigger.triggers.Add(entryExit);
    }

    // 准备视频并显示第一帧
    void PrepareVideo(VideoPlayer videoPlayer)
    {
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepareCompleted;
    }

    // 当视频准备完成时，显示第一帧
    void OnPrepareCompleted(VideoPlayer videoPlayer)
    {
        videoPlayer.Pause();  // 不播放视频，但显示第一帧
    }

    void PlayVideo(VideoPlayer videoPlayer)
    {
        videoPlayer.Play();
    }

    void StopVideo(VideoPlayer videoPlayer)
    {
        videoPlayer.Pause();  // 停止视频并显示当前帧
        videoPlayer.frame = 0; // 回到第一帧
    }

    void SelectButton(Button button, VideoPlayer videoPlayer)
    {
        // 清除之前的白框和视频播放
        if (selectedButton != null && selectedButton != button)
        {
            DeselectButton(selectedButton);
            StopVideo(selectedButton == button1 ? videoPlayer1 : videoPlayer2);
        }

        // 选中当前按钮并显示白框
        selectedButton = button;
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }

        // 设置白框的颜色为白色，并将alpha值设置为0.1（10%不透明度）
        outline.effectColor = new Color(1, 1, 1, 0.01f);  // 1,1,1表示白色，0.1f表示10%不透明度

        // 选中的按钮视频继续播放
        PlayVideo(videoPlayer);
    }

    void DeselectButton(Button button)
    {
        // 移除白框
        Outline outline = button.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }

    void OnNextClicked()
    {
        string jsonData = "";

        if (selectedButton == button1)
        {
            // 选中第一个按钮
            gameObject1.SetActive(true);
            gameObject2.SetActive(false);

            // 设置 JSON 数据
            jsonData = "{\"environment\": \"panopticon\"}";
        }
        else if (selectedButton == button2)
        {
            // 选中第二个按钮
            gameObject1.SetActive(false);
            gameObject2.SetActive(true);

            // 设置 JSON 数据
            jsonData = "{\"environment\": \"park with green land\"}";
        }

        // 启动协程发送POST请求
        StartCoroutine(SendPostRequest("http://localhost:3000/api/receive-data", jsonData));
    }

    // 使用 UnityWebRequest 发送 POST 请求
    IEnumerator SendPostRequest(string url, string jsonData)
    {
        // 创建 UnityWebRequest 对象，指定POST请求和要发送的数据
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 发送请求并等待响应
        yield return request.SendWebRequest();

        // 检查请求是否成功
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error sending POST request: " + request.error);
        }
        else
        {
            Debug.Log("POST request sent successfully! Response: " + request.downloadHandler.text);
        }
    }
}
