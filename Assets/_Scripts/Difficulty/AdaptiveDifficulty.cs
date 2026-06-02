using UnityEngine;

public class AdaptiveDifficulty : MonoBehaviour
{
    public static AdaptiveDifficulty Instance { get; private set; }

    [Header("Пороговые значения (нормальная игра)")]
    public float timeThresholdNormal = 180f;      // 3 минуты – норма
    public float damageThresholdNormal = 30f;     // урон 30 HP – норма
    public float healthThresholdNormal = 0.7f;    // 70% здоровья – норма

    [Header("Коэффициенты адаптации")]
    public float weakMultiplier = 0.8f;   // для низкой сложности (легче)
    public float mediumMultiplier = 1.0f;
    public float strongMultiplier = 1.2f; // для высокой сложности (тяжелее)

    [Header("Коэффициенты роста по этажам (накопленные)")]
    public float floorGrowth = 0.1f;      // +10% к здоровью врагов за каждый следующий этаж

    // Текущий результат для следующего уровня
    public float currentAdaptiveMultiplier = 1.0f;
    public string currentDifficultyTier = "Medium";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Вызывается после завершения уровня (в GameManager.NextLevel)
    public void CalculateDifficultyForNextLevel(float timeSpent, int damageTaken, float healthPercentage, int currentLevel)
    {
        // Для первого уровня (currentLevel == 1) – всегда Medium, база не меняется
        if (currentLevel == 1)
        {
            currentAdaptiveMultiplier = mediumMultiplier;
            currentDifficultyTier = "Medium";
            Debug.Log($"Адаптивная сложность (исходный уровень): {currentDifficultyTier}, множитель {currentAdaptiveMultiplier}");
            return;
        }

        // Нормализуем показатели относительно порогов
        float timeScore = timeThresholdNormal / Mathf.Max(timeSpent, 0.01f);
        // Чем меньше время, тем больше score (быстрее = лучше)
        if (timeScore > 2f) timeScore = 2f;

        float damageScore = damageThresholdNormal / Mathf.Max(damageTaken, 1f);
        // Чем меньше урон, тем больше score

        float healthScore = healthPercentage / healthThresholdNormal;
        // Чем больше здоровья осталось, тем больше score

        // Общая производительность (среднее геометрическое трёх показателей)
        float performance = Mathf.Pow(timeScore * damageScore * healthScore, 1f / 3f);

        // Определяем tier
        if (performance <= 0.7f)
        {
            currentAdaptiveMultiplier = weakMultiplier;
            currentDifficultyTier = "Weak";
        }
        else if (performance >= 1.3f)
        {
            currentAdaptiveMultiplier = strongMultiplier;
            currentDifficultyTier = "Strong";
        }
        else
        {
            currentAdaptiveMultiplier = mediumMultiplier;
            currentDifficultyTier = "Medium";
        }

        Debug.Log($"Адаптивная сложность на основе уровня {currentLevel}: {currentDifficultyTier} (performance={performance:F2}), множитель {currentAdaptiveMultiplier}");
    }
}