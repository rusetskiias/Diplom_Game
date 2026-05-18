using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public RectTransform fillRect;
    public PlayerStats playerStats;
    private float maxWidth;

    void Start()
    {
        maxWidth = fillRect.rect.width;

        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        playerStats.OnHealthChanged.AddListener(UpdateHealthBar);
    }

    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float percent = currentHealth / maxHealth;
        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * percent);
    }
}