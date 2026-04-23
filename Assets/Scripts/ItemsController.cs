using UnityEngine;
using System;
using UnityEngine.Events;

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
    ColorSwap colorSwap;

    float destroyTimer;

    Color itemColor;
    bool freezeItem;
    bool animateItem;
    Vector2 freezeVelocity;
    RigidbodyConstraints2D rb2dConstraints;

    private enum SwapIndex
    {
        Primary = 64,
        Secondary = 128
    };

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
    public enum WeaponPartEnemies { None, BombMan, CutMan, ElecMan, FireMan, GutsMan, IceMan };
    [SerializeField] WeaponPartEnemies weaponPartEnemy = WeaponPartEnemies.None;

    [Header("Bonus Item Events")]
    public UnityEvent BonusItemEvent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        colorSwap = GetComponent<ColorSwap>();
        SetColorPalette();

        Animate(animate);

        SetDestroyDelay(destroyDelay);


        if(itemType == ItemType.BonusBall)
        {
            SetBonusBallColor(bonusBallColor);
        }
        
        if (itemType == ItemType.WeaponPart)
        {
            SetWeaponPartColor(weaponPartColor);
            
        }
    }

    void Update()
    {
        // if the bous item is frozen then don't allow it to destroy
        if (freezeItem) return;

        // countdown to destroy
        if (destroyDelay > 0)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer <= 0)
            {
                Destroy(gameObject);
            }
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
        destroyTimer = delay;
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

        public void SetColorPalette()
    {
        // not all bonus items have the ColorSwap component
        // only the Extra Life, Magnet Beam and Weapon Energies
        if (colorSwap != null)
        {
            colorSwap.SetMainSprite(sprite.sprite);
            
            // default to megabuster / magnetbeam colors
            // dark blue, light blue
            colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x0073F7));
            colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0x00FFFF));

            // find the player's controller to access the weapon type
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                // apply new selected color scheme with ColorSwap
                switch (player.currentWeapon)
                {
                    case PlayerController.WeaponTypes.HyperBomb:
                        // green, light gray
                        colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x009400));
                        colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                        break;
                    case PlayerController.WeaponTypes.RollingCutter:
                        // dark gray, light gray
                        colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x747474));
                        colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                        break;
                    case PlayerController.WeaponTypes.ThunderBeam:
                        // dark gray, light yellow
                        colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x747474));
                        colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCE4A0));
                        break;
                    case PlayerController.WeaponTypes.FireStorm:
                        // dark orange, yellow gold
                        colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0xD82800));
                        colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xF0BC3C));
                        break;
                    case PlayerController.WeaponTypes.SuperArm:
                        // orange red, light gray
                        colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0xC84C0C));
                        colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                        break;
                    case PlayerController.WeaponTypes.IceSlasher:
                        // dark blue, light gray
                        colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x2038EC));
                        colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                        break;
                }
            }
        }
    }

    public void FreezeItem(bool freeze)
    {
        // freeze/unfreeze the bonus item on screen
        // NOTE: this will be called from the GameManager but could be used in other scripts
        if (freeze)
        {
            freezeItem = true;
            animateItem = animate;
            if (animateItem) Animate(false);
            rb2dConstraints = rb.constraints;
            freezeVelocity = rb.linearVelocity;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            freezeItem = false;
            if (animateItem) Animate(true);
            rb.constraints = rb2dConstraints;
            rb.linearVelocity = freezeVelocity;
        }
    }

    public void HideItem(bool hide)
    {
        if (hide)
        {
            itemColor = sprite.color;
            sprite.color = Color.clear;
        }
        else
        {
            sprite.color = itemColor;
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

            if (itemType == ItemType.ExtraLife)
            {
                GameManager.Instance.AddPlayerLives(1);
            }

            if (itemType == ItemType.MagnetBeam)
            {
                player.EnableMagnetBeam(true);
            }

            if (itemType == ItemType.WeaponPart)
            {
                player.EnableWeaponPart(weaponPartEnemy);
            }

            if (itemClip)
            {
                SoundManager.Instance.Play(itemClip);
            }

            BonusItemEvent?.Invoke();

            Destroy(gameObject);
        }   
    }

}
