using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float invincibilityDuration = 1f; // время неуязвимости после удара

    private float currentHealth;
    private bool isInvincible = false;
    private float invincibilityEndTime;

    public UnityEvent<float, float> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("Здоровье игрока: " + currentHealth);
    }

    public void TakeDamage(float amount)
    {
        // Если уже неуязвим — урон не проходит
        if (isInvincible && Time.time < invincibilityEndTime)
        {
            Debug.Log("Игрок неуязвим, урон не нанесён");
            return;
        }

        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log("Игрок получил урон " + amount + ". Осталось здоровья: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Включаем неуязвимость
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
}