using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1f; // Секундына 1 рет атады
    private float nextFireTime;

    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            ShootClosestEnemy();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootClosestEnemy()
    {
        // "Enemy" тегі бар барлық жауларды табады
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        GameObject closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        // Ең жақын жауды анықтау
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = enemy;
            }
        }

        // Егер жау табылса, оқ шығару
        if (closestEnemy != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Vector3 direction = (closestEnemy.transform.position - transform.position).normalized;
            bullet.GetComponent<Bullet>().Setup(direction);
        }
    }
}