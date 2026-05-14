using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable
{
    public float health = 30f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Враг получил урон " + amount + ". Осталось здоровья: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Враг уничтожен");
        Destroy(gameObject);
    }
}