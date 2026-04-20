using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    BoxCollider2D boxCollider;
    Rigidbody2D rb;
    SpriteRenderer sprite;
    ColorSwap colorSwap;

    float keyHorizontal;
    bool keyJump;
    bool keyShoot;
    bool isGrounded;
    bool isJumping;
    bool isShooting;
    bool isThrowing;
    bool isTakingDamage;
    bool isInvincible;
    bool isFacingRight;
    bool isTeleporting;
    bool hitSideRight;

    bool freezeInput;
    bool freezePlayer;
    bool freezeBullets;

    float shootTime;
    float shootTimeLength;
    bool keyShootRelease;
    float keyShootReleaseTimeLength;

    bool canUseWeapon;

    RigidbodyConstraints2D originalConstraints;

    private enum SwapIndex
    {
        Primary = 64,
        Secondary = 128
    };
    public enum WeaponTypes
    {
        MegaBuster,
        MagnetBeam,
        HyperBomb,
        RollingCutter,
        ThunderBeam,
        FireStorm,
        SuperArm,
        IceSlasher
    };
    public WeaponTypes currentWeapon = WeaponTypes.MegaBuster;

    [Serializable]
    public struct WeaponsStruct
    {
        public WeaponTypes weaponType;
        public bool enabled;
        public int currentEnergy;
        public int maxEnergy;
        public int energyCost;
        public int weaponDamage;
        public AudioClip weaponClip;
        public GameObject weaponPrefab;
    }
    public WeaponsStruct[] weaponsData;

    public int currentHealth;
    public int maxHealth = 28;


    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float jumpForce = 3.7f;

    [SerializeField] int shootDamage = 1;
    [SerializeField] float shootSpeed = 5f;


    [Header("Audio Clips")]
    [SerializeField] AudioClip teleportSound;
    [SerializeField] AudioClip jumpLandedSound;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip energyFillSound;
    [SerializeField] AudioClip deathSound;


    [Header("Position and Prefabs")]
    [SerializeField] Transform bulletShootPos;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject explosionPrefab;


    [Header("Teleportation Settings")]
    [SerializeField] float teleportSpeed = -10f;
    public enum TeleportState
    {
        Descending,
        Landed,
        Idle
    }
    [SerializeField] TeleportState teleportState;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        isFacingRight = true;
        currentHealth = maxHealth;

        colorSwap = GetComponent<ColorSwap>();
        SetWeapon(currentWeapon);

        FillWeaponEnergies();
    }

    void FixedUpdate()
    {
        isGrounded = false;
        Color raycastColor;
        RaycastHit2D raycastHit;
        float raycastDistance = 0.05f;
        int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("MagnetBeam");
        Vector3 boxOrigin = boxCollider.bounds.center;
        boxOrigin.y = boxCollider.bounds.min.y + (boxCollider.bounds.extents.y / 4f);
        Vector3 boxSize = boxCollider.bounds.size;
        boxSize.y = boxCollider.bounds.size.y / 4f;
        raycastHit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, Vector2.down, raycastDistance, layerMask);
        if (raycastHit.collider != null)
        {            
            isGrounded = true;
            if (isJumping)
            {
                SoundManager.Instance.Play(jumpLandedSound);
                isJumping = false;
            } 
        }
        raycastColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(boxOrigin + new Vector3(boxCollider.bounds.extents.x, 0), Vector2.down * (boxCollider.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(boxOrigin - new Vector3(boxCollider.bounds.extents.x, 0), Vector2.down * (boxCollider.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(boxOrigin - new Vector3(boxCollider.bounds.extents.x, boxCollider.bounds.extents.y / 4f + raycastDistance), Vector2.right * (boxCollider.bounds.extents.x * 2), raycastColor);
    }

    void Update()
    {
        if (isTeleporting)
        {
            switch (teleportState)
            {
                case TeleportState.Descending:
                    isJumping = false;
                    if (isGrounded)
                    {
                        teleportState = TeleportState.Landed;
                    }
                    break;
                case TeleportState.Landed:
                    animator.speed = 1;
                    break;
                case TeleportState.Idle:
                    Teleport(false);
                    break;
            }

            return;
        }

        if (isTakingDamage)
        {
            animator.Play("Player_Hit");
            return;
        }
        PlayerDebugInput();
        PlayerDirectionInput();
        PlayerJumpInput();
        PlayerShootInput();
        PlayerMovementInput();

        FireWeapon();
    }

    void PlayerDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
            if (bullets.Length > 0)
            {
                freezeBullets = !freezeBullets;
                foreach (GameObject bullet in bullets)                {
                    bullet.GetComponent<Bullet>().Freeze(freezeBullets);
                }
            }
            Debug.Log("Bullet freeze toggled. Now frozen: " + freezeBullets);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            TakeDamage(1);
            Debug.Log("Player took damage. Current health: " + currentHealth);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Die();
            Debug.Log("Player died.");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Invincible(!isInvincible);
            Debug.Log("Player invincibility toggled. Now invincible: " + isInvincible);
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            FreezeInput(!freezeInput);
            Debug.Log("Input freeze toggled. Now frozen: " + freezeInput);
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            FreezePlayer(!freezePlayer);
            Debug.Log("Player freeze toggled. Now frozen: " + freezePlayer);
        }

        // S for Switch Weapon
        if (Input.GetKeyDown(KeyCode.S))
        {
            int nextWeapon = (int)currentWeapon;
            int maxWeapons = weaponsData.Length;
            while (true)
            {
                // cycle to next weapon index
                if (++nextWeapon > maxWeapons - 1)
                {
                    nextWeapon = 0;
                }
                // if weapon is enabled then use it
                if (weaponsData[nextWeapon].enabled)
                {
                    SwitchWeapon((WeaponTypes)nextWeapon);
                    break;
                }
            }
            Debug.Log("SwitchWeapon()");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // SetWeapon((PlayerWeapons)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerWeapons)).Length));
            Teleport(true);
            Debug.Log("Player teleport initiated.");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ApplyLifeEnergy(10);
            Debug.Log("Applied life energy. Current health: " + currentHealth);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.EnemyHealth, (UIEnergyBars.EnergyBarTypes)UnityEngine.Random.Range(0, 
                Enum.GetValues(typeof(UIEnergyBars.EnergyBarTypes)).Length));
            Debug.Log("Set enemy health bar to random type.");
        }
    }

    void PlayerDirectionInput()
    {
        if (!freezeInput)
        {
            keyHorizontal = Input.GetAxisRaw("Horizontal");
        }
    }

    void PlayerJumpInput()
    {
        if (!freezeInput)
        {
            keyJump = Input.GetKeyDown(KeyCode.Space);
        }
    }

    void PlayerShootInput()
    {
        if (!freezeInput)
        {
            keyShoot = Input.GetKey(KeyCode.C);
        }
    }

    void PlayerMovementInput()
    {
        // override speed may vary depending on state
        float speed = moveSpeed;

        // left arrow key - moving left
        if (keyHorizontal < 0)
        {
            // facing right while moving left - flip
            if (isFacingRight)
            {
                Flip();
            }
            // grounded play run animation
            if (isGrounded)
            {
                // play run shoot or run animation
                if (isShooting)
                {
                    animator.Play("Player_RunShoot");
                }
                else if (isThrowing)
                {
                    speed = 0f;
                    animator.Play("Player_Throw");
                }
                else
                {
                    animator.Play("Player_Run");
                }
            }
            // negative move speed to go left
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
        }
        else if (keyHorizontal > 0) // right arrow key - moving right
        {
            // facing left while moving right - flip
            if (!isFacingRight)
            {
                Flip();
            }
            // grounded play run animation
            if (isGrounded)
            {
                // play run shoot or run animation
                if (isShooting)
                {
                    animator.Play("Player_RunShoot");
                }
                else if (isThrowing)
                {
                    speed = 0f;
                    animator.Play("Player_Throw");
                }
                else
                {
                    animator.Play("Player_Run");
                }
            }
            // positive move speed to go right
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }
        else   // no movement
        {
            // grounded play idle animation
            if (isGrounded)
            {
                // play shoot or idle animation
                if (isShooting)
                {
                    animator.Play("Player_Shoot");
                }
                else if (isThrowing)
                {
                    animator.Play("Player_Throw");
                }
                else
                {
                    animator.Play("Player_Idle");
                }
            }
            // no movement zero x velocity
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // pressing jump while grounded - can only jump once
        if (keyJump && isGrounded)
        {
            // play jump/jump shoot animation and jump speed on y velocity
            if (isShooting)
            {
                animator.Play("Player_JumpShoot");
            }
            else if (isThrowing)
            {
                animator.Play("Player_JumpThrow");
            }
            else
            {
                animator.Play("Player_Jump");
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // while not grounded play jump animation (jumping or falling)
        if (!isGrounded)
        {
            // triggers jump landing sound effect in FixedUpdate
            isJumping = true;
            // jump or jump shoot animation
            if (isShooting)
            {
                animator.Play("Player_JumpShoot");
            }
            else if (isThrowing)
            {
                animator.Play("Player_JumpThrow");
            }
            else
            {
                animator.Play("Player_Jump");
            }
        }
    }



    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void SetWeapon(WeaponTypes weapon)
    {
        currentWeapon = weapon;
        int currentEnergy = weaponsData[(int)weapon].currentEnergy;
        int maxEnergy = weaponsData[(int)weapon].maxEnergy;
        float weaponEnergyValue = (float)currentEnergy / maxEnergy;

        colorSwap.SetMainSprite(sprite.sprite);

        switch (currentWeapon)
        {
            case WeaponTypes.MegaBuster:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x0073F7));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0x00FFFF));
                if (UIEnergyBars.Instance)
                {                    
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.PlayerLife);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, false);
                }
                break;
                case WeaponTypes.MagnetBeam:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x0073F7));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0x00FFFF));
                if (UIEnergyBars.Instance)
                {      
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.MagnetBeam);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
            case WeaponTypes.HyperBomb:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x009400));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.HyperBomb);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
            case WeaponTypes.RollingCutter:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x747474));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.RollingCutter);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
            case WeaponTypes.ThunderBeam:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x747474));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCE4A0));
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.ThunderBeam);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
            case WeaponTypes.FireStorm:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0xD82800));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xF0BC3C));
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.FireStorm);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
            case WeaponTypes.SuperArm:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0xC84C0C));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.SuperArm);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
            case WeaponTypes.IceSlasher:
                colorSwap.SetPrimaryColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x2038EC));
                colorSwap.SetSecondaryColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0xFCFCFC));
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.IceSlasher);
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                    UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                }
                break;
        }
    }

    public void SwitchWeapon(WeaponTypes weaponType)
    {
        // we can call this function to switch the player to the chosen weapon
        // change color scheme, do the teleport animation, and enable weapon usage
        SetWeapon(weaponType);
        Teleport(true);
        CanUseWeaponAgain();

        // update any in scene bonus item color palettes
        GameManager.Instance.SetBonusItemsColorPalette();
    }

        void FireWeapon()
    {
        // each weapon has its own function for firing
        switch (currentWeapon)
        {
            case WeaponTypes.MegaBuster:
                MegaBuster();
                break;
            case WeaponTypes.MagnetBeam:
                MagnetBeam();
                break;
            case WeaponTypes.HyperBomb:
                HyperBomb();
                break;
            case WeaponTypes.RollingCutter:
                break;
            case WeaponTypes.ThunderBeam:
                break;
            case WeaponTypes.FireStorm:
                break;
            case WeaponTypes.SuperArm:
                break;
            case WeaponTypes.IceSlasher:
                break;
        }
    }

    void MegaBuster()
    {
        shootTimeLength = 0;
        keyShootReleaseTimeLength = 0;

        // shoot key is being pressed and key release flag true
        if (keyShoot && keyShootRelease)
        {
            isShooting = true;
            keyShootRelease = false;
            shootTime = Time.time;
            // Shoot Bullet
            Invoke("Shoot", 0.1f);
        }
        // shoot key isn't being pressed and key release flag is false
        if (!keyShoot && !keyShootRelease)
        {
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }
        // while shooting limit its duration
        if (isShooting)
        {
            shootTimeLength = Time.time - shootTime;
            if (shootTimeLength >= 0.25f || keyShootReleaseTimeLength >= 0.15f)
            {
                isShooting = false;
            }
        }
    }

    void HyperBomb()
    {
        shootTimeLength = 0;
        keyShootReleaseTimeLength = 0;

        // shoot key is being pressed and key release flag true
        if (keyShoot && keyShootRelease && canUseWeapon)
        {
            // only be able to throw a hyper bomb if there is energy to do so
            // placing the check here so isThrowing can't become true and activate the arm throw animation
            if (weaponsData[(int)WeaponTypes.HyperBomb].currentEnergy > 0)
            {
                isThrowing = true;
                canUseWeapon = false;
                keyShootRelease = false;
                shootTime = Time.time;
                // Throw Bomb
                Invoke("ThrowBomb", 0.1f);
                // spend weapon energy and refresh energy bar
                SpendWeaponEnergy(WeaponTypes.HyperBomb);
                RefreshWeaponEnergyBar(WeaponTypes.HyperBomb);
            }
        }
        // shoot key isn't being pressed and key release flag is false
        if (!keyShoot && !keyShootRelease)
        {
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }
        // while shooting limit its duration
        if (isThrowing)
        {
            shootTimeLength = Time.time - shootTime;
            if (shootTimeLength >= 0.25f)
            {
                isThrowing = false;
            }
        }
    }

    void MagnetBeam()
    {
        shootTimeLength = 0;
        keyShootReleaseTimeLength = 0;

        // shoot key is being pressed and key release flag true
        if (keyShoot && keyShootRelease && canUseWeapon)
        {
            // only be able to use the magnet beam if there is energy to do so
            // and haven't hit the maxinum number of beams on screen at a single time (3)
            if (weaponsData[(int)WeaponTypes.MagnetBeam].currentEnergy > 0 &&
                GameObject.FindGameObjectsWithTag("PlatformBeam").Length < 3)
            {
                isShooting = true;
                canUseWeapon = false;
                keyShootRelease = false;
                shootTime = Time.time;
                // Shoot Magnet Beam
                ShootMagnetBeam();
                // spend weapon energy and refresh energy bar
                SpendWeaponEnergy(WeaponTypes.MagnetBeam);
                RefreshWeaponEnergyBar(WeaponTypes.MagnetBeam);
            }
        }
        // shoot key isn't being pressed and key release flag is false
        if (!keyShoot && !keyShootRelease)
        {
            shootTimeLength = Time.time - shootTime;
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }
        // shoot key released while shooting
        if (isShooting && !keyShoot)
        {
            isShooting = false;
            GameObject beam = bulletShootPos.transform.Find("PlatformBeam").gameObject;
            // lock beam into place
            beam?.GetComponent<MagnetBeamScript>().LockBeam();
        }
    }


    public void ApplyLifeEnergy(int energy)
    {
        if(currentHealth < maxHealth)
        {
            int healthDiff = maxHealth - currentHealth;
            if (healthDiff > energy) healthDiff = energy;
            StartCoroutine(ApplyLifeEnergyCoroutine(healthDiff));

        }
    }

    private IEnumerator ApplyLifeEnergyCoroutine(int energy)
    {
        SoundManager.Instance.Play(energyFillSound, true);
        for (int i = 0; i < energy; i++)
        {
            currentHealth++;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerHealth, currentHealth / (float)maxHealth);
            yield return new WaitForSeconds(0.05f);
        }
        SoundManager.Instance.Stop();
    }

 public void ApplyWeaponEnergy(int amount)
    {
        // only apply weapon energy if we need it
        int wt = (int)currentWeapon;
        if (weaponsData[wt].currentEnergy < weaponsData[wt].maxEnergy)
        {
            int energyDiff = weaponsData[wt].maxEnergy - weaponsData[wt].currentEnergy;
            if (energyDiff > amount) energyDiff = amount;
            // animate adding energy bars via coroutine
            StartCoroutine(AddWeaponEnergy(energyDiff));
        }
    }

    private IEnumerator AddWeaponEnergy(int amount)
    {
        int wt = (int)currentWeapon;
        // loop the energy fill audio clip
        SoundManager.Instance.Play(energyFillSound, true);
        // increment the energy bars with a small delay
        for (int i = 0; i < amount; i++)
        {
            weaponsData[wt].currentEnergy++;
            weaponsData[wt].currentEnergy = Mathf.Clamp(weaponsData[wt].currentEnergy, 0, weaponsData[wt].maxEnergy);
            UIEnergyBars.Instance.SetValue(
                UIEnergyBars.EnergyBars.PlayerWeaponEnergy,
                weaponsData[wt].currentEnergy / (float)weaponsData[wt].maxEnergy);
            yield return new WaitForSeconds(0.05f);
        }
        // done playing energy fill clip
        SoundManager.Instance.Stop();
    }
    public void FillWeaponEnergies()
    {
        // Initialize weapon stats
        for (int i = 0; i < weaponsData.Length; i++)
        {
            weaponsData[i].currentEnergy = weaponsData[i].maxEnergy;
        }
    }

    public void EnableMagnetBeam(bool enable)
    {
        // enable/disable the magnet beam
        weaponsData[(int)WeaponTypes.MagnetBeam].enabled = enable;
    }

    public void EnableWeaponPart(ItemsController.WeaponPartEnemies weaponPartEnemy)
    {
        // this will enable the collected weapon part in our weapon struct
        switch (weaponPartEnemy)
        {
            case ItemsController.WeaponPartEnemies.BombMan:
                weaponsData[(int)WeaponTypes.HyperBomb].enabled = true;
                break;
            case ItemsController.WeaponPartEnemies.CutMan:
                weaponsData[(int)WeaponTypes.RollingCutter].enabled = true;
                break;
            case ItemsController.WeaponPartEnemies.ElecMan:
                weaponsData[(int)WeaponTypes.ThunderBeam].enabled = true;
                break;
            case ItemsController.WeaponPartEnemies.FireMan:
                weaponsData[(int)WeaponTypes.FireStorm].enabled = true;
                break;
            case ItemsController.WeaponPartEnemies.GutsMan:
                weaponsData[(int)WeaponTypes.SuperArm].enabled = true;
                break;
            case ItemsController.WeaponPartEnemies.IceMan:
                weaponsData[(int)WeaponTypes.IceSlasher].enabled = true;
                break;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletShootPos.position, Quaternion.identity);
        bullet.name = bulletPrefab.name;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDamage(shootDamage);
        bulletScript.SetSpeed(shootSpeed);
        bulletScript.SetDirection(isFacingRight ? Vector2.right : Vector2.left);
        bulletScript.SetDestroyDelay(5f);
        bulletScript.Shoot();
        SoundManager.Instance.Play(shootSound);
    }

    void ThrowBomb()
    {
        // create bomb from prefab gameobject
        GameObject bomb = Instantiate(weaponsData[(int)WeaponTypes.HyperBomb].weaponPrefab);
        bomb.name = weaponsData[(int)WeaponTypes.HyperBomb].weaponPrefab.name + "(" + gameObject.name + ")";
        bomb.transform.position = bulletShootPos.position;
        // set the bomb properties and throw it
        bomb.GetComponent<BombScript>().SetContactDamageValue(0);
        bomb.GetComponent<BombScript>().SetExplosionDamageValue(weaponsData[(int)WeaponTypes.HyperBomb].weaponDamage);
        bomb.GetComponent<BombScript>().SetExplosionDelay(3f);
        bomb.GetComponent<BombScript>().SetCollideWithTags("Enemy");
        bomb.GetComponent<BombScript>().SetDirection(isFacingRight ? Vector2.right : Vector2.left);
        bomb.GetComponent<BombScript>().SetVelocity(new Vector2(2f, 1.5f));
        bomb.GetComponent<BombScript>().Bounces(true);
        bomb.GetComponent<BombScript>().ExplosionEvent.AddListener(CanUseWeaponAgain);
        bomb.GetComponent<BombScript>().Launch(false);
    }

    void ShootMagnetBeam()
    {
        // create magnet beam platform from prefab gameobject
        GameObject beam = Instantiate(weaponsData[(int)WeaponTypes.MagnetBeam].weaponPrefab);
        beam.name = weaponsData[(int)WeaponTypes.MagnetBeam].weaponPrefab.name;
        beam.transform.position = bulletShootPos.position;
        beam.transform.parent = bulletShootPos.transform;
        // set the platform beam properties and play the audio clip
        beam.GetComponent<MagnetBeamScript>().SetDestroyDelay(3f);
        beam.GetComponent<MagnetBeamScript>().SetDirection(isFacingRight ? Vector2.right : Vector2.left);
        beam.GetComponent<MagnetBeamScript>().SetMaxSegments(30);
        beam.GetComponent<MagnetBeamScript>().LockedEvent.AddListener(CanUseWeaponAgain);
        SoundManager.Instance.Play(weaponsData[(int)WeaponTypes.MagnetBeam].weaponClip);
    }

    void SpendWeaponEnergy(WeaponTypes weaponType)
    {
        // deplete the weapon energy and make sure the value is within bounds
        int wt = (int)weaponType;
        weaponsData[wt].currentEnergy -= weaponsData[wt].energyCost;
        weaponsData[wt].currentEnergy = Mathf.Clamp(weaponsData[wt].currentEnergy, 0, weaponsData[wt].maxEnergy);
    }

    void RefreshWeaponEnergyBar(WeaponTypes weaponType)
    {
        // refresh the weapon energy bar (should be called after SpendWeaponEnergy)
        int wt = (int)weaponType;
        UIEnergyBars.Instance?.SetValue(
                UIEnergyBars.EnergyBars.PlayerWeaponEnergy,
                weaponsData[wt].currentEnergy / (float)weaponsData[wt].maxEnergy);
    }

    void CanUseWeaponAgain()
    {
        // many (almost all) of our weapons require they play out their animation or be destroyed
        // before another copy can be used so this function resets the flag to be able to fire again
        canUseWeapon = true;
        isShooting = false;
        isThrowing = false;
    }

    public void HitSide(bool hitRight)
    {
        hitSideRight = hitRight;
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            if (damage > 0)
            {
                currentHealth -= damage;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                if (UIEnergyBars.Instance)
                {
                    UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerHealth, currentHealth / (float)maxHealth);
                }
                if (currentHealth <= 0)
                {
                    Die();
                } else
                {
                    StartDamageAnimation();
                }
            }
        }
    }

    void StartDamageAnimation()
    {
        if (!isTakingDamage)
        {
            isTakingDamage = true;
            Invincible(true);
            FreezeInput(true);
            float hitForceX = 0.5f;
            float hitForceY = 1.5f;
            if (hitSideRight) hitForceX = -hitForceX;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(hitForceX, hitForceY), ForceMode2D.Impulse);
            SoundManager.Instance.Play(hitSound);
        }
    }

    void StopDamageAnimation()
    {
        isTakingDamage = false;
        Invincible(false);
        FreezeInput(false);
        animator.Play("Player_Hit", -1, 0f);
        StartCoroutine(FlashAfterDamage());
    }

    private IEnumerator FlashAfterDamage()
    {
        float flashDelay = 0.0833f;
        // Material material = sprite.material;
        for (int i = 0; i < 10; i++)
        {
            sprite.color = Color.clear;
            // sprite.material = null;
            yield return new WaitForSeconds(flashDelay);
            sprite.color = Color.white;
            // sprite.material = material;
            yield return new WaitForSeconds(flashDelay);
        }
        Invincible(false);
    }

    void StartDeathAnimation()
    {
        FreezeInput(true);
        FreezePlayer(true);
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.name = explosionPrefab.name;
        explosion.transform.position = sprite.bounds.center;
        SoundManager.Instance.Play(deathSound);
        Destroy(gameObject);
    }

    void StopDeathAnimation()
    {
        FreezeInput(false);
        FreezePlayer(false);
    }

    public void Invincible(bool invincible)
    {
        isInvincible = invincible;
    }

    void Die()
    {
        GameManager.Instance.GameOver();
        Invoke("StartDeathAnimation", 0.5f);
    }

    public void FreezeInput(bool freeze)
    {
        freezeInput = freeze;
        if (freeze)
        {
            keyHorizontal = 0;
            keyJump = false;
            keyShoot = false;
        }
    }

    public void Teleport(bool teleport)
    {
        if(teleport)
        {
            isTeleporting = true;
            FreezeInput(true);
            animator.Play("Player_Teleport");
            animator.speed = 0;
            teleportState = TeleportState.Descending;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, teleportSpeed);
        } 
        else
        {
            isTeleporting = false;
            FreezeInput(false);
        }   
    }

    public void TeleportAnimationSound()
    {
        SoundManager.Instance.Play(teleportSound);
    }

    public void TeleportAnimationEnd()
    {
        teleportState = TeleportState.Idle;
    }

    public void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            originalConstraints = rb.constraints;
            animator.speed = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            rb.constraints = originalConstraints;
            animator.speed = 1;
        }
        freezePlayer = freeze;
    }

    // public void MobileShootWrapper()
    // {
    //     // wrapper function for button handler script
    //     // can't directly call coroutines
    //     if (!freezeInput)
    //     {
    //         StartCoroutine(MobileShoot());
    //     }
    // }

    // private IEnumerator MobileShoot()
    // {
    //     // press shoot and release
    //     keyShoot = true;
    //     yield return new WaitForSeconds(0.01f);
    //     keyShoot = false;
    // }

    // public void MobileJumpWrapper()
    // {
    //     // wrapper function for button handler script
    //     // can't directly call coroutines
    //     if (!freezeInput)
    //     {
    //         StartCoroutine(MobileJump());
    //     }
    // }

    // private IEnumerator MobileJump()
    // {
    //     // press jump and release
    //     keyJump = true;
    //     yield return new WaitForSeconds(0.01f);
    //     keyJump = false;
    // }

    public void SimulateMoveStop()
    {
        keyHorizontal = 0;
    }

    public void SimulateMoveLeft()
    {
        keyHorizontal = -1;
    }

    public void SimulateMoveRight()
    {
        keyHorizontal = 1;
    }
    // public void SimulateShoot()
    // {
    //    StartCoroutine(MobileShoot());
    // }

    // public void SimulateJump()
    // {
    //     keyJump = true;
    //     StartCoroutine(MobileJump());
    // }
}
