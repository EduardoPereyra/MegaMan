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
    
    [Header("Color Settings")]
    [SerializeField] private Color newPrimaryColor = Color.red;
    [SerializeField] private Color newSecondaryColor = Color.blue;
    
    private Renderer objectRenderer;
    private Material materialInstance;
    
    void Start()
    {
        // Get the renderer component
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer == null)
        {
            Debug.LogError("No Renderer found on this GameObject!");
            return;
        }
        
        // Create a material instance to avoid affecting the original asset
        materialInstance = objectRenderer.material;
    }
    
    // Public method to change both colors at once
    public void SwapColors(Color primary, Color secondary)
    {
        if (materialInstance == null) return;
        
        materialInstance.SetColor(primaryColorRef, primary);
        materialInstance.SetColor(secondaryColorRef, secondary);
    }
    
    // Individual color change methods
    public void SetPrimaryColor(Color newColor)
    {
        materialInstance?.SetColor(primaryColorRef, newColor);
    }
    
    public void SetSecondaryColor(Color newColor)
    {
        materialInstance?.SetColor(secondaryColorRef, newColor);
    }
    
    // // Example method to swap primary and secondary colors
    // public void SwapPrimaryAndSecondary()
    // {
    //     if (materialInstance == null) return;
        
    //     Color currentPrimary = materialInstance.GetColor(primaryColorRef);
    //     Color currentSecondary = materialInstance.GetColor(secondaryColorRef);
        
    //     materialInstance.SetColor(primaryColorRef, currentSecondary);
    //     materialInstance.SetColor(secondaryColorRef, currentPrimary);
    // }
    

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

    void OnDestroy()
    {
        // Clean up the material instance when the object is destroyed
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
