using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Text buttonText;
    private float timer;
    public float blinkSpeed = 1.0f; // 每秒闪烁一次
    private bool isHovered = false;

    void Start()
    {
        buttonText = GetComponent<Text>();
        if (buttonText == null)
        {
            Debug.LogError("No Text component found on the button.");
        }
        timer = 0.0f;
    }

    void Update()
    {
        if (isHovered && buttonText != null)
        {
            timer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.Abs(Mathf.Cos(timer * Mathf.PI));
            Color color = buttonText.color;
            color.a = alpha;
            buttonText.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (buttonText != null)
        {
            Color color = buttonText.color;
            color.a = 1f; // 恢复透明度为100%
            buttonText.color = color;
        }
        timer = 0.0f; // 重置计时器
    }
}
