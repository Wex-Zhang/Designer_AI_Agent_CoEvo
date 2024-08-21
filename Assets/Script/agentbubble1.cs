using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogueData
{
    public string dialogue;
}

public class agentbubble1 : MonoBehaviour
{
    public List<string> filePaths; // JSON 文件路径列表，每个文件对应一个角色
    public List<DialogueData> dialogueDataList; // 对应每个角色的对话数据
    public List<GameObject> dialogueBubbles; // 对应每个角色的气泡 UI
    public List<Transform> objectsToTrack; // 每个气泡要追踪的物体
    public Vector3 offset; // 微调追踪点的偏移向量

    private void Start()
    {
        dialogueDataList = new List<DialogueData>();
        foreach (string filePath in filePaths)
        {
            ReadJson(filePath);
        }
        HideAllBubbles();
    }

    private void ReadJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            DialogueData dialogueData = JsonUtility.FromJson<DialogueData>(json);
            dialogueDataList.Add(dialogueData);
        }
        else
        {
            Debug.LogError($"找不到文件: {filePath}");
        }
    }

    private void HideAllBubbles()
    {
        foreach (GameObject bubble in dialogueBubbles)
        {
            bubble.SetActive(false);
        }
    }

    private void ShowBubble(int index)
    {
        if (index < dialogueBubbles.Count && index < dialogueDataList.Count)
        {
            dialogueBubbles[index].SetActive(true);
            dialogueBubbles[index].GetComponentInChildren<Text>().text = dialogueDataList[index].dialogue;
            StartCoroutine(TrackObject(index));
        }
    }

    private IEnumerator TrackObject(int index)
    {
        while (dialogueBubbles[index].activeSelf)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(objectsToTrack[index].position);
            dialogueBubbles[index].transform.position = screenPos + offset;
            yield return null;
        }
    }

    private void OnMouseDown()
    {
        StartCoroutine(ShowDialogueSequence());
    }

    private IEnumerator ShowDialogueSequence()
    {
        for (int i = 0; i < dialogueBubbles.Count; i++)
        {
            yield return new WaitForSeconds(i == 0 ? 3f : 5f); // 第一个气泡2秒后显示，后续气泡每隔3秒显示
            ShowBubble(i);
        }

        yield return new WaitForSeconds(55f); // 第一个气泡出现55秒后，隐藏所有气泡
        HideAllBubbles();
    }
}
