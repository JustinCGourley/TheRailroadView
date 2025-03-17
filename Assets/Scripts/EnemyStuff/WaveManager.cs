using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

class Portal
{
    public Vector3 position;
    public float timePerSpawn;
    public float lastSpawnTime;
    public GameObject enemyToSpawn;
    public int enemyTypeToSpawn;

    public Portal(Vector3 pos)
    {
        position = pos;
        timePerSpawn = 0;
        lastSpawnTime = 0;
    }
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    List<Enemy> enemies;


    // 0 - basic
    // 1 - fast
    // 2 - tank
    // 3 - flying
    [SerializeField] GameObject[] enemyObj;
    [SerializeField] int[] enemySpawnCount;
    [SerializeField] int[] enemySpawnCost;
    [SerializeField] int[] enemySpawnDay;
    [SerializeField] float[] enemySpawnDelay;
    [SerializeField] GameObject[] bossObj;
    [SerializeField] int forceSpawn = -1; // if this is NOT -1 it will force every spawn to be of this type
    public int maxEnemiesOut;

    GameManager gameManager;

    List<Portal> spawnPortals;
    int lastPortalPosition = 0;

    // Start is called before the first frame update
    void Start()
    {
        enemies = new List<Enemy>();
        
        gameManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<GameManager>();
        spawnPortals = new List<Portal>();
    }

    public void GenerateWaveSpawn()
    {
        int day = ProgressionManager.Instance.CurrentDay;
        
        //every ten days spawn a boss
        if ((day + 1) % 10 == 0)
        {
            //TODO - make this a random boss :)
            //get random boss
            int bossToSpawn = 0;

            GameObject boss = SpawnBoss(bossToSpawn);
            boss.transform.position = GetRandomPortalPosition();
        }

        // decide on what enemy is going to spawn
        List<int> availableEnemies = new List<int>() { 0 };
        for (int i = 0; i < enemySpawnCount.Length; i++)
        {
            if (day >= enemySpawnDay[i])
            {
                availableEnemies.Add(i);
            }
        }

        int ranPick = availableEnemies[Random.Range(0, availableEnemies.Count)];

        // create and setup portal
        Portal portal = new Portal(GetRandomPortalPosition());
        portal.enemyToSpawn = enemyObj[ranPick];
        portal.enemyTypeToSpawn = ranPick;
        portal.timePerSpawn = enemySpawnDelay[ranPick] - (enemySpawnDelay[ranPick] * (day / 100)); // spawn rate is the specified amount, up to 50% reduced based on day (final day being 50% at day 100)
        Debug.Log($"Time per spawn for portal: {portal.timePerSpawn} [{enemySpawnDelay[ranPick]} - {(enemySpawnDelay[ranPick] * (day / 100))}]");

        spawnPortals.Add(portal);
    }

    private GameObject SpawnEnemy(GameObject enemyToSpawn)
    {
        GameObject enemy = Instantiate(enemyToSpawn, this.transform);
        enemy.name = $"Enemy {enemyToSpawn}";
        return enemy;
    }

    private GameObject SpawnBoss(int bossType)
    {
        GameObject boss = Instantiate(bossObj[bossType], this.transform);
        boss.name = $"Boss {bossType}";
        return boss;
    }

    public void UpdateSpawner()
    {
        for (int i = 0; i < spawnPortals.Count; i++)
        {
            SpawnWave(spawnPortals[i]);
            UtilityGizmo.Instance.DrawSphere(spawnPortals[i].position, 2f, new Color(1f, 0f, 0f, 0.5f));
        }
    }

