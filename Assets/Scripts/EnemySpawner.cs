using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab; // Құбыжық шаблоны
    [SerializeField] private float spawnRate = 1.5f; // Қанша секунд сайын шығады
    [SerializeField] private float spawnRadius = 10f; // Ойыншыдан қаншалықты алыс шығады

    private Transform player;

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true) // Тоқтаусыз цикл
        {
            yield return new WaitForSeconds(spawnRate);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (player == null) return;

        // Ойыншының айналасынан кездейсоқ нүкте табу (экран сыртынан шығуы үшін)
        Vector2 randomPos = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = player.position + new Vector3(randomPos.x, randomPos.y, 0f);

        // Жауды сол нүктеде пайда қылу
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}