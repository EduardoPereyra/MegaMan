using UnityEngine;

public class ExplosionController: MonoBehaviour
{
    int damage = 0;

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damage > 0)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                player.HitSide(transform.position.x > player.transform.position.x);
                player.TakeDamage(damage);
            }            
        }
    }
}
