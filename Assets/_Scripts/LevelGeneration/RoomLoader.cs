using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomLoader : MonoBehaviour
{
    public static RoomLoader Instance { get; private set; }

    private Room pendingRoom;
    private Direction incomingDirection = Direction.None;
    private string currentSceneName;

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

    // Загружаем комнату (первый раз, без перехода)
    public void LoadFirstRoom(Room targetRoom)
    {
        if (targetRoom == null)
        {
            Debug.LogError("LoadFirstRoom: targetRoom == null!");
            return;
        }

        pendingRoom = targetRoom;
        incomingDirection = Direction.None;
        Debug.Log($"LoadFirstRoom: pendingRoom = {pendingRoom.roomType}, позиция {pendingRoom.gridPosition}");

        string sceneName = GetSceneNameByRoomType(targetRoom.roomType);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        currentSceneName = sceneName;
    }

    // Загружаем комнату через дверь
    public void TransitionToRoom(Room targetRoom, Direction fromDirection)
    {
        if (targetRoom == null)
        {
            Debug.LogError("TransitionToRoom: targetRoom == null!");
            return;
        }

        pendingRoom = targetRoom;
        incomingDirection = fromDirection;
        Debug.Log($"TransitionToRoom: pendingRoom = {pendingRoom.roomType}, fromDirection = {fromDirection}");

        string sceneName = GetSceneNameByRoomType(targetRoom.roomType);

        if (!string.IsNullOrEmpty(currentSceneName))
        {
            SceneManager.UnloadSceneAsync(currentSceneName);
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        currentSceneName = sceneName;
    }

    private string GetSceneNameByRoomType(RoomType type)
    {
        switch (type)
        {
            case RoomType.Start: return "StartRoom";
            case RoomType.Combat: return "CombatRoom";
            case RoomType.Shop: return "ShopRoom";
            case RoomType.Gold: return "GoldRoom";
            case RoomType.Boss: return "BossRoom";
            default: return "CombatRoom";
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Игнорируем главную сцену
        if (sceneName == "v2.5" || sceneName == "MainScene")
        {
            return;
        }

        if (sceneName != "StartRoom" && sceneName != "CombatRoom" &&
            sceneName != "ShopRoom" && sceneName != "GoldRoom" && sceneName != "BossRoom")
        {
            return;
        }

        if (pendingRoom == null)
        {
            Debug.LogError($"OnSceneLoaded: pendingRoom == NULL! Сцена {scene.name} загружена.");
            return;
        }

        // ВАЖНО: запускаем спавн игрока
        StartCoroutine(PlacePlayerAtCorrectDoor());

        // Ищем компонент Room СТРОГО на только что загруженной сцене
        GameObject[] rootObjects = scene.GetRootGameObjects();
        Room sceneRoom = null;

        foreach (GameObject root in rootObjects)
        {
            sceneRoom = root.GetComponentInChildren<Room>();
            if (sceneRoom != null) break;
        }

        if (sceneRoom != null)
        {
            Room modelRoom = pendingRoom;
            sceneRoom.isCleared = modelRoom.isCleared;

            // КРИТИЧЕСКИ ВАЖНАЯ СТРОКА: Переносим маску выходов из генератора в физический объект сцены
            sceneRoom.availableExits = modelRoom.availableExits;
            sceneRoom.roomType = modelRoom.roomType;

            DoorLinker linker = sceneRoom.GetComponent<DoorLinker>();
            if (linker == null) linker = sceneRoom.gameObject.AddComponent<DoorLinker>();

            linker.Initialize(sceneRoom, FindObjectOfType<GraphGenerator>(), modelRoom);

            // Спавн врагов
            EnemySpawner spawner = sceneRoom.GetComponent<EnemySpawner>();
            if (spawner == null)
            {
                spawner = sceneRoom.gameObject.AddComponent<EnemySpawner>();
            }

            // TODO: передавать актуальную сложность из AdaptiveDifficulty
            float currentDifficulty = 0;
            spawner.Initialize(modelRoom, currentDifficulty);

            // ========== НОВЫЙ КОД: Визуализация дверей ==========
            RoomVisualizer visualizer = sceneRoom.GetComponent<RoomVisualizer>();
            if (visualizer != null)
            {
                visualizer.ApplyLayout(sceneRoom);
                Debug.Log($"[RoomLoader] Визуализация дверей применена для комнаты {sceneRoom.roomType}");
            }
            else
            {
                Debug.LogWarning($"[RoomLoader] RoomVisualizer не найден на объекте {sceneRoom.name}");
            }
        }
        else
        {
            Debug.LogError($"[RoomLoader] В сцене {scene.name} не найден объект с компонентом Room!");
        }

        Minimap minimap = FindObjectOfType<Minimap>();
        LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();

        if(minimap != null && levelGen != null && levelGen.currentRooms != null)
        {
            // Добавляем текущую комнату в открытые
            minimap.RevealRoom(sceneRoom);

            // Если нужно открывать и соседние комнаты (как в Isaac) — раскомментируй
            // foreach (Room neighbor in sceneRoom.connectedRooms)
             //{
             //    minimap.RevealRoom(neighbor);
             //}
       
            minimap.BuildMap(levelGen.currentRooms, sceneRoom);
        }
    }


    private IEnumerator PlacePlayerAtCorrectDoor()
    {
        // Ждём один кадр, чтобы объекты загрузились
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ ИГРОК НЕ НАЙДЕН! Убедитесь, что в MainScene есть Player с тегом Player");
            yield break;
        }

        

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.ResetInvincibility();
        }

        // Если это первый запуск — спавним в центре
        if (incomingDirection == Direction.None)
        {
            player.transform.position = Vector3.zero;
            
            yield break;
        }

        Direction targetDoorDirection = GetOppositeDirection(incomingDirection);
        Door[] doorsInNewScene = FindObjectsOfType<Door>();
        Door targetDoor = null;

        foreach (Door door in doorsInNewScene)
        {
            if (door.direction == targetDoorDirection)
            {
                targetDoor = door;
                break;
            }
        }

        if (targetDoor != null)
        {
            Vector3 spawnOffset = GetSpawnOffset(targetDoorDirection);
            player.transform.position = targetDoor.transform.position + spawnOffset;
            Debug.Log($"Игрок перемещён к двери {targetDoorDirection}");
        }
        else
        {
            player.transform.position = Vector3.zero;
            Debug.LogWarning($"Дверь {targetDoorDirection} не найдена! Спавн в центре.");
        }
    }

    private Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            default: return Direction.None;
        }
    }

    private Vector3 GetSpawnOffset(Direction dir)
    {
        float offsetDistance = 1.5f;
        switch (dir)
        {
            case Direction.Left: return Vector3.right * offsetDistance;
            case Direction.Right: return Vector3.left * offsetDistance;
            case Direction.Up: return Vector3.down * offsetDistance;
            case Direction.Down: return Vector3.up * offsetDistance;
            default: return Vector3.zero;
        }
    }
}