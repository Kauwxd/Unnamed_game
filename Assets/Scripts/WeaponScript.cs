using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;
    [SerializeField]
    private float timeBetweenFiring;
    [SerializeField]
    private bool canFire;

    private float timer;


    // Update is called once per frame
    void Update()
    {

            
        if (!canFire)
        {
            timer += Time.deltaTime;
            if (timer >= timeBetweenFiring)
            {
                canFire = true;
                timer = 0f;
            }
        }

        if (Input.GetButtonDown("Fire1") && canFire)
        {
            Shoot();
        }

       
    }

    void Shoot() //Shooting logic
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        soundManager.Instance.PlaySound("fireballSwoosh");
        canFire = false;
    }
}
