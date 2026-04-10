using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBars: MonoBehaviour
{
    public static UIEnergyBars Instance = null;
    
    [System.Serializable]
    public struct EnergyBarStruct
    {
        public Image mask;
        public float size;
    }
    public EnergyBarStruct[] energyBarsStructs;

    public enum EnergyBars
    {
        PlayerHealth,
        PlayerWeaponEnergy,
        EnemyHealth
    }
    [SerializeField] Sprite[] energySprite;
    public enum EnergyBarTypes
    {
        PlayerLife,
        MagnetBeam,
        BombMan,
        HyperBomb,
        CutMan,
        RollingCutter,
        ElecMan,
        ThunderBeam,
        FireMan,
        FireStorm,
        GutsMan,
        SuperArm,
        IceMan,
        IceSlasher,
        CopyRobot,
        CWU01P_1,
        CWU01P_2,
        CWU01P_3,
        CWU01P_4,
        CWU01P_5,
        CWU01P_6,
        CWU01P_7,
        YellowDevil,
        WilyMachine
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        foreach (EnergyBars energyBar in Enum.GetValues(typeof(EnergyBars)))
        {
            energyBarsStructs[(int)energyBar].size =
                energyBarsStructs[(int)energyBar].mask.rectTransform.rect.height;
        }
    }

    public void SetValue(EnergyBars energyBar, float value)
    {
        EnergyBarStruct energyBarStruct = energyBarsStructs[(int)energyBar];
        energyBarStruct.mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, energyBarStruct.size * value);
    }

    public void SetImage(EnergyBars energyBar, EnergyBarTypes energyBarType)
    {
        energyBarsStructs[(int)energyBar].mask.gameObject.transform.GetChild(0).GetComponent<Image>().sprite = energySprite[(int)energyBarType];
    }

    public void SetVisibility(EnergyBars energyBar, bool visible)
    {
        energyBarsStructs[(int)energyBar].mask.gameObject.transform.parent.GetComponent<CanvasGroup>().alpha = visible ? 1 : 0;
    }
}