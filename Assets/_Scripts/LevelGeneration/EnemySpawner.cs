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

        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        SpawnEnemies(enemyCount, difficulty);
    }

    private void SpawnEnemies(int count, float difficulty)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomPositionInRoom();
            GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity);
            EnemyStats enemy = enemyObj.GetComponent<EnemyStats>();
            enemy.Initialize(difficulty);
            aliveEnemies.Add(enemy);
            enemy.OnDeath.AddListener(OnEnemyDeath);
        }

        if (aliveEnemies.Count == 0) UnlockDoors();
    }

    private void SpawnBoss(float difficulty)
    {
        Vector2 spawnPos = GetRandomPositionInRoom();
        GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        EnemyStats boss = bossObj.GetComponent<EnemyStats>();

        boss.isBoss = true;
        boss.Initialize(difficulty);

        // ѕереопредел€ем здоровье босса (можно больше, чем у обычных врагов)
        boss.maxHealth = 400 + difficulty * 30;
        boss.health = boss.maxHealth;

        aliveEnemies.Add(boss);
        boss.OnDeath.AddListener(OnEnemyDeath);

        // ѕоказать полоску здоровь€ босса
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