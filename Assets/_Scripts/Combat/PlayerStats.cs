using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Базовые параметры")]
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float speed = 5f;

    [Header("Временная неуязвимость")]
    [SerializeField] private float invincibilityDuration = 1f;

    private float currentHealth;
    private bool isInvincible = false;
    private float invincibilityEndTime;

    // Статистика для адаптивной сложности
    public int totalDamageTaken = 0;
    public float healthPercentage => currentHealth / maxHealth;

    public UnityEvent<float, float> OnHealthChanged;

    // ========== Публичные свойства для доступа из других скриптов ==========
    public float MaxHealth => maxHealth;
    public float Damage => damage;
    public float FireRate => fireRate;
    public float Speed => speed;
    public float CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible && Time.time < invincibilityEndTime) return;
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        totalDamageTaken += (int)amount;
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
       UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ========== Модификаторы статов ==========
    public void ModifyDamage(float delta)
    {
        damage += delta;
        if (damage < 1f) damage = 1f;
    }

    public void ModifyFireRate(float delta)
    {
        fireRate += delta;
        if (fireRate < 0.05f) fireRate = 0.05f;
    }

    public void ModifySpeed(float delta)
    {
        speed += delta;
        if (speed < 2f) speed = 2f;
    }

    public void ModifyMaxHealth(float delta)
    {
        maxHealth += delta;
        if (maxHealth < 10f) maxHealth = 10f;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetInvincibility()
    {
        isInvincible = false;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetLevelStats()
    {
        totalDamageTaken = 0;
    }
}