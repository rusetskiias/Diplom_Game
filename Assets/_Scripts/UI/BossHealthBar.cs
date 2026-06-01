using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public RectTransform fillRect;
    private EnemyStats currentBoss;
    private float maxWidth;
    private Canvas canvas;

    void Start()
    {
        maxWidth = fillRect.rect.width;
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }

    public void SetBoss(EnemyStats boss)
    {
        currentBoss = boss;
        canvas.enabled = true;
        currentBoss.OnHealthChanged.AddListener(UpdateHealthBar);
    }

    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float percent = currentHealth / maxHealth;
        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * percent);
    }
}