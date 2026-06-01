using UnityEngine;
using UnityEngine.Events;

public class EnemyStats : MonoBehaviour, IDamageable
{
    public UnityEvent<EnemyStats> OnDeath;
    public UnityEvent<float, float> OnHealthChanged;

    public float maxHealth = 50f;
    public float health;
    public bool isBoss = false;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Враг получил урон " + amount + ". Осталось здоровья: " + health);

        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0)
        {
            Debug.Log("Враг умирает");
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Враг уничтожен");
        OnDeath?.Invoke(this);

        // Если это босс — активируем дверь перехода на следующий уровень
        if (isBoss)
        {
            NextLevelDoor door = FindObjectOfType<NextLevelDoor>(true);
            if (door != null)
            {
                door.Activate();
                Debug.Log("Дверь на следующий уровень активирована!");
            }
            else
            {
                Debug.LogWarning("NextLevelDoor не найден в сцене босса");
            }
        }

        Destroy(gameObject);
    }

    public void Initialize(float difficulty)
    {
        maxHealth = 50f + difficulty * 20f;
        health = maxHealth;
    }
}