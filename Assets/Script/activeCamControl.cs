using UnityEngine;
using UnityEngine.UI;

public class ActivateScriptButton : MonoBehaviour
{
    public Button yourButton; // 按钮
    public MonoBehaviour scriptToActivate; // 需要激活的脚本

    void Start()
    {
        // 确保按钮和脚本已经被赋值
        if (yourButton != null && scriptToActivate != null)
        {
            // 给按钮添加点击事件监听器
            yourButton.onClick.AddListener(ActivateScript);
        }
    }

    void ActivateScript()
    {
        // 激活脚本
        scriptToActivate.enabled = true;
    }
}
