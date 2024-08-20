using UnityEngine;
using UnityEngine.UI;

public class ChangeAgentMemWithSpaces : MonoBehaviour
{
    public InputField inputField;
    private string spaces = "                              "; // 定义固定空格
    private string currentInput = ""; // 存储当前输入的内容
    private bool isInitialized = false; // 标记是否已经初始化过空格

    void Start()
    {
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void OnInputValueChanged(string text)
    {
        if (!isInitialized)
        {
            // 用户第一次输入时，添加空格并标记初始化完成
            isInitialized = true;
            currentInput = text.ToUpper(); // 将输入的文本转换为大写
            inputField.text = spaces + currentInput;
            inputField.caretPosition = spaces.Length + currentInput.Length; // 将光标移到用户输入的文本末尾
        }
        else
        {
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
}