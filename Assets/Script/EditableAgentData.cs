using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class AgentInfoHandler : MonoBehaviour
{
    public List<InputField> inputFields; // Drag your InputFields here in the Inspector
    private string jsonFilePath; // The path to your JSON file
    private JObject agentsData;
    public int agentIndex = 0; // The index of the agent you want to use, here it's the first agent (index 0)
    private Color textColor = new Color32(214, 212, 204, 255); // #D6D4CC

    void Start()
    {
        // Set the path to the JSON file
        jsonFilePath = Path.Combine(Application.dataPath, "../AgentVisData/vis_data.json");

        LoadJson();
        DisplayAgentInfo();
        AddInputFieldListeners();
        SetTextColor();
    }

    void LoadJson()
    {
        // Load JSON data from the file
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            agentsData = JObject.Parse(jsonContent);
        }
        else
        {
            Debug.LogError("JSON file not found at " + jsonFilePath);
        }
    }

    void DisplayAgentInfo()
    {
        if (agentsData != null)
        {
            // Assuming you have a specific mapping of InputFields to agent properties
            if (inputFields.Count > 0)
            {
                inputFields[0].text = agentsData["agents"][agentIndex]["name"].ToString().ToUpper();
                inputFields[1].text = agentsData["agents"][agentIndex]["age"].ToString().ToUpper();
                inputFields[2].text = agentsData["agents"][agentIndex]["traits"].ToString().ToUpper();
                inputFields[3].text = agentsData["agents"][agentIndex]["status"].ToString().ToUpper();

                // Handle initial_memory with extra lines between all but the last item
                JArray initialMemoryArray = (JArray)agentsData["agents"][agentIndex]["initial_memory"];
                string formattedMemory = "";
                for (int i = 0; i < initialMemoryArray.Count; i++)
                {
                    formattedMemory += initialMemoryArray[i].ToString().ToUpper();
                    if (i < initialMemoryArray.Count - 1)
                    {
                        formattedMemory += "\n\n"; // Add two line breaks after each sentence except the last one
                    }
                }

                // Assuming the corresponding InputField for initial memory is at index 4
                if (inputFields.Count > 4)
                {
                    inputFields[4].text = formattedMemory;
                }
            }
        }
    }

    void AddInputFieldListeners()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            int index = i; // Capture the current index for the listener
            inputFields[i].onEndEdit.AddListener((userInput) => OnInputFieldEdited(userInput, index));
        }
    }

    void OnInputFieldEdited(string userInput, int fieldIndex)
    {
        // Convert user input to uppercase for display
        string uppercasedInput = userInput.ToUpper();
        inputFields[fieldIndex].text = uppercasedInput;

        // Convert user input to lowercase for saving to JSON
        string lowercasedInput = userInput.ToLower();

        // Update the corresponding JSON field with the new input (lowercase)
        if (agentsData != null)
        {
            if (fieldIndex == 0)
            {
                agentsData["agents"][agentIndex]["name"] = lowercasedInput;
            }
            else if (fieldIndex == 1)
            {
                agentsData["agents"][agentIndex]["age"] = lowercasedInput;
            }
            else if (fieldIndex == 2)
            {
                agentsData["agents"][agentIndex]["traits"] = lowercasedInput;
            }
            else if (fieldIndex == 3)
            {
                agentsData["agents"][agentIndex]["status"] = lowercasedInput;
            }
            else if (fieldIndex == 4)
            {
                // Split the input back into the initial_memory array
                string[] splitInput = lowercasedInput.Split(new[] { "\n\n" }, System.StringSplitOptions.None);
                JArray updatedMemoryArray = new JArray();
                foreach (string memory in splitInput)
                {
                    updatedMemoryArray.Add(memory.Trim());
                }
                agentsData["agents"][agentIndex]["initial_memory"] = updatedMemoryArray;
            }

            // Save the updated JSON back to the file
            File.WriteAllText(jsonFilePath, agentsData.ToString());
        }
    }

    void SetTextColor()
    {
        foreach (InputField inputField in inputFields)
        {
            inputField.textComponent.color = textColor; // Set the color of the text
        }
    }
}
