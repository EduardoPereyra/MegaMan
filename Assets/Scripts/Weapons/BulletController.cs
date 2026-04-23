using UnityEngine;

public class Bullet:MonoBehaviour
{
    Animator animator;
    CircleCollider2D circleCollider;
    Rigidbody2D rb;
    SpriteRenderer sprite;

    float destroyTimer;

    Color bulletColor;
    bool freezeBullet = false;
    RigidbodyConstraints2D originalConstraints;

    public int damage = 1;

    [SerializeField] float speed = 5f;
    [SerializeField] Vector2 direction;
    [SerializeField] float destroyDelay = 2f;
    [SerializeField] string[] collideWithTags = {"Enemy"};
    
    public enum BulletType
    {
        Default,
        MiniBlue,
        MiniGreen,
        MiniOrange,
        MiniPink,
        MiniRed
    }
    [SerializeField] BulletType bulletType = BulletType.Default;

    [System.Serializable]
    public struct BulletStruct
    {
        public Sprite sprite;
        public float radius;
        public Vector3 scale;
    }
    [SerializeField] BulletStruct[] bulletData;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        SetBulletType(bulletType);
    }

    void Update ()
    {
        if (freezeBullet) return;

        if (destroyDelay > 0)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer < 0f)
            {
                Destroy(gameObject);
            }
        }

    }

    public void SetBulletType(BulletType type)
    {
        sprite.sprite = bulletData[(int)type].sprite;
        circleCollider.radius = bulletData[(int)type].radius;
        transform.localScale = bulletData[(int)type].scale;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
        if (direction.x > 0)
        {
            transform.Rotate(0, 180, 0);
        }
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetDestroyDelay(float destroyDelay)
    {
        this.destroyDelay = destroyDelay;
    }

    public void SetCollideWithTags(string[] tags)
    {
        collideWithTags = tags;
    }

    public void Shoot()
    {
        rb.linearVelocity = direction * speed;
        destroyTimer = destroyDelay;
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

    public void HideBullet(bool hide)
    {
        if (hide)
        {
            bulletColor = sprite.color;
            sprite.color = Color.clear;
        }
        else
        {
            sprite.color = bulletColor;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (string tag in collideWithTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                switch(tag) {
                    case "Enemy":
                        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                        if (enemy)
                        {
                            enemy.TakeDamage(damage);
                        }
                        break;
                    case "Player":
                        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                        if (player)
                        {
                            player.HitSide(transform.position.x < player.transform.position.x);
                            player.TakeDamage(damage);
                        }
                        break;
                }
                Destroy(gameObject, 0.01f);
            }
        }
    }
}
