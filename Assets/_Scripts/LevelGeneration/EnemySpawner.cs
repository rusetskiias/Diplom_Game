using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab;   // префаб босса
    public int minEnemies = 1;
    public int maxEnemies = 3;
    public float minDistanceFromPlayer = 2f;
    public LayerMask obstacleMask;

    private List<EnemyStats> aliveEnemies = new List<EnemyStats>();
    private Room modelRoom;


    public void Initialize(Room roomModel, float difficulty)
    {
        modelRoom = roomModel;

        // Безопасно получаем уровень и адаптивный множитель
        int currentLevel = 1;
        float adaptiveMult = 1f;
        if (GameManager.Instance != null)
            currentLevel = GameManager.Instance.currentLevel;
        if (AdaptiveDifficulty.Instance != null)
            adaptiveMult = AdaptiveDifficulty.Instance.currentAdaptiveMultiplier;

        // ===== ЛОГИКА ОБНОВЛЕНИЯ КОЛИЧЕСТВА ВРАГОВ =====
        if (currentLevel == 1)
        {
            minEnemies = 1;
            maxEnemies = 3;
        }
        else if (currentLevel == 2)
        {
            minEnemies = 2;
            maxEnemies = 3;
        }
        else
        {
            minEnemies = 2;
            maxEnemies = 4;
        }

        if (adaptiveMult < 0.95f)
        {
            minEnemies = Mathf.Max(1, minEnemies - 1);
            maxEnemies = Mathf.Max(2, maxEnemies - 1);
        }
        else if (adaptiveMult > 1.05f)
        {
            minEnemies += 1;
            maxEnemies += 1;
        }
        // =============================================

        if (modelRoom.isCleared)
        {
            UnlockDoors();
            return;
        }

        if (modelRoom.roomType == RoomType.Start ||
            modelRoom.roomType == RoomType.Shop ||
            modelRoom.roomType == RoomType.Gold)
        {
            modelRoom.isCleared = true;
            UnlockDoors();
            return;
        }

        if (modelRoom.roomType == RoomType.Boss)
        {
            SpawnBoss(difficulty);
            return;
        }

        // Проверяем, что префаб врага назначен
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawner: enemyPrefab не назначен для комнаты {modelRoom.roomType}! Двери разблокированы.");
            UnlockDoors();
            return;
        }

        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        SpawnEnemies(enemyCount, difficulty);
    }

    private void SpawnEnemies(int count, float difficulty)
    {
        // Проверяем, что префаб врага назначен
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemyPrefab не назначен!");
            UnlockDoors();
            return;
        }

        float floorMult = 1f;
        if (LevelGenerator.Instance != null)
            floorMult = LevelGenerator.Instance.floorMultiplier;

        float adaptiveMult = 1f;
        if (AdaptiveDifficulty.Instance != null)
            adaptiveMult = AdaptiveDifficulty.Instance.currentAdaptiveMultiplier;

        float totalMultiplier = floorMult * adaptiveMult;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomPositionInRoom();
            GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity);
            if (enemyObj == null) continue;

            EnemyStats enemy = enemyObj.GetComponent<EnemyStats>();
            if (enemy == null)
            {
                Debug.LogError("EnemySpawner: у префаба врага нет компонента EnemyStats!");
                Destroy(enemyObj);
                continue;
            }

            float baseHealth = enemy.maxHealth;
            enemy.maxHealth = baseHealth * totalMultiplier;
            enemy.health = enemy.maxHealth;

            aliveEnemies.Add(enemy);
            enemy.OnDeath.AddListener(OnEnemyDeath);
        }

        if (aliveEnemies.Count == 0)
        {
            Debug.LogWarning("В комнате не было создано ни одного врага, двери разблокированы.");
            UnlockDoors();
        }
    }

    private void SpawnBoss(float difficulty)
    {
        if (bossPrefab == null)
        {
            Debug.LogError("EnemySpawner: bossPrefab не назначен!");
            UnlockDoors();
            return;
        }

        Vector2 spawnPos = GetRandomPositionInRoom();
        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        if (bossObj == null) return;

        EnemyStats boss = bossObj.GetComponent<EnemyStats>();
        if (boss == null)
        {
            Debug.LogError("EnemySpawner: у префаба босса нет компонента EnemyStats!");
            Destroy(bossObj);
            UnlockDoors();
            return;
        }

        boss.isBoss = true;

        float floorMult = 1f;
        if (LevelGenerator.Instance != null)
            floorMult = LevelGenerator.Instance.floorMultiplier;

        float adaptiveMult = 1f;
        if (AdaptiveDifficulty.Instance != null)
            adaptiveMult = AdaptiveDifficulty.Instance.currentAdaptiveMultiplier;

        float totalMultiplier = floorMult * adaptiveMult;
        float baseHealth = 400f;
        boss.maxHealth = baseHealth * totalMultiplier;
        boss.health = boss.maxHealth;

        aliveEnemies.Add(boss);
        boss.OnDeath.AddListener(OnEnemyDeath);

        BossHealthBar bossBar = FindObjectOfType<BossHealthBar>();
        if (bossBar != null)
        {
            bossBar.SetBoss(boss);
        }
    }

    private void OnEnemyDeath(EnemyStats enemy)
    {
        aliveEnemies.Remove(enemy);
        if (aliveEnemies.Count == 0)
        {
            modelRoom.isCleared = true;
            UnlockDoors();
        }
    }

    private void UnlockDoors()
    {
        foreach (Door door in GetComponentsInChildren<Door>())
            door.SetLocked(false);
    }

    private Vector2 GetRandomPositionInRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 playerPos = player != null ? player.transform.position : Vector2.zero;

        while (true)
        {
            float x = Random.Range(-7f, 7f);
            float y = Random.Range(-4f, 4f);
            Vector2 candidate = new Vector2(x, y);

            if (Vector2.Distance(candidate, playerPos) < minDistanceFromPlayer)
                continue;

            if (Physics2D.OverlapCircle(candidate, 0.5f, obstacleMask) != null)
                continue;

            bool occupied = false;
            foreach (EnemyStats e in aliveEnemies)
            {
                if (e != null && Vector2.Distance(candidate, e.transform.position) < 1f)
                {
                    occupied = true;
                    break;
                }
            }
            if (occupied) continue;

            return candidate;
        }
    }
}