using UnityEngine;

public class ExplosionController: MonoBehaviour
{
    int damage = 0;

    string[] collideWithTags = {"Player"};

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetCollideWithTags(params string[] other)
    {
        collideWithTags = other;
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
