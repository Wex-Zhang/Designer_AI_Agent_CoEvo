using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BlinkCallToAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject objectB; // 用于引用对象B
    private Text objectBText;
    private float timer;
    public float blinkSpeed = 1.0f; // 每秒闪烁一次
    public float minAlpha = 0.2f; // 最小透明度
    public float maxAlpha = 0.7f; // 最大透明度
    private bool isHovered = false;

    void Start()
    {
        if (objectB != null)
        {
            objectBText = objectB.GetComponent<Text>();
            if (objectBText == null)
            {
                Debug.LogError("No Text component found on the objectB.");
            }
        }
        else
        {
            Debug.LogError("objectB is not assigned.");
        }
        timer = 0.0f;
    }

    void Update()
    {
        if (isHovered && objectBText != null)
        {
            timer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.Abs(Mathf.Cos(timer * Mathf.PI)));
            Color color = objectBText.color;
            color.a = alpha;
            objectBText.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (objectBText != null)
        {
            Color color = objectBText.color;
            color.a = maxAlpha; // 恢复透明度为100%
            objectBText.color = color;
        }
        timer = 0.0f; // 重置计时器
    }
}

