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

        // ѕринудительно обновл€ем полоску до 100% сразу
        UpdateHealthBar(boss.health, boss.maxHealth);

        currentBoss.OnHealthChanged.AddListener(UpdateHealthBar);
        currentBoss.OnDeath.AddListener(OnBossDeath);
    }

    public void Hide()
    {
        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged.RemoveListener(UpdateHealthBar);
            currentBoss.OnDeath.RemoveListener(OnBossDeath);
        }
        canvas.enabled = false;
        currentBoss = null;
    }

    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float percent = currentHealth / maxHealth;
        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * percent);
    }

    private void OnBossDeath(EnemyStats boss)
    {
        Hide();
    }
}