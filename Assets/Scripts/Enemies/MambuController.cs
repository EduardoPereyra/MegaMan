using UnityEngine;

public class MambuController: MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    EnemyController enemyController;

    bool isFacingRight;
    bool isShooting;
    float openTimer;
    float closedTimer;
    float shootTimer;

    public float moveSpeed = 1f;
    public float openDuration = 1f;
    public float closedDuration = 1f;
    public float shootDuration = 0.5f;
    public enum MambuState { Open, Closed }
    public MambuState currentState = MambuState.Closed;

    public enum MoveDirection { Left, Right }
    [SerializeField] public MoveDirection moveDirection = MoveDirection.Left;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        rb = enemyController.GetComponent<Rigidbody2D>();

        isFacingRight = true;
        if (moveDirection == MoveDirection.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }

        isShooting = false;

        if (currentState == MambuState.Closed)
        {
            closedTimer = closedDuration;
        } else if (currentState == MambuState.Open)
        {
            openTimer = openDuration;
        }
    }   

    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            return;
        }

        switch (currentState)
        {
            case MambuState.Closed:
                animator.Play("Mambu_Closed");
                rb.linearVelocity = new Vector2((isFacingRight ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
                closedTimer -= Time.deltaTime;
                if (closedTimer < 0f)                
                {
                    currentState = MambuState.Open;
                    openTimer = openDuration;
                    shootTimer = shootDuration;
                }
                break;
            case MambuState.Open:
                animator.Play("Mambu_Open");
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                shootTimer -= Time.deltaTime;
                if (shootTimer < 0f && !isShooting)
                {
                    isShooting = true;
                    Shoot();
                }
                openTimer -= Time.deltaTime;
                if (openTimer < 0f)
                {
                    currentState = MambuState.Closed;
                    closedTimer = closedDuration;
                    isShooting = false;
                }
                break;

        }
    }

    public void SetMoveDirection(MoveDirection direction)
    {
        moveDirection = direction;
        if ((moveDirection == MoveDirection.Left && isFacingRight) || (moveDirection == MoveDirection.Right && !isFacingRight))
        {
            enemyController.Flip();
            isFacingRight = !isFacingRight;
        }
    }

    private void Shoot()
    {
        GameObject[] bullets = new GameObject[8];
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
            new Vector2(1, 1).normalized,
            new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized,
            new Vector2(-1, -1).normalized,
        };

        for (int i = 0; i < directions.Length; i++)
        {
            bullets[i] = Instantiate(enemyController.bulletPrefab);
            bullets[i].name = enemyController.bulletPrefab.name;
            bullets[i].transform.position = enemyController.bulletShootPos.transform.position;
            Bullet bulletComponent = bullets[i].GetComponent<Bullet>();
            bulletComponent.SetBulletType(Bullet.BulletType.MiniPink);
            bulletComponent.SetDamage(enemyController.bulletDamage);
            bulletComponent.SetSpeed(enemyController.bulletSpeed);
            bulletComponent.SetDirection(directions[i]);
            bulletComponent.SetCollideWithTags(new string[] { "Player" });
            bulletComponent.SetDestroyDelay(5f);
            bulletComponent.Shoot();
        }

        SoundManager.Instance.Play(enemyController.shootSound);
    }

    private void StartInvincibleAnimation()
    {
        enemyController.SetInvincible(true);
    }

    private void StopInvincibleAnimation()
    {
        enemyController.SetInvincible(false);
    }
}
