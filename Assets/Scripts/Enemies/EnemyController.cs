using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class EnemyController: MonoBehaviour
{
    Animator animator;
    BoxCollider2D boxCollider;
    Rigidbody2D rb;
    SpriteRenderer sprite;


    bool isInvincible;

    GameObject explodeEffect;

    RigidbodyConstraints2D originalConstraints;
    public bool freezeEnemy;

    public bool hasHealthBar;

    [Header("Enemy Settings")]
    public int scorePoints = 500;
    public int currentHealth;
    public int maxHealth = 1;
    public int contactDamage = 1;
    public int explosionDamage = 0;
    public int bulletDamage = 1;
    public float bulletSpeed = 3f;

    [Header("Bonus Item drop settings")]
    public ItemsController.ItemType bonusItemType;
    public ItemsController.BonusBallColor bonusBallColor;
    public ItemsController.WeaponPartColor weaponPartColor;
    public float bonusDestroyDelay = 5f;
    public Vector2 bonusVelocity = new(0, 3f);
    public UnityAction BonusItemAction;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip blockAttackSound;
    public AudioClip energyFullSound;

    [Header("Position and Prefabs")]
    public GameObject bulletShootPos;
    public GameObject bulletPrefab;
    public GameObject explosionPrefab;
    public float explodeEffectDestroyDelay = 2f;

    [Header("Enemy Events")]
    public UnityEvent TakeDamageEvent;
    public UnityEvent DefeatEvent;
    

    void Start()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    public void Flip()
    {
        transform.Rotate(0f, 180f, 0f);
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            TakeDamageEvent.Invoke();
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            if(hasHealthBar && UIEnergyBars.Instance)
            {
                UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.EnemyHealth, currentHealth / (float)maxHealth);
            }
            SoundManager.Instance.Play(hitSound);
            if (currentHealth <= 0)
            {
                Die();
            }
        } else
        {
            SoundManager.Instance.Play(blockAttackSound);
        }
    }

    void StartDeathAnimation()
    {
        explodeEffect = Instantiate(explosionPrefab);
        explodeEffect.name = explosionPrefab.name;
        explodeEffect.transform.position = sprite.bounds.center;
        explodeEffect.GetComponent<ExplosionController>().SetDamage(explosionDamage);
        Destroy(explodeEffect, explodeEffectDestroyDelay);

        GameObject bonusItemsPrefab = GameManager.Instance.GetBonusItem(bonusItemType);
        if (bonusItemsPrefab)
        {
            GameObject bonusItem = Instantiate(bonusItemsPrefab);
            bonusItem.name = bonusItemsPrefab.name;
            bonusItem.transform.position = explodeEffect.transform.position;
            bonusItem.GetComponent<ItemsController>().Animate(true);
            bonusItem.GetComponent<ItemsController>().SetDestroyDelay(bonusDestroyDelay);
            bonusItem.GetComponent<ItemsController>().SetBonusBallColor(bonusBallColor);
            bonusItem.GetComponent<ItemsController>().SetWeaponPartColor(weaponPartColor);
            if (BonusItemAction != null)
            {
                bonusItem.GetComponent<ItemsController>().BonusItemEvent.AddListener(BonusItemAction);
            }

            bonusItem.GetComponent<Rigidbody2D>().linearVelocity = bonusVelocity;
        }
    }

    void EndDeathAnimation()
    {
        Destroy(explodeEffect);
    }

    void Die()
    {
        DefeatEvent.Invoke();
        StartDeathAnimation();
        Destroy(gameObject);
        GameManager.Instance.AddScorePoints(scorePoints);
    }
    
    public void FreezeEnemy(bool freeze)
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
        freezeEnemy = freeze;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            player.HitSide(collision.transform.position.x > transform.position.x);
            player.TakeDamage(contactDamage);
        }
    }
}
