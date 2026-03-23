using UnityEngine;
using UnityEngine.UI;

public class UiHealthBar: MonoBehaviour
{

    public Image mask;
    float originalSize;

    public static UiHealthBar Instance {get; private set;}
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalSize = mask.rectTransform.rect.height;
    }

    public void SetHealth(float currentHealth)
    {
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * currentHealth);
    }
}
     