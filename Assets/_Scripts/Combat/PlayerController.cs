using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float lastShootTime;
    private PlayerStats playerStats;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();

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

        rb.linearVelocity = moveInput.normalized * playerStats.Speed;

        // Стрельба на стрелки
        HandleArrowShooting();
    }

    void HandleArrowShooting()
    {
        if (playerStats == null) return;

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
             
        // Используем FireRate из PlayerStats, а не shootCooldown
        if (Time.time >= lastShootTime + playerStats.FireRate)
        {
            Shoot(shootDirection);
            lastShootTime = Time.time;
        }
    }

    void Shoot(Vector2 direction)
    {
        if (projectilePrefab == null)
        {
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