using UnityEngine;
using UnityEngine.UI; // 用于 UI

public class AgentMoveAfterFocus : MonoBehaviour
{
    // 这三个是需要激活的GameObjects
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;

    // 按钮
    public Button activateButton;

    void Start()
    {
        // 确保这些对象在开始时是非激活的
        object1.SetActive(false);
        object2.SetActive(false);
        object3.SetActive(false);

        // 为按钮添加点击事件监听器
        activateButton.onClick.AddListener(OnButtonClick);
    }

    // 当按钮点击时调用
    void OnButtonClick()
    {
        // 设置所有对象为 active
        object1.SetActive(true);
        object2.SetActive(true);
        object3.SetActive(true);
    }
}
