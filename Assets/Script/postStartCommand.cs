using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class postStartCommand : MonoBehaviour
{
    // 按钮对象
    public Button sendPostButton;

    void Start()
    {
        // 为按钮添加点击事件监听器
        sendPostButton.onClick.AddListener(OnSendPostButtonClick);
    }

    // 按钮点击事件
    void OnSendPostButtonClick()
    {
        // 创建要发送的JSON数据
        string jsonData = "{\"command\":\"start\"}";

        // 启动协程发送POST请求
        StartCoroutine(SendPostRequest("http://localhost:3000/api/receive-data", jsonData));
    }

    // 协程发送POST请求
    IEnumerator SendPostRequest(string url, string jsonData)
    {
        // 将JSON数据转为字节数组
        byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);

        // 创建UnityWebRequest对象
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // 设置上传的数据
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);

        // 设置接收的数据
        request.downloadHandler = new DownloadHandlerBuffer();

        // 设置请求头中的内容类型为application/json
        request.SetRequestHeader("Content-Type", "application/json");

        // 发送请求并等待响应
        yield return request.SendWebRequest();

        // 检查请求是否有错误
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("请求失败: " + request.error);
        }
        else
        {
            Debug.Log("请求成功: " + request.downloadHandler.text);
        }
    }
}
