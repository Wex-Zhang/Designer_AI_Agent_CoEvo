using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class InputFieldManager : MonoBehaviour
{
    public InputField inputField; // 引用你的InputField
    private string jsonFilePath;
    private Dictionary<string, object> jsonData; // 用于存储整个JSON对象
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
            string jsonContent = File.ReadAllText(jsonFilePath);
            jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);

            // 读取 "original_topic" 并将其转换为大写
            if (jsonData.ContainsKey("original_topic"))
            {
                previousContent = jsonData["original_topic"].ToString().ToUpper();
                inputField.text = spaces + previousContent;
                isInitialized = true;
            }
            else
            {
                Debug.LogError("The key 'original_topic' was not found in the JSON file.");
            }
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
        // 更新JSON中的 "original_topic"
        if (jsonData != null && jsonData.ContainsKey("original_topic"))
        {
            // 将输入内容转换为小写并写入JSON文件
            string contentToWrite = newContent.ToLower();
            jsonData["original_topic"] = contentToWrite;

            // 将更新后的JSON写回文件，不会覆盖其他字段
            string updatedJson = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            File.WriteAllText(jsonFilePath, updatedJson);

            // 更新previousContent为当前内容（保持大写显示）
            previousContent = newContent;
        }
    }
}
