using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// 数据模型类，用于存储从JSON文件读取的数据
[System.Serializable]
public class CharacterData1
{
    public string name;
    public int age;
    public List<string> traits;
    public string status;
    public List<string> initial_memory;
}

public class agentsDataDisplayAndInteraction1 : MonoBehaviour
{
    public string filePath = "Assets/Agents_Data/alexData.json";
    public CharacterData characterData;

    public GameObject uiPanel; // 拖放UI面板
    public Text nameText;
    public Text ageText;
    public Text traitsText;
    public Text statusText;
    public InputField memoryInputField;
    public Button saveButton;
    public Vector3 offset; // 相对于物体的偏移量


    void Start()
    {
        uiPanel.SetActive(false); // 初始化时隐藏UI面板
        ReadJson();
        saveButton.onClick.AddListener(SaveChanges);
    }

    void Update()
    {
        // 检测鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    ShowCharacterInfo();
                }
            }
        }

        // 使UI面板跟随人物
        if (uiPanel.activeSelf)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            uiPanel.transform.position = screenPos + new Vector3(100, 0, 0) + offset; // 偏移位置，使其在人物右侧
        }
    }

    void ReadJson()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            characterData = JsonUtility.FromJson<CharacterData>(json);
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
    }

    void ShowCharacterInfo()
    {
        uiPanel.SetActive(true);
        nameText.text = "Name: Lucy";
        ageText.text = "Age: 30";
        traitsText.text = "Traits: Innovative, Critical";
        statusText.text = "Status: Working on a design project";
        memoryInputField.text = string.Join("\n", characterData.initial_memory);
    }

    public void OnMemoryInputFieldChanged()
    {
        characterData.initial_memory = new List<string>(memoryInputField.text.Split('\n'));
    }

    public void SaveChanges()
    {
        string json = JsonUtility.ToJson(characterData, true);
        File.WriteAllText(filePath, json);
    }
}
