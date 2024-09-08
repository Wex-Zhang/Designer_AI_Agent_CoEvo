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
    public GameObject viewPortController; // Assign the sphere (viewPortController) here in the Inspector.

    private List<string> proposals = new List<string>();
    private int currentProposalIndex = 0;
    private int wordsPerUpdate = 10;
    private bool isLoaded = false; // To ensure proposals are loaded only once
    private int clickCount = 0; // Track the number of clicks

    void Start()
    {
        // Ensure viewPortController is not null
        if (viewPortController != null)
        {
            // Add an event listener for the mouse click on the viewPortController
            viewPortController.AddComponent<SphereCollider>().isTrigger = true; // Ensure it has a collider
        }
    }

    void Update()
    {
        // Check if the viewPortController is clicked
        if (Input.GetMouseButtonDown(0)) // 0 is for left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is the viewPortController
                if (hit.collider.gameObject == viewPortController)
                {
                    clickCount++; // Increment click count
                    ToggleScrollRectVisibility(); // Toggle visibility based on click count

                    if (!isLoaded)
                    {
                        StartCoroutine(LoadProposals());
                        isLoaded = true; // Ensure it only loads once
                    }
                }
            }
        }
    }

    void ToggleScrollRectVisibility()
    {
        // Show or hide ScrollRect based on the number of clicks
        if (clickCount % 2 == 1) // Odd number of clicks
        {
            scrollRect.gameObject.SetActive(true);
        }
        else // Even number of clicks
        {
            scrollRect.gameObject.SetActive(false);
        }
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

        // Start updating the text content: first line after 3 seconds, then every 5 seconds
        StartCoroutine(UpdateTextContent());

        yield return null;
    }

    IEnumerator UpdateTextContent()
    {
        // Wait for 3 seconds before showing the first line
        yield return new WaitForSeconds(3f);

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

            // Wait 5 seconds before displaying the next text
            yield return new WaitForSeconds(5f);
        }
    }
}
