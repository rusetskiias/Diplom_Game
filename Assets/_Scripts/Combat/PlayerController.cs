using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float attackRange = 1.5f;
    public float attackDamage = 20f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        rb.linearVelocity = moveInput.normalized * speed;

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        Debug.Log("Атака!");

        // Находим все объекты с интерфейсом IDamageable
        MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour obj in allObjects)
        {
            // Пропускаем самого игрока
            if (obj == this) continue;
            if (obj == GetComponent<PlayerStats>()) continue;

            if (obj is IDamageable damageable)
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance <= attackRange)
                {
                    damageable.TakeDamage(attackDamage);
                }
            }
        }
    }
}
    