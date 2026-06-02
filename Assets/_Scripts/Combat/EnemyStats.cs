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

        if (isBoss)
        {
            NextLevelDoor door = FindObjectOfType<NextLevelDoor>(true);
            if (door != null) door.Activate();
        }

        Destroy(gameObject);
    }

    public void Initialize(float difficulty)
    {
        maxHealth = 50f + difficulty * 20f;
        health = maxHealth;
    }
}