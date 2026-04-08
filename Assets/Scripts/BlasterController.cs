using UnityEngine;

public class BlasterController: MonoBehaviour
{
    Animator animator;
    BoxCollider2D boxCollider;
    Rigidbody2D rb;
    EnemyController enemyController;

    int bulletIndex = 0;

    Bullet.BulletType bulletType;

    float closedTimer;
    public float closedDuration = 2f;

    bool doAttack;
    public float playerRange = 2f;

    public enum BlasterColors
    {
        Blue,
        Orange,
        Red
    };

    [SerializeField] BlasterColors blasterColor = BlasterColors.Blue;

    public enum BlasterState { Closed, Open };
    [SerializeField] BlasterState blasterState = BlasterState.Closed;

    public enum BlasterOrientation { Top, Bottom, Left, Right };
    [SerializeField] BlasterOrientation blasterOrientation = BlasterOrientation.Left;

    [SerializeField] RuntimeAnimatorController racBlasterBlue;
    [SerializeField] RuntimeAnimatorController racBlasterOrange;
    [SerializeField] RuntimeAnimatorController racBlasterRed;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        boxCollider = enemyController.GetComponent<BoxCollider2D>();
        rb = enemyController.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SetColor(blasterColor);
        SetOrientation();
    }

    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        switch (blasterState)
        {
            case BlasterState.Closed:
                animator.Play("Blaster_Closed");
                if (player && !doAttack)
                {
                    float distance = Vector2.Distance(transform.position, player.transform.position);
                    if (distance <= playerRange)
                    {
                        doAttack = true;
                        closedTimer = closedDuration;
                    }
                }

                if (doAttack)
                {
                    closedTimer -= Time.deltaTime;
                    if (closedTimer <= 0f)
                    {
                        blasterState = BlasterState.Open;
                    }
                }
                break;
            case BlasterState.Open:
                animator.Play("Blaster_Open");
                break;
        }
    }

    public void SetColor(BlasterColors color)
    {
        blasterColor = color;
        SetBulletType();
        SetAnimatorController();
    }

    void SetAnimatorController()
    {
        switch (blasterColor)
        {
            case BlasterColors.Blue:
                animator.runtimeAnimatorController = racBlasterBlue;
                break;
            case BlasterColors.Orange:
                animator.runtimeAnimatorController = racBlasterOrange;
                break;
            case BlasterColors.Red:
                animator.runtimeAnimatorController = racBlasterRed;
                break;
        }
    }

    void SetBulletType()
    {
        switch (blasterColor)
        {
            case BlasterColors.Blue:
                bulletType = Bullet.BulletType.MiniBlue;
                break;
            case BlasterColors.Orange:
                bulletType = Bullet.BulletType.MiniPink;
                break;
            case BlasterColors.Red:
                bulletType = Bullet.BulletType.MiniRed;
                break;
        }
    }

    private void SetOrientation()
    {
        transform.rotation = Quaternion.identity;

        switch (blasterOrientation)
        {
            case BlasterOrientation.Top:
                transform.Rotate(0f, 0f, -90f);
                break;
            case BlasterOrientation.Bottom:
                transform.Rotate(0f, 0f, 90f);
                break;
            case BlasterOrientation.Left:
                transform.Rotate(0f, 0f, 0f);
                break;
            case BlasterOrientation.Right:
                transform.Rotate(0f, 180f, 0f);
                break;
        }
    }

    private void Shoot()
    {
        GameObject bullet;
        Vector2[] bulletDirections =
        {
            new Vector2(0.75f, 0.75f), 
            new Vector2(1f, 0.15f), 
            new Vector2(1f, -0.15f), 
            new Vector2(0.75f, -0.75f),
        };

        switch (blasterOrientation)
        {
            case BlasterOrientation.Left:
                break;
            case BlasterOrientation.Right:
                bulletDirections[bulletIndex].x *= -1;
                break;
            case BlasterOrientation.Bottom:
                bulletDirections[bulletIndex] = UtilityFunctions.RotateByAngle(bulletDirections[bulletIndex], 90f);
                break;
            case BlasterOrientation.Top:
                bulletDirections[bulletIndex] = UtilityFunctions.RotateByAngle(bulletDirections[bulletIndex], -90f);
                break;
        }
        bullet = Instantiate(enemyController.bulletPrefab);
        bullet.name = enemyController.bulletPrefab.name;
        bullet.transform.position = enemyController.bulletShootPos.transform.position;
        Bullet bulletcomponent = bullet.GetComponent<Bullet>();
        bulletcomponent.SetBulletType(bulletType);
        bulletcomponent.SetDamage(enemyController.bulletDamage);
        bulletcomponent.SetSpeed(enemyController.bulletSpeed);
        bulletcomponent.SetDirection(bulletDirections[bulletIndex]);
        bulletcomponent.SetCollideWithTags(new string[] { "Player" });
        bulletcomponent.SetDestroyDelay(5f);
        bulletcomponent.Shoot();

        if(++bulletIndex > bulletDirections.Length - 1)
        {
            bulletIndex = 0;
        }

        SoundManager.Instance.Play(enemyController.shootSound);
    }

    private void InvencibleAnimationStart()
    {
        enemyController.SetInvincible(true);
    }

    private void OpenAnimationStart()
    {
        enemyController.SetInvincible(false);
    }

    private void OpenAnimationEnd()
    {
        doAttack = false;
        blasterState = BlasterState.Closed;
    }
}
