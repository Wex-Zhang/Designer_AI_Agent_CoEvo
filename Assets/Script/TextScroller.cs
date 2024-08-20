using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;

public class TextScroller : MonoBehaviour
{
    public string jsonFilePath = "../AgentVisData/vis_data.json";
    public GameObject contentPanel;
    public GameObject textPrefab; // Assign this in the Inspector, it should be the TextContent object.
    public ScrollRect scrollRect; // Assign the ScrollRect component here in the Inspector.

    private List<string> proposals = new List<string>();
    private int currentProposalIndex = 0;
    private int wordsPerUpdate = 10;

    void Start()
    {
        StartCoroutine(LoadProposals());
    }

    IEnumerator LoadProposals()
    {
        // Load the JSON file
        string path = Path.Combine(Application.dataPath, jsonFilePath);
        string jsonData = File.ReadAllText(path);
        JObject data = JObject.Parse(jsonData);
        
        // Extract proposals
        foreach (var round in data["runtime"])
        {
            foreach (var proposal in round["proposals"])
            {
                string proposalText = proposal["proposal"].ToString();
                proposals.Add(proposalText);
            }
        }
        
        // Start updating the text content every 5 seconds
        StartCoroutine(UpdateTextContent());
        
        yield return null;
    }

    IEnumerator UpdateTextContent()
    {
        while (currentProposalIndex < proposals.Count)
        {
            // Fetch the next portion of text
            string[] words = proposals[currentProposalIndex].Split(' ');
            int endIndex = Mathf.Min(words.Length, wordsPerUpdate);

            string displayedText = string.Join(" ", words, 0, endIndex);

            // Create a new Text object in the Scroll View
            GameObject newText = Instantiate(textPrefab, contentPanel.transform);
            newText.GetComponent<Text>().text = displayedText;

            // Force the ScrollRect to scroll to the bottom
            Canvas.ForceUpdateCanvases(); // Force an update to the layout before adjusting the scroll position
            scrollRect.verticalNormalizedPosition = 0f; // 0 means bottom, 1 means top

            // Update the proposal index
            currentProposalIndex++;
            yield return new WaitForSeconds(5f);
        }
    }
}

