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
        if (hitInfo.CompareTag("Player"))
        {
            return; // Ignore collisions with the player
        }

        EnemyHealth enemy = hitInfo.GetComponent<EnemyHealth>();
        if(enemy != null)
        {
            enemy.TakeDamage(damage); 
        }

        Instantiate(impactEffect, transform.position, transform.rotation); // Create an impact effect at the collision point

        Debug.Log(hitInfo.name); // Log the name of the object the fireball collides with
        Destroy(gameObject);
    }
}
