using UnityEngine;

public class NextLevelDoor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Изначально дверь не видна и неактивна
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (doorCollider != null) doorCollider.enabled = false;
    }

    public void Activate()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (doorCollider != null) doorCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.NextLevel();
        }
    }
}