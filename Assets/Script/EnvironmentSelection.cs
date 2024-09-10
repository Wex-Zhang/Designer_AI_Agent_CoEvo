using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;

public class EnvironmentSelection : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public VideoPlayer videoPlayer1;
    public VideoPlayer videoPlayer2;
    public GameObject gameObject1;
    public GameObject gameObject2;
    public Image image1; // 第一个 Image
    public Image image2; // 第二个 Image
    public Button nextButton;
    private Button selectedButton;
    private Coroutine image1FadeCoroutine;
    private Coroutine image2FadeCoroutine;

    void Start()
    {
        // 准备视频以显示第一帧
        PrepareVideo(videoPlayer1);
        PrepareVideo(videoPlayer2);

        // 添加悬停和移出事件
        AddHoverEvent(button1, videoPlayer1, image1);
        AddHoverEvent(button2, videoPlayer2, image2);

        // 添加点击事件
        button1.onClick.AddListener(() => SelectButton(button1, videoPlayer1, image1));
        button2.onClick.AddListener(() => SelectButton(button2, videoPlayer2, image2));

        // Next按钮事件
        nextButton.onClick.AddListener(OnNextClicked);

        // 初始化 image 的透明度
        SetImageAlpha(image1, 0.4f);
        SetImageAlpha(image2, 0.4f);
    }

    void AddHoverEvent(Button button, VideoPlayer videoPlayer, Image image)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // 悬停事件：播放视频和淡出图片
        EventTrigger.Entry entryHover = new EventTrigger.Entry();
        entryHover.eventID = EventTriggerType.PointerEnter;
        entryHover.callback.AddListener((eventData) =>
        {
            PlayVideo(videoPlayer);
            if (selectedButton != button)
            {
                if (image == image1 && image1FadeCoroutine != null) StopCoroutine(image1FadeCoroutine);
                if (image == image2 && image2FadeCoroutine != null) StopCoroutine(image2FadeCoroutine);
                StartCoroutine(FadeImage(image, 0f, 0.5f));
            }
        });
        trigger.triggers.Add(entryHover);

        // 鼠标移出事件：停止视频和恢复图片透明度
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) =>
        {
            if (selectedButton != button)
            {
                StopVideo(videoPlayer);
                if (image == image1 && image1FadeCoroutine != null) StopCoroutine(image1FadeCoroutine);
                if (image == image2 && image2FadeCoroutine != null) StopCoroutine(image2FadeCoroutine);
                StartCoroutine(FadeImage(image, 0.4f, 0.5f));
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

    void SelectButton(Button button, VideoPlayer videoPlayer, Image image)
    {
        // 清除之前的白框和视频播放
        if (selectedButton != null && selectedButton != button)
        {
            DeselectButton(selectedButton);
            StopVideo(selectedButton == button1 ? videoPlayer1 : videoPlayer2);
            if (selectedButton == button1)
                image1FadeCoroutine = StartCoroutine(FadeImage(image1, 0.4f, 0.5f));
            else if (selectedButton == button2)
                image2FadeCoroutine = StartCoroutine(FadeImage(image2, 0.4f, 0.5f));
        }

        // 选中当前按钮并显示白框
        selectedButton = button;
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(1, 1, 1, 0.01f);  // 白框颜色

        // 选中的按钮视频继续播放
        PlayVideo(videoPlayer);

        // 选中时图像透明度保持为0
        StartCoroutine(FadeImage(image, 0f, 0.5f));
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
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error sending POST request: " + request.error);
        }
        else
        {
            Debug.Log("POST request sent successfully! Response: " + request.downloadHandler.text);
        }
    }

    // 设置 Image 的透明度
    void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    // 淡入淡出效果的协程
    IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        float startAlpha = image.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            SetImageAlpha(image, alpha);
            yield return null;
        }

        SetImageAlpha(image, targetAlpha);
    }
}
