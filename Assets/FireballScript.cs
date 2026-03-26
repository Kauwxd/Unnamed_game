using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireballScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float speed = 20f; // Speed of the fireball
    public Rigidbody2D rb;



    void Start()
    {
        rb.linearVelocity = transform.right * speed; // Move the fireball in the direction it's facing
    }
    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Debug.Log(hitInfo.name); // Log the name of the object the fireball collides with
        Destroy(gameObject);
    }
}
