using UnityEngine;
using System;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class ItemsController: MonoBehaviour
{
    private Animator animator;
    private BoxCollider2D boxCollider2D;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    public enum ItemType
    {
        Nothing,
        Random,
        BonusBall,
        ExtraLife,
        LifeEnergyBig,
        LifeEnergySmall,
        WeaponEnergyBig,
        WeaponEnergySmall,
        MagnetBeam,
        WeaponPart,
        Yashichi
    }

    [SerializeField] ItemType itemType;

    [SerializeField] bool animate;
    [SerializeField] float destroyDelay;
    [SerializeField] int lifeEnergyValue;
    [SerializeField] int weaponEnergyValue;
    [SerializeField] int bonusPointsValue;


    [Header("Audio Clips")]
    [SerializeField] AudioClip itemClip;

    [Header("Bonus Ball Settings")]
    [SerializeField] RuntimeAnimatorController racBonusBallBlue;
    [SerializeField] RuntimeAnimatorController racBonusBallGray;
    [SerializeField] RuntimeAnimatorController racBonusBallGreen;
    [SerializeField] RuntimeAnimatorController racBonusBallOrange;
    [SerializeField] RuntimeAnimatorController racBonusBallRed;
    public enum BonusBallColor
    {
        Random,
        Blue,
        Gray,
        Green,
        Orange,
        Red
    }
    [SerializeField] BonusBallColor bonusBallColor = BonusBallColor.Blue;

    [Header("Weapon Part Settings")]
    [SerializeField] RuntimeAnimatorController racWeaponPartBlue;
    [SerializeField] RuntimeAnimatorController racWeaponPartRed;
    [SerializeField] RuntimeAnimatorController racWeaponPartOrange;
    public enum WeaponPartColor
    {
        Random,
        Blue,
        Red,
        Orange
    }
    [SerializeField] WeaponPartColor weaponPartColor = WeaponPartColor.Blue;

    void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Animate(animate);

        if (destroyDelay > 0)
        {
            SetDestroyDelay(destroyDelay);
        }

        if(itemType == ItemType.BonusBall)
        {
            SetBonusBallColor(bonusBallColor);
        }
        
        if (itemType == ItemType.WeaponPart)
        {
            SetWeaponPartColor(weaponPartColor);
            
        }
    }

    public void Animate(bool animate)
    {
        if (animate)
        {
            animator.Play("Default");
            animator.speed = 1;
        }
        else
        {
            animator.Play("Default", 0, 0);
            animator.speed = 0;
        }
    }

    public void SetDestroyDelay(float delay)
    {
        Destroy(gameObject, delay);
    }

    public void SetBonusBallColor(BonusBallColor color)
    {
        if(itemType == ItemType.BonusBall)
        {
            bonusBallColor = color;
            SetBonusBallAnimatorController();
        }
    }

    void SetBonusBallAnimatorController()
    {
        BonusBallColor color = bonusBallColor;

        if (color == BonusBallColor.Random)
        {
            color = (BonusBallColor)UnityEngine.Random.Range(1, Enum.GetNames(typeof(BonusBallColor)).Length);
        }
        
        switch (color)
        {
            case BonusBallColor.Blue:
                animator.runtimeAnimatorController = racBonusBallBlue;
                break;
            case BonusBallColor.Gray:
                animator.runtimeAnimatorController = racBonusBallGray;
                break;
            case BonusBallColor.Green:
                animator.runtimeAnimatorController = racBonusBallGreen;
                break;
            case BonusBallColor.Orange:
                animator.runtimeAnimatorController = racBonusBallOrange;
                break;
            case BonusBallColor.Red:
                animator.runtimeAnimatorController = racBonusBallRed;
                break;
        }
    }

        public void SetWeaponPartColor(WeaponPartColor color)
    {
        if(itemType == ItemType.WeaponPart)
        {
            weaponPartColor = color;
            SetWeaponPartAnimatorController();
        }
    }

    void SetWeaponPartAnimatorController()
    {
        WeaponPartColor color = weaponPartColor;

        if (color == WeaponPartColor.Random)
        {
            color = (WeaponPartColor)UnityEngine.Random.Range(1, Enum.GetNames(typeof(WeaponPartColor)).Length);
        }
        
        switch (color)
        {
            case WeaponPartColor.Blue:
                animator.runtimeAnimatorController = racWeaponPartBlue;
                break;
            case WeaponPartColor.Red:
                animator.runtimeAnimatorController = racWeaponPartRed;
                break;
            case WeaponPartColor.Orange:
                animator.runtimeAnimatorController = racWeaponPartOrange;
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (lifeEnergyValue > 0)
            {
                player.ApplyLifeEnergy(lifeEnergyValue);
            }

            if (weaponEnergyValue > 0)
            {
                player.ApplyWeaponEnergy(weaponEnergyValue);
            }

            if (bonusPointsValue > 0)
            {
                GameManager.Instance.AddBonusPoints(bonusPointsValue);
            }

            if (itemClip)
            {
                SoundManager.Instance.Play(itemClip);
            }

            Destroy(gameObject);
        }   
    }

}
