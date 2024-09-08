using UnityEngine;
using UnityEngine.Video;

public class SceneSwitcher : MonoBehaviour
{
    public GameObject firstScene; // 场景1的父对象
    public GameObject secondScene; // 场景2的父对象
    public GameObject videoPlayerObject; // 包含 VideoPlayer 的对象
    private VideoPlayer videoPlayer; // 视频播放组件

    private void Start()
    {
        // 确保第二个场景和视频播放器在开始时隐藏
        secondScene.SetActive(false);
        videoPlayer = videoPlayerObject.GetComponent<VideoPlayer>();
        videoPlayerObject.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd; // 视频播放结束时调用方法
        }
        else
        {
            Debug.LogError("VideoPlayer component is not assigned or found on the videoPlayerObject.");
        }
    }

    public void OnButtonClick()
    {
        Debug.Log("Button clicked! Starting process...");

        // 隐藏第一个场景并播放视频
        firstScene.SetActive(false);
        PlayVideo();
    }

    private void PlayVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayerObject.SetActive(true); // 显示视频播放器
            videoPlayer.Play(); // 开始播放视频
            Debug.Log("Video started playing...");
        }
        else
        {
            Debug.LogWarning("VideoPlayer component is not assigned.");
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video ended. Switching to second scene...");

        // 播放完成后，隐藏视频播放器并显示第二个场景
        videoPlayerObject.SetActive(false);
        secondScene.SetActive(true);
    }
}