    public void UpdateEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || enemy.controller == null)
            { 
                enemies.RemoveAt(i);
                continue;
            }
            enemy.controller.UpdateEnemy();
        }
    }

    private void SpawnWave(Portal portal)
    {
        if (Time.time - portal.lastSpawnTime >= portal.timePerSpawn)
        {
            GameObject enemyToSpawn = SpawnEnemy(portal.enemyToSpawn);
            enemyToSpawn.transform.position = portal.position;
            enemies.Add(new Enemy(enemyToSpawn, enemyToSpawn.GetComponent<EnemyController>()));

            portal.lastSpawnTime = Time.time;
        }
    }

    private float GetTimeToNext(int enemyType)
    {
        switch (enemyType)
        {
            case 0:
                return 1f;
            case 1:
                return 1.25f;
            case 2:
                return 2.5f;
            case 3:
                return 1.5f;
        }
        return 0;
    }

    public void KillEnemy(GameObject enemy)
    {
        enemies.Remove(GetEnemy(enemy));
        Destroy(enemy);
    }

    public Enemy GetEnemy(GameObject enemy)
    {
        foreach (Enemy e in enemies)
        {
            if (e.obj == enemy)
            {
                return e;
            }
        }
        return null;
    }

    private Vector3 GetRandomPortalPosition()
    {
        //params
        int minDistance = 6;
        int maxDistance = 10;


        Vector2Int nexusPos = gameManager.GetNexusCoords();
        int[,] placementArea = GameManager.Instance.GetPlacementArea();


        Vector2Int furthestLeft = Vector2Int.zero;
        Vector2Int furthestRight = Vector2Int.zero;
        Vector2Int furthestTop = Vector2Int.zero;
        Vector2Int furthestBottom = Vector2Int.zero;
        for (int x = 0; x < placementArea.GetLength(0); x++)
        {
            for (int y = 0; y < placementArea.GetLength(1); y++)
            {
                if (placementArea[x, y] == 1)
                {
                    if (x < furthestLeft.x)
                    {
                        furthestLeft.x = x;
                        furthestLeft.y = y;
                    }
                    else if (x > furthestRight.x)
                    {
                        furthestRight.x = x;
                        furthestRight.y = y;
                    }
                    else if (y < furthestBottom.y)
                    {
                        furthestBottom.x = x;
                        furthestBottom.y = y;
                    }
                    else if (y > furthestTop.y)
                    {
                        furthestTop.x = x;
                        furthestTop.y = y;
                    }
                }
            }
        }

        int pick = Random.Range(0, 4);
        if (lastPortalPosition == pick)
        {
            pick += (Random.Range(0, 2) == 0 ? 1 : -1);

            if (pick > 3)
                pick = 0;
            else if (pick < 0)
                pick = 3;
        }
        Vector2Int pickPos = Vector2Int.zero;

        int placementWidth = placementArea.GetLength(0);
        int placementHeight = placementArea.GetLength(1);
        //left
        if (pick == 0)
        {
            if (furthestLeft.x == 0)
            {
                pick++;
            }
            if (furthestLeft.x <= 3)
            {
                pickPos = furthestLeft;
                pickPos.x = 0;
            }
            else
            {
                int numToEnd = furthestLeft.x;
                int ran = Random.Range(minDistance, numToEnd > maxDistance ? maxDistance : numToEnd);
                pickPos = furthestLeft;
                pickPos.x = furthestLeft.x - ran;
            }
        }
        //right
        if (pick == 1)
        {
            if (furthestRight.x == placementWidth - 1)
            {
                pick++;
            }
            if (placementWidth - furthestRight.x <= 3)
            {
                pickPos = furthestRight;
                pickPos.x = placementWidth-1;
            }
            else
            {
                int numToEnd = placementWidth - furthestRight.x;
                int ran = Random.Range(minDistance, numToEnd > maxDistance ? maxDistance : numToEnd);
                pickPos = furthestRight;
                pickPos.x = furthestRight.x + ran;
            }
        }
        //bottom
        if (pick == 2)
        {
            if (furthestBottom.y == 0)
            {
                pick++;
            }
            if (furthestBottom.y <= 3)
            {
                pickPos = furthestBottom;
                pickPos.x = 0;
            }
            else
            {
                int numToEnd = furthestBottom.y;
                int ran = Random.Range(minDistance, numToEnd > maxDistance ? maxDistance : numToEnd);
                pickPos = furthestBottom;
                pickPos.y = furthestBottom.y - ran;
            }
        }
        //top
        if (pick == 3)
        {
            if (placementHeight - furthestTop.y <= 3)
            {
                pickPos = furthestTop;
                pickPos.x = placementHeight - 1;
            }
            else
            {
                int numToEnd = placementHeight - furthestTop.y;
                int ran = Random.Range(minDistance, numToEnd > maxDistance ? maxDistance : numToEnd);
                pickPos = furthestTop;
                pickPos.y = furthestTop.y + ran;
            }
        }


        lastPortalPosition = pick;
        Debug.Log($"pick: {pick} - pickPos: {pickPos}");

        return Utility.Instance.GetPositionFromTileCoords(pickPos, true);
    }

    public List<Enemy> getEnemiesInRange(Vector3 pos, float radius)
    {
        List<Enemy> enemiesInRange = new List<Enemy>();

        for (int i = 0; i < enemies.Count; i++)
        {
            if ((pos - enemies[i].obj.transform.position).magnitude <= radius)
            {
                enemiesInRange.Add(enemies[i]);
            }
        }

        return enemiesInRange;
    }

    public List<Enemy> DamageEnemiesInRange(Vector3 pos, float radius, float damage, Element element, int limit = -1)
    {
        List<Enemy> enemiesHit = new List<Enemy>();

        for (int i = 0; i < enemies.Count; i++)
        {
            try
            {
                if ((pos - enemies[i].obj.transform.position).magnitude <= radius)
                {
                    enemies[i].controller.TakeDamage(damage, element);
                    enemiesHit.Add(enemies[i]);
                    limit--;
                    if (limit == 0)
                        break;
                }
            } catch
            {
                Debug.LogWarning("Something broke during [DamageEnemiesInRange] but it was probably just a kill a group race condition?");
            }
        }

        return enemiesHit;
    }
}
