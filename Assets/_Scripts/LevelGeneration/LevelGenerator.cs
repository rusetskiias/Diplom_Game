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

    private void Start()
    {
        if (graphGenerator == null)
        {
            graphGenerator = FindObjectOfType<GraphGenerator>();
        }
        int randomSeed = Random.Range(0, 100);
        Debug.Log($"Сид уровня: {randomSeed}");
        GenerateNewLevel(randomSeed);
    }

    // Генерация нового уровня
    public void GenerateNewLevel(int seed)
    {
        if (graphGenerator == null)
        {
            Debug.LogError("GraphGenerator не назначен!");
            return;
        }

        currentRooms = graphGenerator.GenerateLevel(seed);
        Debug.Log($"Сгенерировано {currentRooms.Count} комнат");

        // Загружаем стартовую комнату
        LoadStartRoom();
    }

    // Загрузить стартовую комнату
    private void LoadStartRoom()
    {
        if (currentRooms == null || currentRooms.Count == 0)
        {
            Debug.LogError("Нет комнат для загрузки!");
            return;
        }

        Room startRoom = currentRooms.Find(r => r.roomType == RoomType.Start);
        if (startRoom == null)
        {
            Debug.LogError("Стартовая комната не найдена!");
            return;
        }

        currentRoom = startRoom;
        

        // Наш новый метод. Direction.None означает, что для первой комнаты 
        // нам не нужно высчитывать противоположные двери (игрок просто начинает в ней)
        RoomLoader.Instance.TransitionToRoom(startRoom, Direction.None);

    }

    // Получить комнату по типу (для тестов)
    public Room GetRoomByType(RoomType type)
    {
        return currentRooms.Find(r => r.roomType == type);
    }
}
