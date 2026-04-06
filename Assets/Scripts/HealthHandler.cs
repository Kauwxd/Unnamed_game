using UnityEngine;
using UnityEngine.UI;

public class HealthHandler : MonoBehaviour
{
    // Define the health changed event and handler delegate.
    public delegate void HealthChangedHandler(object source, float oldHealth, float newHealth);
    public event HealthChangedHandler OnHealthChanged;
    public Image healthBar;
    // Inspector-visible fields for health management.
    [SerializeField]
    private float maxHealth = 100f;

    [SerializeField]
    private float currentHealth = 100f;
    // Allow other scripts a readonly property to access current health
    public float CurrentHealth => currentHealth;

    public GameObject deathEffect;

    // Method to change health, clamping the value between 0 and maxHealth.
    public void ChangeHealth(float amount)
    {
        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        healthBar.fillAmount = currentHealth / maxHealth;

        // Fire off health change event.
        OnHealthChanged?.Invoke(this, oldHealth, currentHealth);
    }

    public void Heal(float HealAmount)
    {
       currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
       currentHealth += HealAmount;
       healthBar.fillAmount = currentHealth / maxHealth;
    }

    // Test code to change health with keyboard input.
    void Update()
    {
        Die();
    }

    void Die()
    {
        if (currentHealth <= 0)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
            Destroy(gameObject);

            //  Appplication.LoadLevel("GameOverScene"); Gameover scene is not implemented yet, so this line is commented out for now.
        }
    }
}