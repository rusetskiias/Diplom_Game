using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float invincibilityDuration = 1f; // время неуязвимости после удара

    private float currentHealth;
    private bool isInvincible = false;
    private float invincibilityEndTime;

    public int totalDamageTaken = 0; // общий полученный урон на уровне
    public float healthPercentage => currentHealth / maxHealth; // процент здоровья

    public UnityEvent<float, float> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("Здоровье игрока: " + currentHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible && Time.time < invincibilityEndTime) return;
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        totalDamageTaken += (int)amount; // добавляем полученный урон

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
        else
        {
            isInvincible = true;
            invincibilityEndTime = Time.time + invincibilityDuration;
        }
    }

    private void Die()
    {
        Debug.Log("Игрок умер!");
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    // Метод для сброса неуязвимости (например, при переходе между комнатами)
    public void ResetInvincibility()
    {
        isInvincible = false;
    }

    public void ResetLevelStats()
    {
        totalDamageTaken = 0;
        // здоровье не сбрасываем, оно сохраняется между уровнями
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}