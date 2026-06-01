using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;
    public float damage = 10f;
    public float attackCooldown = 1f; // секунд между атаками

    private Transform player;
    private float lastAttackTime;
    private bool isTouchingPlayer = false;
    private PlayerStats playerStats;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Атака при касании (с задержкой)
        if (isTouchingPlayer && playerStats != null)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                playerStats.TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;
            // Первая атака сразу
            if (playerStats != null && Time.time >= lastAttackTime + attackCooldown)
            {
                playerStats.TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }
}