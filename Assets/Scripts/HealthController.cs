using System;

internal class HealthController
{
    public Action OnHealthDepleted;
    private readonly float maxHealth;
    private float currentHealth;
    public HealthController(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = MathF.Max(currentHealth - damage, 0f);
        if(currentHealth == 0f)
        {
            OnHealthDepleted?.Invoke();
        }
    }

    public void AddHealth(float health)
    {
        currentHealth = MathF.Min(currentHealth + health, maxHealth);
    }
}

