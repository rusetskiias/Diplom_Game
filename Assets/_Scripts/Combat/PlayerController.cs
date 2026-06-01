using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootCooldown = 0.2f; // частота стрельбы (при зажатии)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float lastShootTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.parent = transform;
            fp.transform.localPosition = Vector3.zero;
            firePoint = fp.transform;
        }
    }

    void Update()
    {
        // Движение ТОЛЬКО на WASD
        moveInput.x = 0f;
        moveInput.y = 0f;

        if (Input.GetKey(KeyCode.W)) moveInput.y = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput.y = -1f;
        if (Input.GetKey(KeyCode.A)) moveInput.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput.x = 1f;

        rb.linearVelocity = moveInput.normalized * speed;

        // Стрельба на стрелки
        HandleArrowShooting();
    }

    void HandleArrowShooting()
    {
        Vector2 shootDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
            shootDirection = Vector2.left;
        else if (Input.GetKey(KeyCode.RightArrow))
            shootDirection = Vector2.right;
        else if (Input.GetKey(KeyCode.UpArrow))
            shootDirection = Vector2.up;
        else if (Input.GetKey(KeyCode.DownArrow))
            shootDirection = Vector2.down;
        else
            return;

        //Debug.Log($"Стрелка нажата! Направление: {shootDirection}");

        if (Time.time >= lastShootTime + shootCooldown)
        {
            //Debug.Log("Вызываем Shoot()");
            Shoot(shootDirection);
            lastShootTime = Time.time;
        }
        else
        {
            //Debug.Log($"На перезарядке: {Time.time} >= {lastShootTime + shootCooldown}");
        }
    }

    void Shoot(Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("projectilePrefab не назначен!");
            return;
        }

        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(direction);
        }
    }
}