using UnityEngine;

public class ExplosionController: MonoBehaviour
{
    Animator animator;
    SpriteRenderer sprite;
    int damage = 0;

    float destroyTimer;
    float destroyDelay = 2f;

    // freeze explosion on screen
    float animatorSpeed;
    Color explosionColor;
    bool freezeExplosion;

    string[] collideWithTags = {"Player"};

    void Awake()
    {
        // get components
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // init the timer with the delay
        SetDestroyDelay(destroyDelay);
    }

    void Update()
    {
        // if the explosion is frozen then don't allow it to destroy
        if (freezeExplosion) return;

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

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetDestroyDelay(float delay)
    {
        destroyDelay = delay;
        // set the timer in motion here
        // nothing triggers the timer to start elsewhere
        destroyTimer = delay;
    }

    public void SetCollideWithTags(params string[] other)
    {
        collideWithTags = other;
    }

    public void FreezeExplosion(bool freeze)
    {
        // freeze/unfreeze the explosions on screen
        // NOTE: this will be called from the GameManager but could be used in other scripts
        if (freeze)
        {
            freezeExplosion = true;
            animatorSpeed = animator.speed;
            animator.speed = 0;
        }
        else
        {
            freezeExplosion = false;
            animator.speed = animatorSpeed;
        }
    }

    public void HideExplosion(bool hide)
    {
        if (hide)
        {
            explosionColor = sprite.color;
            sprite.color = Color.clear;
        }
        else
        {
            sprite.color = explosionColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damage > 0)
        {
            foreach(string tag in collideWithTags)
            {
                if (collision.gameObject.CompareTag(tag))
                {
                    switch (tag)
                    {
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
                                player.HitSide(transform.position.x > player.transform.position.x);
                                player.TakeDamage(damage);
                            }
                            break;
                    }
                }            
            }
        }
    }
}
