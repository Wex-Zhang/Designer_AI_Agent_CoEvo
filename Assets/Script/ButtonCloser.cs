using UnityEngine;
using UnityEngine.UI;

public class ButtonCloser : MonoBehaviour
{
    private Button button; // 引用按钮组件

    private void Start()
    {
        // 获取按钮组件
        button = GetComponent<Button>();

        // 为按钮添加点击事件监听
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button component not found on this GameObject.");
        }
    }

    private void OnButtonClick()
    {
        Debug.Log("Button clicked, now closing...");

        // 隐藏或禁用按钮
        button.gameObject.SetActive(false);
    }
}
