using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    [Header("Компоненты")]
    public GraphGenerator graphGenerator;
    

    [Header("Результат генерации")]
    public List<Room> currentRooms;
    public Room currentRoom;

    public float timeSpentOnCurrentLevel;

    
    public int damageTakenOnCurrentLevel;
    public float healthPercentageOnCurrentLevel;
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

    public float floorMultiplier = 1.0f;   // 1.0 для 1 этажа, 1.1 для 2, 1.2 для 3

    // Метод для обновления множителя этажа (вызывать при переходе на следующий уровень)
    public void UpdateFloorMultiplier(int newLevel)
    {
        floorMultiplier = 1f + (newLevel - 1) * AdaptiveDifficulty.Instance.floorGrowth;
    }


    private void Start()
    {
        if (graphGenerator == null)
        {
            graphGenerator = FindObjectOfType<GraphGenerator>();
        }
        int randomSeed = Random.Range(0, 1000);
       
        
        GenerateNewLevel(randomSeed);
    }
    public void ApplyAdaptiveSettings()
    {
        float adaptive = AdaptiveDifficulty.Instance.currentAdaptiveMultiplier;
        // Корректируем minRooms/maxRooms на основе текущих базовых значений для этажа
        int baseMin = graphGenerator.minRooms;
        int baseMax = graphGenerator.maxRooms;
        int newMin = Mathf.RoundToInt(baseMin * adaptive);
        int newMax = Mathf.RoundToInt(baseMax * adaptive);
        graphGenerator.minRooms = Mathf.Max(6, newMin);
        graphGenerator.maxRooms = Mathf.Max(8, newMax);
    }

    // Генерация нового уровня
    public void GenerateNewLevel(int seed)
    {
        if (graphGenerator == null)
        {
            return;
        }

        currentRooms = graphGenerator.GenerateLevel(seed);
        

        // Загружаем стартовую комнату
        LoadStartRoom();
        
    }

    // Загрузить стартовую комнату
    private void LoadStartRoom()
    {
        if (currentRooms == null || currentRooms.Count == 0)
        {
            
            return;
        }

        Room startRoom = currentRooms.Find(r => r.roomType == RoomType.Start);
        if (startRoom == null)
        {
            return;
        }

        currentRoom = startRoom;
        RoomLoader.Instance.TransitionToRoom(startRoom, Direction.None);
    }

    // Получить комнату по типу (для тестов)
    public Room GetRoomByType(RoomType type)
    {
        return currentRooms.Find(r => r.roomType == type);
    }
}
