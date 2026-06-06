using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    public void StartNewRun()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public int currentLevel = 1;
    public int maxLevels = 3;

    public void NextLevel()
    {
        // Сохраняем данные текущего уровня
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        Timer timer = FindObjectOfType<Timer>();
        LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();

        if (playerStats != null && timer != null && levelGen != null)
        {
            levelGen.timeSpentOnCurrentLevel = timer.StopAndSave();
            levelGen.damageTakenOnCurrentLevel = playerStats.totalDamageTaken;
            levelGen.healthPercentageOnCurrentLevel = playerStats.healthPercentage;
                  
            playerStats.ResetLevelStats();
            timer.ResetTimer();
        }

        // Сбрасываем флаг двери для нового уровня
        NextLevelDoor.IsActivatedForCurrentLevel = false;

        currentLevel++;
        if (currentLevel > maxLevels)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Если levelGen всё ещё null, пробуем найти ещё раз
        if (levelGen == null)
            levelGen = FindObjectOfType<LevelGenerator>();

        if (levelGen == null)
        {
            return;
        }

        if (levelGen.graphGenerator != null)
        {
            // Базовая установка диапазонов в зависимости от уровня
            switch (currentLevel)
            {
                case 2:
                    levelGen.graphGenerator.minRooms = 12;
                    levelGen.graphGenerator.maxRooms = 16;
                    break;
                case 3:
                    levelGen.graphGenerator.minRooms = 16;
                    levelGen.graphGenerator.maxRooms = 20;
                    break;
            }

            // Адаптивная коррекция количества комнат
            float adaptive = 1f;
            if (AdaptiveDifficulty.Instance != null)
                adaptive = AdaptiveDifficulty.Instance.currentAdaptiveMultiplier;

            int baseMin = levelGen.graphGenerator.minRooms;
            int baseMax = levelGen.graphGenerator.maxRooms;
            int newMin = Mathf.RoundToInt(baseMin * adaptive);
            int newMax = Mathf.RoundToInt(baseMax * adaptive);
            levelGen.graphGenerator.minRooms = Mathf.Max(6, newMin);
            levelGen.graphGenerator.maxRooms = Mathf.Max(8, newMax);
        }

        // Рассчитываем адаптивную сложность для следующего уровня (если есть данные)
        if (levelGen != null && AdaptiveDifficulty.Instance != null)
        {
            AdaptiveDifficulty.Instance.CalculateDifficultyForNextLevel(
                levelGen.timeSpentOnCurrentLevel,
                levelGen.damageTakenOnCurrentLevel,
                levelGen.healthPercentageOnCurrentLevel,
                currentLevel
            );
        }

        // Обновляем множитель этажа
        if (levelGen != null)
            levelGen.UpdateFloorMultiplier(currentLevel + 1);

        int newSeed = Random.Range(0, 1000);
        levelGen.GenerateNewLevel(newSeed);
    }

    public void ResetGameState()
    {
        currentLevel = 1;
        // Если нужно сбросить здоровье игрока и другие параметры
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.ResetHealth();
            playerStats.ResetLevelStats();
        }

        // Сброс адаптивной сложности
        if (AdaptiveDifficulty.Instance != null)
        {
            AdaptiveDifficulty.Instance.currentAdaptiveMultiplier = 1f;
            AdaptiveDifficulty.Instance.currentDifficultyTier = "Medium";
        }

        // Сброс флага двери
        NextLevelDoor.IsActivatedForCurrentLevel = false;
    }

    public void GameOver()
    {
        Debug.Log("Игра окончена");
    }
}