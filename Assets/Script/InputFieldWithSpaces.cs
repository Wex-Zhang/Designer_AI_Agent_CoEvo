using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InputFieldManager : MonoBehaviour
{
    public InputField inputField; // 引用你的InputField
    private string jsonFilePath;
    private TopicData topicData;
    private string previousContent;
    private string spaces = "                              "; // 固定空格
    private bool isInitialized = false; // 标记是否已经初始化过空格

    void Start()
    {
        // 定义JSON文件的路径（在Assets文件夹中）
        jsonFilePath = Path.Combine(Application.dataPath, "../AgentVisData/vis_data.json");

        // 读取并解析JSON文件
        LoadJson();

        // 监听InputField内容变化
        inputField.onEndEdit.AddListener(OnEndEdit);
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void LoadJson()
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonData = File.ReadAllText(jsonFilePath);
            topicData = JsonUtility.FromJson<TopicData>(jsonData);

            // 将读取到的内容设置到InputField中，添加空格并转换为大写
            previousContent = topicData.original_topic.ToUpper();
            inputField.text = spaces + previousContent;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("JSON file not found at: " + jsonFilePath);
        }
    }

    void OnInputValueChanged(string text)
    {
        if (isInitialized)
        {
            string currentInput;
            if (text.StartsWith(spaces))
            {
                currentInput = text.Substring(spaces.Length).ToUpper(); // 获取用户实际输入的内容并转换为大写
            }
            else
            {
                currentInput = text.ToUpper();
            }

            inputField.text = spaces + currentInput; // 更新输入字段的内容
            inputField.caretPosition = inputField.text.Length; // 将光标移到文本末尾
        }
    }

    void OnEndEdit(string newContent)
    {
        // 去掉前面的空格并将内容转换为大写
        newContent = newContent.Substring(spaces.Length).ToUpper();

        // 仅当内容发生变化时才更新JSON
        if (newContent != previousContent)
        {
            UpdateJson(newContent);
        }
    }

    void UpdateJson(string newContent)
    {
        // 更新JSON对象中的内容
        topicData.original_topic = newContent;

        // 将更新后的内容写回JSON文件
        string updatedJson = JsonUtility.ToJson(topicData, true);
        File.WriteAllText(jsonFilePath, updatedJson);

        // 更新previousContent为当前内容
        previousContent = newContent;
    }
}

[System.Serializable]
public class TopicData
{
    public string original_topic;
}