using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class UppercaseInputField : MonoBehaviour
{
    private InputField inputField;

    void Awake()
    {
        inputField = GetComponent<InputField>();
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void OnDestroy()
    {
        inputField.onValueChanged.RemoveListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string input)
    {
        inputField.text = input.ToUpper();
    }
}
