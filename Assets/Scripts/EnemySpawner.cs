using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private const int FinalWave = 5;
    private const int MaxSpawnAttempts = 32;

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField, Min(1f)] private float spawnRadius = 10f;
    [SerializeField, Min(0f)] private float spawnInterval = 0.65f;
    [SerializeField, Min(0f)] private float timeBetweenWaves = 2f;

    private Transform player;
    private PlayerStats playerStats;
    private GameHUD gameHUD;
    private Coroutine waveRoutine;
    private RangedEnemy boss;
    private int currentWave;
    private int enemiesAlive;
    private int enemiesDefeated;
    private int enemiesInWave;
    private bool stopped;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject == null)
        {
            Debug.LogError("EnemySpawner: Player табылмады.");
            enabled = false;
            return;
        }

        if (enemyPrefab == null || enemyPrefab.GetComponent<RangedEnemy>() == null)
        {
            Debug.LogError("EnemySpawner: RangedEnemy бар зомби префабы тағайындалмаған.");
            enabled = false;
            return;
        }

        if (bossPrefab == null)
        {
            bossPrefab = Resources.Load<GameObject>("BossPrefab");
        }

        if (bossPrefab == null || bossPrefab.GetComponent<RangedEnemy>() == null)
        {
            Debug.LogError("EnemySpawner: BossPrefab табылмады. Unity жобасын ашып, boss анимацияларын генерациялаңыз.");
            enabled = false;
            return;
        }

        player = playerObject.transform;
        playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("EnemySpawner: Player объектісінде PlayerStats жоқ.");
            enabled = false;
            return;
        }

        waveRoutine = StartCoroutine(RunWaves());
    }

    public void StopSpawning()
    {
        stopped = true;
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }

    public void NotifyEnemyDefeated(RangedEnemy enemy)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        enemiesDefeated++;
        if (enemy == boss)
        {
            boss = null;
        }
    }

    private IEnumerator RunWaves()
    {
        for (currentWave = 1; currentWave <= FinalWave; currentWave++)
        {
            if (stopped)
            {
                yield break;
            }

            bool isBossWave = currentWave == FinalWave;
            if (gameHUD == null)
            {
                gameHUD = playerStats.Hud;
            }

            playerStats.ApplyWaveBonus();
            if (gameHUD != null)
            {
                gameHUD.ShowWaveAnnouncement(
                    currentWave,
                    isBossWave,
                    playerStats.WaveDamageBonus,
                    playerStats.WaveXPBonusPerGem);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
            if (stopped)
            {
                yield break;
            }

            enemiesInWave = isBossWave ? 1 : 3 + (currentWave - 1) * 2;
            enemiesAlive = enemiesInWave;
            enemiesDefeated = 0;
            UpdateHud(isBossWave ? "БОСС ШЫҚТЫ!" : "ЗОМБИЛЕРДІ ЖЕҢ!");

            for (int i = 0; i < enemiesInWave; i++)
            {
                if (stopped)
                {
                    yield break;
                }

                if (!SpawnEnemy(currentWave, isBossWave))
                {
                    stopped = true;
                    yield break;
                }

                UpdateHud(isBossWave ? "БОСС ШЫҚТЫ!" : "ЗОМБИЛЕРДІ ЖЕҢ!");
                if (i + 1 < enemiesInWave)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            float hudRefreshTime = 0f;
            while (enemiesAlive > 0 && !stopped)
            {
                if (Time.time >= hudRefreshTime)
                {
                    UpdateHud(boss != null
                        ? $"БОСС HP: {boss.CurrentHealth}/{boss.MaxHealth}"
                        : "ЗОМБИЛЕРДІ ЖЕҢ!");
                    hudRefreshTime = Time.time + 0.2f;
                }

                yield return null;
            }

            if (stopped)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.65f);
            if (isBossWave)
            {
                if (gameHUD != null)
                {
                    gameHUD.ShowVictory();
                }

                waveRoutine = null;
                yield break;
            }
        }
    }

    private bool SpawnEnemy(int wave, bool isBoss)
    {
        GameObject prefab = isBoss ? bossPrefab : enemyPrefab;
        Collider2D prefabCollider = prefab.GetComponent<Collider2D>();
        if (prefabCollider == null)
        {
            Debug.LogError($"EnemySpawner: '{prefab.name}' префабында Collider2D жоқ.", prefab);
            return false;
        }

        float spawnClearance = prefabCollider.bounds.extents.magnitude;
        if (spawnClearance <= 0.01f)
        {
            spawnClearance = prefabCollider is BoxCollider2D boxCollider
                ? boxCollider.size.magnitude * 0.5f
                : 0.5f;
        }

        if (isBoss)
        {
            spawnClearance *= 1.5f;
        }

        Vector3 position = default;
        bool foundClearPosition = false;
        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            Vector2 direction = Random.insideUnitCircle;
            if (direction.sqrMagnitude < 0.01f)
            {
                continue;
            }

            Vector2 candidate = (Vector2)player.position + direction.normalized * spawnRadius;
            if (IsSpawnPositionClear(candidate, spawnClearance))
            {
                position = candidate;
                foundClearPosition = true;
                break;
            }
        }

        if (!foundClearPosition)
        {
            Debug.LogError(
                $"EnemySpawner: {MaxSpawnAttempts} әрекеттен кейін кедергіден бос орын табылмады.",
                this);
            return false;
        }

        GameObject spawned = Instantiate(prefab, position, Quaternion.identity);
        RangedEnemy enemy = spawned.GetComponent<RangedEnemy>();
        enemy.ConfigureForWave(wave, isBoss);
        if (isBoss)
        {
            spawned.name = "Zombie Boss";
            boss = enemy;
        }

        return true;
    }

    private static bool IsSpawnPositionClear(Vector2 position, float clearanceRadius)
    {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, clearanceRadius);
        foreach (Collider2D overlap in overlaps)
        {
            if (overlap.isTrigger ||
                overlap.GetComponentInParent<RangedEnemy>() != null ||
                overlap.GetComponentInParent<PlayerStats>() != null)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void UpdateHud(string message)
    {
        if (gameHUD == null)
        {
            gameHUD = playerStats != null ? playerStats.Hud : null;
        }

        if (gameHUD == null)
        {
            return;
        }

        string detail = boss != null
            ? $"{message}  |  HP {boss.CurrentHealth}/{boss.MaxHealth}"
            : $"{message}  |  {enemiesDefeated}/{enemiesInWave}";
        gameHUD.SetWaveStatus(currentWave, FinalWave, detail);
    }
}
