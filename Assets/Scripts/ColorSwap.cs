using UnityEngine;
using UnityEngine.UI;

/*
 * Supports both:
 * - Renderer (SpriteRenderer / MeshRenderer / etc.)
 * - UI Image (UnityEngine.UI.Image)
 */
public class ColorSwap : MonoBehaviour
{
    [Header("Shader Properties")]
    [SerializeField] private string primaryColorRef = "_PrimaryColor";
    [SerializeField] private string secondaryColorRef = "_SecondaryColor";
    [SerializeField] private string backgroundColorRef = "_BackgroundColor";
    [SerializeField] private string newPrimaryColorRef = "_NewPrimaryColor";
    [SerializeField] private string newSecondaryColorRef = "_NewSecondaryColor";
    [SerializeField] private string newBackgroundColorRef = "_NewBackgroundColor";
    [SerializeField] private string mainTexRef = "_MainTex";
    
    private Renderer objectRenderer;
    private Image uiImage;
    private Material materialInstance;

    public Material MaterialInstance => materialInstance;
    
    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (materialInstance != null) return;

        objectRenderer = GetComponent<Renderer>();
        uiImage = GetComponent<Image>();

        if (objectRenderer != null)
        {
            materialInstance = new Material(objectRenderer.material);
            objectRenderer.material = materialInstance;
            return;
        }

        if (uiImage != null)
        {
            Material sourceMaterial = uiImage.material != null ? uiImage.material : uiImage.defaultMaterial ? uiImage.defaultMaterial : uiImage.defaultMaterial;
            materialInstance = new Material(sourceMaterial);
            uiImage.material = materialInstance;

            if (uiImage.sprite != null)
            {
                materialInstance.SetTexture(mainTexRef, uiImage.sprite.texture);
            }

            return;
        }

        Debug.LogError($"No Renderer or UI Image found on {gameObject.name}!");
    }
    
    // Public method to change both colors at once
    public void SwapColors(int primary, int secondary, Color newPrimary, Color newSecondary)
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor(primaryColorRef, ParseGray(primary));
            materialInstance.SetColor(secondaryColorRef, ParseGray(secondary));
            materialInstance.SetColor(newPrimaryColorRef, newPrimary);
            materialInstance.SetColor(newSecondaryColorRef, newSecondary);
        }
    }
    
    // Individual color change methods
    public void SetPrimaryColor(int primary,Color newColor)
    {
        if (materialInstance == null) return;

        materialInstance.SetColor(primaryColorRef, ParseGray(primary));
        materialInstance.SetColor(newPrimaryColorRef, newColor);
    }
    
    public void SetSecondaryColor(int secondary,Color newColor)
    {
        if (materialInstance == null) return;
        
        materialInstance.SetColor(secondaryColorRef, ParseGray(secondary));
        materialInstance.SetColor(newSecondaryColorRef, newColor);
    }

        public void SetBackgroundColor(int background,Color newColor)
    {
        if (materialInstance == null) return;
        
        materialInstance.SetColor(backgroundColorRef, ParseGray(background));
        materialInstance.SetColor(newBackgroundColorRef, newColor);
    }

    public void SetMainSprite(Sprite sprite)
    {
        if (materialInstance == null || sprite == null) return;

        if (uiImage != null)
        {
            uiImage.sprite = sprite;
        }
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

    public static Color ParseGray(int c)
    {
        return new Color(c / 255f, c / 255f, c / 255f);
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
