using UnityEngine;

public class ScrewDriverController: MonoBehaviour
{
    Animator animator;
    BoxCollider boxCollider;
    Rigidbody2D rb;
    EnemyController enemyController;

    Bullet.BulletType bulletType;

    float openTimer;
    public float openDelay = 0.25f;

    bool doAttack;
    public float playerRange = 2f;

    public enum ScrewDriverColors { Blue, Orange}
    [SerializeField] ScrewDriverColors screwDriverColor = ScrewDriverColors.Blue;

    public enum ScrewDriverStates { Closed, Open }
    [SerializeField] ScrewDriverStates screwDriverState = ScrewDriverStates.Closed;

    public enum ScrewDriverOrientation { Top, Bottom, Left, Right }
    [SerializeField] ScrewDriverOrientation screwDriverOrientation = ScrewDriverOrientation.Bottom;

    [SerializeField] RuntimeAnimatorController racScrewDriverBlue;
    [SerializeField] RuntimeAnimatorController racScrewDriverOrange;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        boxCollider = enemyController.GetComponent<BoxCollider>();
        rb = enemyController.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SetColor(screwDriverColor);
        SetOrientation();
    }

    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        switch (screwDriverState)
        {
            case ScrewDriverStates.Closed:
                animator.Play("ScrewDriver_Closed");
                if (player && !doAttack)
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
                    if (distanceToPlayer <= playerRange)
                    {
                        doAttack = true;
                        openTimer = openDelay;
                    }
                }

                if (doAttack)
                {
                    openTimer -= Time.deltaTime;
                    if (openTimer <= 0f)
                    {
                        screwDriverState = ScrewDriverStates.Open;
                    }
                }
                break;

            case ScrewDriverStates.Open:
                animator.Play("ScrewDriver_Open");
                break;
        }
    }

    public void SetColor(ScrewDriverColors newColor)
    {
        screwDriverColor = newColor;
        SetBulletType();
        SetAnimatorController();
    }

    void SetAnimatorController()
    {
        switch (screwDriverColor)
        {
            case ScrewDriverColors.Blue:
                animator.runtimeAnimatorController = racScrewDriverBlue;
                break;
            case ScrewDriverColors.Orange:
                animator.runtimeAnimatorController = racScrewDriverOrange;
                break;
        }
    }

    void SetBulletType()
    {
        switch (screwDriverColor)
        {
            case ScrewDriverColors.Blue:
                bulletType = Bullet.BulletType.MiniBlue;
                break;
            case ScrewDriverColors.Orange:
                bulletType = Bullet.BulletType.MiniPink;
                break;
        }
    }

    void SetOrientation()
    {
        transform.rotation = Quaternion.identity;
        switch (screwDriverOrientation)
        {
            case ScrewDriverOrientation.Bottom:
                transform.Rotate(0f, 0f, 0f);
                break;
            case ScrewDriverOrientation.Top:
                transform.Rotate(180f, 0f, 0f);
                break;
            case ScrewDriverOrientation.Left:
                transform.Rotate(0f, 0f, -90f);
                break;
            case ScrewDriverOrientation.Right:
                transform.Rotate(0f, 0f, 90f);
                break;
        }
    }

    void Shoot()
    {
        GameObject[] bullets = new GameObject[5];        
        Vector2[] bulletDirections =
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(-0.75f, 0.75f),
            new Vector2(0.75f, 0.75f),
        };

        for (int i = 0; i < bulletDirections.Length; i++)
        {
            switch (screwDriverOrientation)
            {
                case ScrewDriverOrientation.Bottom:
                    break;
                case ScrewDriverOrientation.Top:
                    bulletDirections[i] = UtilityFunctions.RotateByAngle(bulletDirections[i], 180f);
                    break;
                case ScrewDriverOrientation.Left:
                    bulletDirections[i] = UtilityFunctions.RotateByAngle(bulletDirections[i], -900f);
                    break;
                case ScrewDriverOrientation.Right:
                    bulletDirections[i] = UtilityFunctions.RotateByAngle(bulletDirections[i], 90f);
                    break;
            }
            bullets[i] = Instantiate(enemyController.bulletPrefab);
            bullets[i].name = enemyController.bulletPrefab.name;
            bullets[i].transform.position = enemyController.bulletShootPos.transform.position;
            Bullet bulletcomponent = bullets[i].GetComponent<Bullet>();
            bulletcomponent.SetBulletType(bulletType);
            bulletcomponent.SetDamage(enemyController.bulletDamage);
            bulletcomponent.SetSpeed(enemyController.bulletSpeed);
            bulletcomponent.SetDirection(bulletDirections[i]);
            bulletcomponent.SetCollideWithTags(new string[] { "Player" });
            bulletcomponent.SetDestroyDelay(5f);
            bulletcomponent.Shoot();
        }

        SoundManager.Instance.Play(enemyController.shootSound);
    }

    private void OpenAnimationStop()
    {
        doAttack = false;
        screwDriverState = ScrewDriverStates.Closed;
    }
}
