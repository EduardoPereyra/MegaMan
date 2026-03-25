using UnityEngine;

public class Bullet:MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    float destroyTime;

    bool freezeBullet = false;
    RigidbodyConstraints2D originalConstraints;

    public int damage = 1;

    [SerializeField] float speed = 5f;
    [SerializeField] Vector2 direction;
    [SerializeField] float destroyDelay = 2f;


    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update ()
    {
        if (freezeBullet) return;

        destroyTime -= Time.deltaTime;
        if (destroyTime < 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetDestroyDelay(float destroyDelay)
    {
        this.destroyDelay = destroyDelay;
    }

    public void Shoot()
    {
        spriteRenderer.flipX = direction.x < 0;
        rb.linearVelocity = direction * speed;
        destroyTime = destroyDelay;
    }

    public void Freeze(bool freeze)
    {
        if (freeze)
        {
            originalConstraints = rb.constraints;
            animator.speed = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.linearVelocity = Vector2.zero;
        } else
        {
            animator.speed = 1;
            rb.constraints = originalConstraints;
            rb.linearVelocity = direction * speed;
        }
        freezeBullet = freeze;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (
            // collision.gameObject.layer == LayerMask.NameToLayer("Ground") || 
        collision.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject, 0.02f);
        }
    }
}
