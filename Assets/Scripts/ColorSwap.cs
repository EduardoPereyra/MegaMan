using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * Comes from this article with some minor changes by me
 * https://gamedevelopment.tutsplus.com/tutorials/how-to-use-a-shader-to-dynamically-swap-a-sprites-colors--cms-25129
 */

public class ColorSwap : MonoBehaviour
{
    [Header("Shader Properties")]
    [SerializeField] private string primaryColorRef = "_PrimaryColor";
    [SerializeField] private string secondaryColorRef = "_SecondaryColor";
    [SerializeField] private string newPrimaryColorRef = "_NewPrimaryColor";
    [SerializeField] private string newSecondaryColorRef = "_NewSecondaryColor";
    [SerializeField] private string mainTexRef = "_MainTex";
    
    private Renderer objectRenderer;
    private Material materialInstance;
    
    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (materialInstance != null) return;

        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogError($"No Renderer found on {gameObject.name}!");
            return;
        }

        materialInstance = objectRenderer.material;
    }
    
    // Public method to change both colors at once
    public void SwapColors(int primary, int secondary, Color newPrimary, Color newSecondary)
    {
        if (materialInstance == null) return;
        
        materialInstance.SetColor(primaryColorRef, ParseColorFromOnlyInt(primary));
        materialInstance.SetColor(secondaryColorRef, ParseColorFromOnlyInt(secondary));
        materialInstance.SetColor(newPrimaryColorRef, newPrimary);
        materialInstance.SetColor(newSecondaryColorRef, newSecondary);
    }
    
    // Individual color change methods
    public void SetPrimaryColor(int primary,Color newColor)
    {
        materialInstance?.SetColor(primaryColorRef, ParseColorFromOnlyInt(primary));
        materialInstance?.SetColor(newPrimaryColorRef, newColor);
    }
    
    public void SetSecondaryColor(int secondary,Color newColor)
    {
        materialInstance?.SetColor(secondaryColorRef, ParseColorFromOnlyInt(secondary));
        materialInstance?.SetColor(newSecondaryColorRef, newColor);
    }

    public void SetMainSprite(Sprite sprite)
    {
        if (materialInstance == null || sprite == null) return;
        materialInstance.SetTexture(mainTexRef, sprite.texture);
    }

    public static Color ColorFromInt(int c, float alpha = 1.0f)
    {
        int r = (c >> 16) & 0x000000FF;
        int g = (c >> 8) & 0x000000FF;
        int b = c & 0x000000FF;

        Color ret = ColorFromIntRGB(r, g, b);
        ret.a = alpha;

        return ret;
    }

    public static Color ColorFromIntRGB(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
    }

    public static Color ParseColorFromOnlyInt(int c)
    {
        return new Color(c / 255f, c / 255f, c / 255f);
    }
}
