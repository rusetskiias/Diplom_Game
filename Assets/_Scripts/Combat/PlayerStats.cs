using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Здоровье игрока: " + currentHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Игрок получил урон" + amount + ". Осталось здоровья: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
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
}