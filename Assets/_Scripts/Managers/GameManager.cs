using UnityEngine;

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
        Debug.Log("Новый забег начат");
    }

    public int currentLevel = 1;
    public int maxLevels = 3;

    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel > maxLevels)
        {
            Debug.Log("ПОБЕДА! Игра пройдена.");
            // TODO: показать UI победы
            return;
        }

        LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();
        if (levelGen != null && levelGen.graphGenerator != null)
        {
            // Устанавливаем диапазоны в зависимости от уровня
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
                default:
                    // 1 уровень уже задан (8-12)
                    break;
            }
        }

        int newSeed = Random.Range(0, 1000);
        levelGen.GenerateNewLevel(newSeed);
    }

    public void GameOver()
    {
        Debug.Log("Игра окончена");
    }
}