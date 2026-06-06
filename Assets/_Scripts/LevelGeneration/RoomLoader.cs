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
            return;
        }
        pendingRoom = targetRoom;
        incomingDirection = Direction.None;
       
        string sceneName = GetSceneNameByRoomType(targetRoom.roomType);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        currentSceneName = sceneName;
    }

    // Загружаем комнату через дверь
    public void TransitionToRoom(Room targetRoom, Direction fromDirection)
    {
        if (targetRoom == null)
        {
            return;
        }

        pendingRoom = targetRoom;
        incomingDirection = fromDirection;
        
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

            // ========== УПРАВЛЕНИЕ ПОЛОСКОЙ БОССА ==========
            BossHealthBar bossBar = FindObjectOfType<BossHealthBar>();

            if (modelRoom.roomType == RoomType.Boss)
            {
                if (modelRoom.isCleared)
                {
                    // Босс уже мёртв – скрываем полоску
                    if (bossBar != null) bossBar.Hide();
                }
                // Если босс жив – полоска появится в EnemySpawner.SpawnBoss()
            }
            else
            {
                // В любой не-босс комнате полоска должна быть скрыта
                if (bossBar != null) bossBar.Hide();
            }
            
            if (modelRoom.roomType == RoomType.Boss && modelRoom.isDoorToNextLevelActive)
            {
                NextLevelDoor door = FindObjectOfType<NextLevelDoor>(true);
                if (door != null) door.Activate();
            }

            // ВИЗУАЛИЗАЦИЯ ДВЕРЕЙ 
            RoomVisualizer visualizer = sceneRoom.GetComponent<RoomVisualizer>();
            if (visualizer != null)
            {
                visualizer.ApplyLayout(sceneRoom);
            }
        }
      

        Minimap minimap = FindObjectOfType<Minimap>();
        LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();

        if (minimap != null && levelGen != null && levelGen.currentRooms != null)
        {
            // Добавляем текущую комнату в открытые
            minimap.RevealRoom(sceneRoom);
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
        }
        else
        {
            player.transform.position = Vector3.zero;
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
        float offsetDistance = 2.5f;
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