using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 20f;

    private Vector2 direction;

    public void Initialize(Vector2 targetDirection)
    {
        direction = targetDirection.normalized;
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