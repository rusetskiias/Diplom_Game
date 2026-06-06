using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    private float damage;
    private Vector2 direction;
    private PlayerStats playerStats;

    public void Initialize(Vector2 targetDirection)
    {
        direction = targetDirection.normalized;

        // Получаем урон из статов игрока при создании снаряда
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                damage = playerStats.Damage;
            }
            else
            {
                damage = 20f; // значение по умолчанию, если не найден PlayerStats
            }
        }
        else
        {
            damage = 20f; // значение по умолчанию, если не найден игрок
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Уничтожаем только при попадании во врага или стену
        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}