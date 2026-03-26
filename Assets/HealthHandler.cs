using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    // Define the health changed event and handler delegate.
    public delegate void HealthChangedHandler(object source, float oldHealth, float newHealth);
    public event HealthChangedHandler OnHealthChanged;

    // Inspector-visible fields for health management.
    [SerializeField]
    private float maxHealth = 100f;

    [SerializeField]
    private float currentHealth = 100f;
    // Allow other scripts a readonly property to access current health
    public float CurrentHealth => currentHealth;

    // Test values for health adjustment.
    [SerializeField]
    private float testHealAmount = 5f;
    [SerializeField]
    private float testDamageAmount = -5f;

    // Method to change health, clamping the value between 0 and maxHealth.
    public void ChangeHealth(float amount)
    {
        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Fire off health change event.
        OnHealthChanged?.Invoke(this, oldHealth, currentHealth);
    }

    // Test code to change health with keyboard input.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            ChangeHealth(testHealAmount);

        if (Input.GetKeyDown(KeyCode.E))
            ChangeHealth(testDamageAmount);
    }
}