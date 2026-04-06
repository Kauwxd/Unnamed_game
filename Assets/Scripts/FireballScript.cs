using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FireballScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float speed = 20f; // Speed of the fireball
    public Rigidbody2D rb;
    public int damage = 35; // Damage dealt by the fireball
    public GameObject impactEffect; // Effect to instantiate on impact
    [SerializeField] private float AutoDestroyTimer = 10f;
    private float timer; // Time after which the bullet will be destroyed
    private GameObject owner;

    public void SetOwner(GameObject shooter)
    {
        owner = shooter;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > AutoDestroyTimer)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb.linearVelocity = transform.right * speed; // Move the fireball in the direction it's facing
    }
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // IGNORE owner
        if (hitInfo.gameObject == owner)
            return;

        // DAMAGE PLAYER
        HealthHandler player = hitInfo.GetComponent<HealthHandler>();
        if (player != null)
        {
            player.ChangeHealth(-damage);
        }

        // DAMAGE ENEMY
        EnemyHealth enemy = hitInfo.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Instantiate(impactEffect, transform.position, transform.rotation);
        soundManager.Instance.PlaySound("fireballImpact");
        Debug.Log("Fireball hit: " + hitInfo.name);
        Destroy(gameObject);
    }
}
