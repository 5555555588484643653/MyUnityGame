using System.Collections;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("Спавн параметрлері")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float initialSpawnRate = 1.4f;
    [SerializeField] private float minSpawnRate = 0.35f;
    [SerializeField] private float spawnRadius = 12f;

    private float currentSpawnRate;
    private float gameTime = 0f;
    private float nextBossTime = 60f; // Әр 60 секунд сайын Босс шығады
    private Transform player;
    private TextMeshProUGUI bossWarningText;

    private void Start()
    {
        currentSpawnRate = initialSpawnRate;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) player = playerObj.transform;

        CreateBossWarningUI();
        StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        gameTime += Time.deltaTime;

        // Уақыт өткен сайын жаулар тезірек шығады
        currentSpawnRate = Mathf.Max(minSpawnRate, initialSpawnRate - (gameTime * 0.005f));

        // Босс уақыты жеткенде
        if (gameTime >= nextBossTime)
        {
            SpawnBoss();
            nextBossTime = gameTime + 60f;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnRate);
            SpawnNormalEnemy();
        }
    }

    private void SpawnNormalEnemy()
    {
        if (player == null || enemyPrefab == null) return;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = player.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private void SpawnBoss()
    {
        if (player == null || enemyPrefab == null) return;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * (spawnRadius * 0.8f);
        Vector3 spawnPos = player.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

        GameObject boss = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        boss.name = "BOSS_GoblinKing";

        // Боссты үлкен әрі қауіпті қылу
        boss.transform.localScale = Vector3.one * 2.2f;

        EnemyMovement em = boss.GetComponent<EnemyMovement>();
        if (em != null)
        {
            // Босстың күшін арттыру
            boss.GetComponent<SpriteRenderer>().color = new Color(1f, 0.2f, 0.2f);
        }

        StartCoroutine(ShowBossWarning());
        CameraFollow.Shake(0.5f, 0.35f);
    }

    private void CreateBossWarningUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject warnObj = new GameObject("BossWarning", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        warnObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = warnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.75f);
        rt.anchorMax = new Vector2(0.5f, 0.75f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800, 80);

        bossWarningText = warnObj.GetComponent<TextMeshProUGUI>();
        bossWarningText.text = "⚠️ ДАБЫЛ: ГОБЛИНДЕР ПАТШАСЫ АРЕНАҒА ШЫҚТЫ! ⚠️";
        bossWarningText.fontSize = 28;
        bossWarningText.fontStyle = FontStyles.Bold;
        bossWarningText.alignment = TextAlignmentOptions.Center;
        bossWarningText.color = new Color(1f, 0.15f, 0.15f);

        warnObj.SetActive(false);
    }

    private IEnumerator ShowBossWarning()
    {
        if (bossWarningText == null) yield break;

        bossWarningText.gameObject.SetActive(true);

        // Жарқылдау әсері
        for (int i = 0; i < 6; i++)
        {
            bossWarningText.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
            bossWarningText.color = Color.red;
            yield return new WaitForSeconds(0.2f);
        }

        bossWarningText.gameObject.SetActive(false);
    }
}