using UnityEngine;

public class NextLevelDoor : MonoBehaviour
{
    public static bool IsActivatedForCurrentLevel = false; // статический флаг для текущего уровня

    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null) spriteRenderer.enabled = IsActivatedForCurrentLevel;
        if (doorCollider != null) doorCollider.enabled = IsActivatedForCurrentLevel;
    }

    public void Activate()
    {
        IsActivatedForCurrentLevel = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log("Дверь стала видимой");
        }
        if (doorCollider != null)
            doorCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.NextLevel();
        }
    }
}