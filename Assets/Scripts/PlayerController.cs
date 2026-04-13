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
    bool isTakingDamage;
    bool isInvincible;
    bool isFacingRight;
    bool isTeleporting;
    bool hitSideRight;

    bool freezeInput;
    bool freezePlayer;
    bool freezeBullets;

    float shootTime;
    bool keyShootRelease;

    RigidbodyConstraints2D originalConstraints;

    private enum SwapIndex
    {
        Primary = 64,
        Secondary = 128
    };
    public enum PlayerWeapons
    {
        Default,
        BombMan,
        CutMan,
        ElecMan,
        FireMan,
        GutsMan,
        IceMan,
    }
    public PlayerWeapons currentWeapon = PlayerWeapons.Default;

    [Serializable]
    public struct PlayerWeaponStats
    {
        public PlayerWeapons weapon;
        public int currentEnergy;
        public int maxEnergy;
    }
    public PlayerWeaponStats[] weaponStats;

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
    [SerializeField] Transform shootPoint;
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

        // Initialize weapon stats
        for (int i = 0; i < weaponStats.Length; i++)
        {
            weaponStats[i].currentEnergy = weaponStats[i].maxEnergy;
        }
        colorSwap = GetComponent<ColorSwap>();
        SetWeapon(currentWeapon);
    }

    void FixedUpdate()
    {
        isGrounded = false;
        Color raycastColor;
        RaycastHit2D raycastHit;
        float raycastDistance = 0.05f;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
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

        if (Input.GetKeyDown(KeyCode.T))
        {
            SetWeapon((PlayerWeapons)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerWeapons)).Length));
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

        float shootTimeLength = 0;
        float keyShootReleaseTimeLength = 0;

        if(keyShoot && keyShootRelease)
        {
            isShooting = true;
            keyShootRelease = false;
            shootTime = Time.time;
            Invoke("Shoot", 0.1f);
        }
        if(!keyShoot && !keyShootRelease)
        {
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }
        if(isShooting)
        {
            shootTimeLength = Time.time - shootTime;
            if(shootTimeLength > 0.25f || keyShootReleaseTimeLength >= 0.15f)
            {
                isShooting = false;
            }
        }
    }

    void PlayerMovementInput()
    {
        isShooting = keyShoot;

        if (keyHorizontal < 0)
        {
            if (isFacingRight)
            {
                Flip();
            }
            if (isGrounded)
            {
                if (isShooting)
                {
                    animator.Play("Player_RunShoot");
                }
                else
                {
                    animator.Play("Player_Run");
                }
            }
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        } 
        else if (keyHorizontal > 0)
        {  
            if (!isFacingRight)
            {
                Flip();
            }
            if (isGrounded)
            {
                if (isShooting)
                {
                    animator.Play("Player_RunShoot");
                }
                else
                {
                    animator.Play("Player_Run");
                }
            }
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        } 
        else
        {
            if (isGrounded)
            {
                if (isShooting)
                {
                    animator.Play("Player_Shoot");
                }
                else
                {
                    animator.Play("Player_Idle");
                }
            }
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (keyJump && isGrounded)
        {
                if (isShooting)
                {
                    animator.Play("Player_JumpShoot");
                }
                else
                {
                    animator.Play("Player_Jump");
                }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if(!isGrounded)
        {
                isJumping = true;
                if (isShooting)
                {
                    animator.Play("Player_JumpShoot");
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

    public void SetWeapon(PlayerWeapons weapon)
    {
        currentWeapon = weapon;
        int currentEnergy = weaponStats[(int)weapon].currentEnergy;
        int maxEnergy = weaponStats[(int)weapon].maxEnergy;
        float weaponEnergyValue = (float)currentEnergy / maxEnergy;

        switch (currentWeapon)
        {
            case PlayerWeapons.Default:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0x0073F7));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0x00FFFF));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.PlayerLife);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, false);
                break;
            case PlayerWeapons.BombMan:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0x009400));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0xFCFCFC));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.HyperBomb);
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                break;
            case PlayerWeapons.CutMan:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0x747474));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0xFCFCFC));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.RollingCutter);
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                break;
            case PlayerWeapons.ElecMan:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0x747474));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0xFCE4A0));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.ThunderBeam);
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                break;
            case PlayerWeapons.FireMan:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0xD82800));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0xF0BC3C));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.FireStorm);
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                break;
            case PlayerWeapons.GutsMan:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0xC84C0C));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0xFCFCFC));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.SuperArm);
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                break;
            case PlayerWeapons.IceMan:
                colorSwap.SetPrimaryColor(ColorSwap.ColorFromInt(0x2038EC));
                colorSwap.SetSecondaryColor(ColorSwap.ColorFromInt(0xFCFCFC));
                UIEnergyBars.Instance.SetImage(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, UIEnergyBars.EnergyBarTypes.IceSlasher);
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, weaponEnergyValue);
                UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.PlayerWeaponEnergy, true);
                break;
        }

        // colorSwap.ApplyColor();
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

    public void ApplyWeaponEnergy(int energy)
    {
        // Implement weapon energy logic here
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.name = bulletPrefab.name;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDamage(shootDamage);
        bulletScript.SetSpeed(shootSpeed);
        bulletScript.SetDirection(isFacingRight ? Vector2.right : Vector2.left);
        bulletScript.SetDestroyDelay(5f);
        bulletScript.Shoot();
        SoundManager.Instance.Play(shootSound);
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
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerHealth, currentHealth / (float)maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            } else
            {
                StartDamageAnimation();
            }
        }
    }

    void StartDamageAnimation()
    {
        if (!isTakingDamage)
        {
            isTakingDamage = true;
            Invincible(true);
            // FreezeInput(true);
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
        Material material = sprite.material;
        for (int i = 0; i < 10; i++)
        {
            // sprite.color = Color.clear;
            sprite.material = null;
            yield return new WaitForSeconds(flashDelay);
            // sprite.color = Color.white;
            sprite.material = material;
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
