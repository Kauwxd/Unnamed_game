using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public GameObject deathEffect;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //Death effect missing so far
        Instantiate(deathEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
