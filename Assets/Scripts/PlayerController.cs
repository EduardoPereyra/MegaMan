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
    float keyVertical;
    bool keyJump;
    bool keyShoot;
    bool isGrounded;
    bool isClimbing;
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
    bool freezeEverything;

    float shootTime;
    float shootTimeLength;
    bool keyShootRelease;
    float keyShootReleaseTimeLength;

    string lastAnimationName;

    bool jumpStarted;

    bool canUseWeapon;

    RigidbodyConstraints2D originalConstraints;

    // ladder climbing variables
    float transformY;
    float transformHY;
    bool isClimbingDown;
    bool atLaddersEnd;
    bool hasStartedClimbing;
    bool startedClimbTransition;
    bool finishedClimbTransition;

    private enum SwapIndex
    {
        Primary = 64,
        Secondary = 128
    };
    public enum WeaponTypes
    {
        HyperBomb,
        ThunderBeam,
        SuperArm,
        IceSlasher,
        RollingCutter,
        FireStorm,
        MagnetBeam,
        MegaBuster,
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
        public Vector2 weaponVelocity;
        public AudioClip weaponClip;
        public GameObject weaponPrefab;
    }
    public WeaponsStruct[] weaponsData;

    public int currentHealth;
    public int maxHealth = 28;

    [HideInInspector] public LadderScript ladder;

    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float jumpForce = 3.7f;
    [SerializeField] float climbSpeed = 0.525f;

    [Header("Audio Clips")]
    [SerializeField] AudioClip teleportSound;
    [SerializeField] AudioClip jumpLandedSound;
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip energyFillSound;
    [SerializeField] AudioClip deathSound;


    [Header("Position and Prefabs")]
    [SerializeField] Transform bulletShootPos;
    [SerializeField] GameObject explosionPrefab;

    [Header("Ladder Settings")]
    [SerializeField] float climbSpriteHeight = 0.36f;

    [Header("Teleportation Settings")]
    [SerializeField] float teleportSpeed = -10f;
    [SerializeField] float teleportLandingY = 0f;

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
        GameManager.Instance.RestorePlayerWeapons();
    }

    void FixedUpdate()
    {
        isGrounded = false;
        Color raycastColor;
        RaycastHit2D raycastHit;
        float raycastDistance = 0.025f;
        int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("MagnetBeam");
        Vector3 boxOrigin = boxCollider.bounds.center;
        boxOrigin.y = boxCollider.bounds.min.y + (boxCollider.bounds.extents.y / 4f);
        Vector3 boxSize = boxCollider.bounds.size;
        boxSize.y = boxCollider.bounds.size.y / 4f;
        raycastHit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, Vector2.down, raycastDistance, layerMask);
        if (raycastHit.collider != null && gameObject.layer != LayerMask.NameToLayer("Teleport") && !jumpStarted)
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
                    if (transform.position.y <= teleportLandingY)
                    {
                        gameObject.tag = "Player";
                        gameObject.layer = LayerMask.NameToLayer("Player");

                        rb.linearVelocity = Vector2.zero;
                        rb.constraints = RigidbodyConstraints2D.FreezeAll;
                        transform.position = new Vector3(transform.position.x, teleportLandingY, 0);
                        teleportState = TeleportState.Landed;
                    }
                    break;
                case TeleportState.Landed:
                    animator.speed = 1;
                    break;
                case TeleportState.Idle:
                    Teleport(false);
                    GameManager.Instance.TeleportFinished();
                    break;
            }

            return;
        }

        if (isTakingDamage)
        {
            PlayAnimation("Player_Hit");
            return;
        }

        if (!GameManager.Instance.IsGamePaused() &&
            !GameManager.Instance.InCameraTransition())
        {
            PlayerDebugInput();
            PlayerDirectionInput();
            PlayerJumpInput();
            PlayerShootInput();
        }

        PlayerMovement();

        FireWeapon();
    }

    void PlayerDebugInput()
    {
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

        if(Input.GetKeyDown(KeyCode.F))
        {
            freezeEverything = !freezeEverything;
            GameManager.Instance.FreezeEverything(freezeEverything);
            Debug.Log("Paused: " + freezeEverything);
        }

    }

    void PlayerDirectionInput()
    {
        if (!freezeInput)
        {
            keyHorizontal = Input.GetAxis("Horizontal");
            keyVertical = Input.GetAxisRaw("Vertical");
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

    void PlayerMovement()
    {
        // these are for the ladder climbing but can be used elsewhere
        // y position and the y position with the climb sprite height
        transformY = transform.position.y;
        transformHY = transformY + climbSpriteHeight;

        // override speed may vary depending on state
        float speed = moveSpeed;

        // ladder climbing part
        if (isClimbing && ladder)
        {
            // debug lines for our ladder handling
            Debug.DrawLine(new Vector3(ladder.posX - 2f, ladder.posTopHandlerY, 0),
                new Vector3(ladder.posX + 2f, ladder.posTopHandlerY, 0), Color.blue);
            Debug.DrawLine(new Vector3(ladder.posX - 2f, ladder.posBottomHandlerY, 0),
                new Vector3(ladder.posX + 2f, ladder.posBottomHandlerY, 0), Color.blue);
            Debug.DrawLine(new Vector3(transform.position.x - 2f, transformHY, 0),
                new Vector3(transform.position.x + 2f, transformHY, 0), Color.magenta);
            Debug.DrawLine(new Vector3(transform.position.x - 2f, transformY, 0),
                new Vector3(transform.position.x + 2f, transformY, 0), Color.magenta);

            // we just passed the top ladder handler position 
            if (transformHY > ladder.posTopHandlerY)
            {
                // this should only happen when we're not climbing down
                // otherwise we get some real funky results!
                if (!isClimbingDown)
                {
                    // start the climb transition animation
                    if (!startedClimbTransition)
                    {
                        startedClimbTransition = true;
                        ClimbTransition(true);
                    }
                    else if (finishedClimbTransition)
                    {
                        // we only want this block to happen once
                        finishedClimbTransition = false;

                        // we may not be completely touching the ground so setting
                        // this to false will stop the jump landed audio clip
                        isJumping = false;

                        // climb transition has finished now reposition ourself
                        // we kind of dip into the ground so we pad a little on our new y
                        PlayAnimation("Player_Idle");
                        transform.position = new Vector2(ladder.posX, ladder.posPlatformY + 0.005f);

                        // at the top of the ladder
                        if (!atLaddersEnd)
                        {
                            // reset climbing after a short delay
                            // gives the rigidbody and ground check to settle
                            atLaddersEnd = true;
                            Invoke("ResetClimbing", 0.1f);
                        }
                    }
                }
            }
            else if (transformHY < ladder.posBottomHandlerY)
            {
                // reaching this point means we have gone below of bottom handler
                // and haven't touched the ground so we should let go of the ladder
                ResetClimbing();
            }
            else
            {
                // this should only happen when we're not climbing down
                // otherwise we get some real funky results!
                if (!isClimbingDown)
                {
                    // jump off the ladder as long as there is no vertical input
                    if (keyJump && keyVertical == 0)
                    {
                        ResetClimbing();
                    }
                    // reached the ground by climbing down
                    else if (isGrounded && !hasStartedClimbing)
                    {
                        // we may not be completely touching the ground so setting
                        // this to false will stop the jump landed audio clip
                        isJumping = false;

                        // climbing has finished and now reposition ourself
                        // we kind of dip into the ground so we shave a little off our new y
                        PlayAnimation("Player_Idle");
                        transform.position = new Vector2(ladder.posX, ladder.posBottomY - 0.005f);

                        // at the bottom of the ladder
                        if (!atLaddersEnd)
                        {
                            // reset climbing after a short delay
                            // gives the rigidbody and ground check to settle
                            atLaddersEnd = true;
                            Invoke("ResetClimbing", 0.1f);
                        }
                    }
                    // somewhere in between the top and bottom of the ladder
                    else
                    {
                        // animate if we're moving in either direction
                        animator.speed = Mathf.Abs(keyVertical);

                        // move on the ladder as long as we're not shooting/throwing
                        if (keyVertical != 0 && !isShooting && !isThrowing &&
                            !GameManager.Instance.InCameraTransition())
                        {
                            // apply the direction and climb speed to our position
                            Vector3 climbDirection = new Vector3(0, climbSpeed) * keyVertical;
                            transform.position = transform.position + climbDirection * Time.deltaTime;
                        }

                        // if we're shooting or throwing then we can change our horizontal direction
                        if (isShooting || isThrowing)
                        {
                            // update the facing direction
                            if (keyHorizontal < 0)
                            {
                                // facing right while shooting left - flip
                                if (isFacingRight)
                                {
                                    Flip();
                                }
                            }
                            else if (keyHorizontal > 0)
                            {
                                // facing left while shooting right - flip
                                if (!isFacingRight)
                                {
                                    Flip();
                                }
                            }
                            // and then choose which animation to play
                            if (isShooting)
                            {
                                // play the shooting climb animation
                                PlayAnimation("Player_ClimbShoot");
                            }
                            else if (isThrowing)
                            {
                                // play the throwing climb animation
                                PlayAnimation("Player_ClimbThrow");
                            }
                        }
                        else
                        {
                            // not shooting or throwing then we play
                            // the regular climbing animation
                            PlayAnimation("Player_Climb");
                        }
                    }
                }
            }
        }
        // not climbing on any ladders
        else
        {
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
                        PlayAnimation("Player_RunShoot");
                    }
                    else if (isThrowing)
                    {
                        speed = 0f;
                        PlayAnimation("Player_Throw");
                    }
                    else
                    {
                        PlayAnimation("Player_Run");
                    }
                }
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
                        PlayAnimation("Player_RunShoot");
                    }
                    else if (isThrowing)
                    {
                        speed = 0f;
                        PlayAnimation("Player_Throw");
                    }
                    else
                    {
                        PlayAnimation("Player_Run");
                    }
                }
            }
            else   // no movement
            {
                // grounded play idle animation
                if (isGrounded)
                {
                    // play shoot or idle animation
                    if (isShooting)
                    {
                        PlayAnimation("Player_Shoot");
                    }
                    else if (isThrowing)
                    {
                        PlayAnimation("Player_Throw");
                    }
                    else
                    {
                        PlayAnimation("Player_Idle");
                    }
                }
            }
            rb.linearVelocity = new Vector2(speed * keyHorizontal, rb.linearVelocity.y);

            Jump();

            // while not grounded play jump animation (jumping or falling)
            if (!isGrounded)
            {
                // triggers jump landing sound effect in FixedUpdate
                isJumping = true;
                // jump or jump shoot animation
                if (isShooting)
                {
                    PlayAnimation("Player_JumpShoot");
                }
                else if (isThrowing)
                {
                    PlayAnimation("Player_JumpThrow");
                }
                else
                {
                    PlayAnimation("Player_Jump");
                }
            }

            // start ladder climbing here
            if (ladder != null)
            {
                // climbing up
                if (ladder.isNearLadder && keyVertical > 0 && transformHY < ladder.posTopHandlerY)
                {
                    isClimbing = true;
                    isClimbingDown = false;
                    animator.speed = 0;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    transform.position = new Vector3(ladder.posX, transformY + 0.025f, 0);
                    StartedClimbing();
                }

                // climbing down
                if (ladder.isNearLadder && keyVertical < 0 && isGrounded && transformHY > ladder.posTopHandlerY)
                {
                    isClimbing = true;
                    isClimbingDown = true;
                    animator.speed = 0;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    transform.position = new Vector3(ladder.posX, transformY, 0);
                    ClimbTransition(false);
                }
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    void Jump()
    {
       if (keyJump && isGrounded && !jumpStarted)
       {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            StartCoroutine(JumpCo());
       }
    }

    private IEnumerator JumpCo()
    {
        jumpStarted = true;
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        jumpStarted = false;
    }

    void PlayAnimation(string animationName, int layer = -1, float normalizedTime = float.NegativeInfinity)
    {
        if (animationName != lastAnimationName)
        {
            lastAnimationName = animationName;
            animator.Play(animationName, layer, normalizedTime);
        }
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
        ResetClimbing();
        SetWeapon(weaponType);
        Teleport(true, false);
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
        GameObject bullet = Instantiate(weaponsData[(int)WeaponTypes.MegaBuster].weaponPrefab);
        bullet.name = weaponsData[(int)WeaponTypes.MegaBuster].weaponPrefab.name + "(" + gameObject.name + ")";;
        bullet.transform.position = bulletShootPos.position;

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDamage(weaponsData[(int)WeaponTypes.MegaBuster].weaponDamage);
        bulletScript.SetSpeed(weaponsData[(int)WeaponTypes.MegaBuster].weaponVelocity.x);
        bulletScript.SetDirection(isFacingRight ? Vector2.right : Vector2.left);
        bulletScript.SetDestroyDelay(5f);
        bulletScript.Shoot();
        SoundManager.Instance.Play(weaponsData[(int)WeaponTypes.MegaBuster].weaponClip);
    }

    void ThrowBomb()
    {
        // create bomb from prefab gameobject
        GameObject bomb = Instantiate(weaponsData[(int)WeaponTypes.HyperBomb].weaponPrefab);
        bomb.name = weaponsData[(int)WeaponTypes.HyperBomb].weaponPrefab.name + "(" + gameObject.name + ")";
        bomb.transform.position = bulletShootPos.position;
        // set the bomb properties and throw it
        BombScript bombScript = bomb.GetComponent<BombScript>();
        bombScript.SetContactDamageValue(0);
        bombScript.SetExplosionDamageValue(weaponsData[(int)WeaponTypes.HyperBomb].weaponDamage);
        bombScript.SetExplosionDelay(3f);
        bombScript.SetCollideWithTags("Enemy");
        bombScript.SetDirection(isFacingRight ? Vector2.right : Vector2.left);
        bombScript.SetVelocity(weaponsData[(int)WeaponTypes.HyperBomb].weaponVelocity);
        bombScript.Bounces(true);
        bombScript.ExplosionEvent.AddListener(CanUseWeaponAgain);
        bombScript.Launch(false);
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

    public bool GetInvincible()
    {
        return isInvincible;
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
            ResetClimbing();
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
        FreezeInput(false);
        PlayAnimation("Player_Hit", -1, 0f);
        StartCoroutine(FlashAfterDamage());
    }

    private IEnumerator FlashAfterDamage()
    {
        float flashDelay = 0.0833f;
        // Material material = sprite.material;
        for (int i = 0; i < 10; i++)
        {
            // sprite.color = Color.clear;
            sprite.material.SetFloat("_Transparency", 0f);
            // sprite.material = null;
            yield return new WaitForSeconds(flashDelay);
            // sprite.color = Color.white;
            sprite.material.SetFloat("_Transparency", 1f);
            // sprite.material = material;
            yield return new WaitForSeconds(flashDelay);
        }
        Invincible(false);
    }

    private IEnumerator StartDeathAnimation(bool explode)
    {

        yield return new WaitForSeconds(0.5f);

        FreezeInput(true);
        FreezePlayer(true);

        if (explode)
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.name = explosionPrefab.name;
            explosion.transform.position = sprite.bounds.center;
            explosion.GetComponent<ExplosionController>().SetDestroyDelay(5f);
        }
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

    public void Die(bool explode = true)
    {
        GameManager.Instance.GameOver();
        // Invoke("StartDeathAnimation", 0.5f);
        StartCoroutine(StartDeathAnimation(explode));
    }

    public void FreezeInput(bool freeze)
    {
        freezeInput = freeze;
        if (freeze && !GameManager.Instance.InCameraTransition())
        {
            keyHorizontal = 0;
            keyVertical = 0;
            keyJump = false;
            keyShoot = false;
        }
    }

    public void HidePlayer(bool hide)
    {
        if (hide)
        {
            Debug.Log("Hide Player");            
            sprite.material.SetFloat("_Transparency", 0f);
        }
        else
        {
            sprite.material.SetFloat("_Transparency", 1f);
        }
    }

    public void Teleport(bool teleport, bool descend = true)
    {
        if(teleport)
        {
            isTeleporting = true;
            FreezeInput(true);
            PlayAnimation("Player_Teleport");
            originalConstraints = rb.constraints;
            teleportState = TeleportState.Landed;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (descend)
            {
                animator.speed = 0;
                gameObject.tag = "Untagged";
                gameObject.layer = LayerMask.NameToLayer("Teleport");
                teleportState = TeleportState.Descending;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, teleportSpeed);
            }
        } 
        else
        {
            isTeleporting = false;
            rb.constraints = originalConstraints;
            FreezeInput(false);
        }   
    }

    public void SetTeleportLanding(float landingY)
    {
        teleportLandingY = landingY;
    }

    public void TeleportAnimationSound()
    {
        SoundManager.Instance.Play(teleportSound);
    }

    public void TeleportAnimationEnd()
    {
        teleportState = TeleportState.Idle;
    }

        // wrapper for the StartedClimbingCo() coroutine below
    void StartedClimbing()
    {
        StartCoroutine(StartedClimbingCo());
    }

    // started climbing coroutine
    // (this gives us a delay from the ground check giving false positives)
    private IEnumerator StartedClimbingCo()
    {
        hasStartedClimbing = true;
        yield return new WaitForSeconds(0.1f);
        hasStartedClimbing = false;
    }

    // reset our ladder climbing variables and 
    // put back the animator speed and rigidbody type
    void ResetClimbing()
    {
        // reset climbing if we're climbing
        if (isClimbing)
        {
            isClimbing = false;
            atLaddersEnd = false;
            startedClimbTransition = false;
            finishedClimbTransition = false;
            animator.speed = 1;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // wrapper for the ClimbTransitionCo() coroutine below
    void ClimbTransition(bool movingUp)
    {
        StartCoroutine(ClimbTransitionCo(movingUp));
    }

    // climbing transition animation for when we move to the top of
    // the ladder or when we move down from the top of it
    private IEnumerator ClimbTransitionCo(bool movingUp)
    {
        // we don't want any player input during this
        FreezeInput(true);

        // flag to signal we're not done performing the transition
        finishedClimbTransition = false;

        // there are two positions, going up and going down
        Vector3 newPos = Vector3.zero;
        if (movingUp)
        {
            // moving up we transition the top offset amount
            // (it looks like his body is half above the the ladder top)
            newPos = new Vector3(ladder.posX, transformY + ladder.handlerTopOffset, 0);
        }
        else
        {
            // moving down we first reposition our y (~position at the end of the moving up transition)
            // then we transition down the top offset amount so looks like we're climbing down from the top(ish)
            transform.position = new Vector3(ladder.posX, ladder.posTopHandlerY - climbSpriteHeight + ladder.handlerTopOffset, 0);
            newPos = new Vector3(ladder.posX, ladder.posTopHandlerY - climbSpriteHeight, 0);
        }

        while (transform.position != newPos)
        {
            // we are going to move towards the new position playing our other climb animation (the bent over look)
            transform.position = Vector3.MoveTowards(transform.position, newPos, climbSpeed * Time.deltaTime);
            animator.speed = 1;
            PlayAnimation("Player_ClimbTop");
            yield return null;
        }

        // done climbing down so those other code blocks can work again
        isClimbingDown = false;

        // now we're signaling that we finished the climb transition
        finishedClimbTransition = true;

        // give the player back their input
        FreezeInput(false);
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
            animator.speed = 1;
            rb.constraints = originalConstraints;
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
