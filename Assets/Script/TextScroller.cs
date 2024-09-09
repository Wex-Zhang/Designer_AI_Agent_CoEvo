using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking; // UnityWebRequest 需要引用的命名空间
using System.IO;
using Newtonsoft.Json.Linq;

public class TextScroller : MonoBehaviour
{
    public string jsonFilePath = "../AgentVisData/vis_data.json";
    public GameObject contentPanel;
    public GameObject textPrefab; // Assign this in the Inspector, it should be the TextContent object.
    public ScrollRect scrollRect; // Assign the ScrollRect component here in the Inspector.
    public GameObject viewPortController; // Assign the sphere (viewPortController) here in the Inspector.
    public InputField inputField; // Assign the InputField here in the Inspector.
    public Text placeholderText; // Assign the placeholder text here in the Inspector.
    public Button focusButton; // Assign the Focus Button here in the Inspector.

    private List<string> proposals = new List<string>();
    private int currentProposalIndex = 0;
    private bool isLoaded = false; // To ensure proposals are loaded only once
    private int clickCount = 0; // Track the number of clicks

    void Start()
    {
        // Set initial placeholder text and font size
        placeholderText.text = "Make Proposal_>";
        placeholderText.fontSize = 80;

        // Ensure viewPortController is not null
        if (viewPortController != null)
        {
            // Add an event listener for the mouse click on the viewPortController
            viewPortController.AddComponent<SphereCollider>().isTrigger = true; // Ensure it has a collider
        }

        // Initially hide ScrollRect and InputField
        scrollRect.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);

        // Add an event listener for the InputField to handle Enter key submission
        inputField.onEndEdit.AddListener(HandleUserInput);

        // Add a listener for the Focus Button to start loading proposals
        focusButton.onClick.AddListener(OnFocusButtonClick);
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
                }
            }
        }
    }

    void HandleUserInput(string inputText)
    {
        // Handle the user's input when they press Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                // Add the user's input to the scroll rect content
                AddTextToScrollRect(inputText);

                // Create the JSON data for the HTTP request
                JObject jsonData = new JObject();
                jsonData["humanProposal"] = inputText;

                // Start the POST request coroutine
                StartCoroutine(SendPostRequest("http://localhost:3000/api/receive-data", jsonData.ToString()));

                // Clear the input field and reset the placeholder
                inputField.text = "";
                placeholderText.text = "Make Proposal_>";
                inputField.placeholder.GetComponent<Text>().fontSize = 80;

                // Ensure the ScrollRect scrolls to the bottom
                ScrollToBottom();
            }
        }
    }

    void AddTextToScrollRect(string newText)
    {
        // Create a new Text object in the Scroll View for the user input
        GameObject newTextObject = Instantiate(textPrefab, contentPanel.transform);
        newTextObject.GetComponent<Text>().text = newText;

        // Add blank lines after the new text
        GameObject blankLine1 = Instantiate(textPrefab, contentPanel.transform);
        blankLine1.GetComponent<Text>().text = "";

        GameObject blankLine2 = Instantiate(textPrefab, contentPanel.transform);
        blankLine2.GetComponent<Text>().text = "";
    }

    void ScrollToBottom()
    {
        // Force the ScrollRect to update its layout before scrolling
        Canvas.ForceUpdateCanvases();

        // Set scroll position to bottom (0f = bottom, 1f = top)
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void ToggleScrollRectVisibility()
    {
        // Show or hide ScrollRect and InputField based on the number of clicks
        if (clickCount % 2 == 1) // Odd number of clicks
        {
            scrollRect.gameObject.SetActive(true);
            inputField.gameObject.SetActive(true);
        }
        else // Even number of clicks
        {
            scrollRect.gameObject.SetActive(false);
            inputField.gameObject.SetActive(false);
        }
    }

    void OnFocusButtonClick()
    {
        // Check if the proposals have already been loaded
        if (!isLoaded)
        {
            StartCoroutine(DelayedLoadProposals()); // Start a coroutine that delays the loading process
            isLoaded = true; // Ensure proposals are only loaded once
        }
    }

    // Coroutine to delay the loading by 5 seconds
    IEnumerator DelayedLoadProposals()
    {
        // Wait for 5 seconds before starting to load proposals
        yield return new WaitForSeconds(5f);

        // Start loading proposals after the delay
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

        // Start updating the text content: first line after 1 second, then every 10 seconds
        StartCoroutine(UpdateTextContent());

        yield return null;
    }

    IEnumerator UpdateTextContent()
    {
        // Wait for 1 second before showing the first proposal
        yield return new WaitForSeconds(1f);

        while (currentProposalIndex < proposals.Count)
        {
            // Fetch the next proposal
            string currentProposal = proposals[currentProposalIndex];

            // Add the proposal to the ScrollRect
            AddTextToScrollRect(currentProposal);

            // Ensure the ScrollRect scrolls to the bottom after adding each proposal
            ScrollToBottom();

            // Wait 10 seconds before displaying the next proposal
            yield return new WaitForSeconds(10f);

            // Update the proposal index
            currentProposalIndex++;
        }
    }

    // Coroutine to send the HTTP POST request
    IEnumerator SendPostRequest(string url, string jsonData)
    {
        // Create a UnityWebRequest with the specified URL and JSON data
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        // Handle the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request sent successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error in sending request: " + request.error);
        }
    }
}
