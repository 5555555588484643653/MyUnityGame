using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Movement & Attack")]
    public float speed = 2f;
    public float stoppingDistance = 5f; // Осы қашықтыққа келгенде тоқтайды
    public GameObject enemyBulletPrefab; // Жаудың оғы
    public float fireRate = 2f; // Әр 2 секунд сайын атады

    [Header("Enemy Health")]
    public int maxHealth = 100; // Зомбидің HP-і (бірден өліп қалмауы үшін)
    private int currentHealth;

    private float nextFireTime;
    private Transform player;

    void Start()
    {
        currentHealth = maxHealth;

        // Ойыншыны табу (аты немесе тегі арқылы)
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > stoppingDistance)
            {
                // Егер алыс болса, ойыншыға қарай жүру
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
            else
            {
                // Тоқтап, ату
                if (Time.time > nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }
    }

    void Shoot()
    {
        if (enemyBulletPrefab != null)
        {
            Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        }
    }

    // Ойыншы соққы жасағанда шақырылатын функция
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Алыстан ататын зомби соққы алды! Қалған HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Өлген кезде ойыншыға XP беру
        if (player != null)
        {
PlayerStats playerXP = player.GetComponent<PlayerStats>();
if (playerXP != null)
{
    playerXP.AddXP(25); // Или вызывайте соответствующий метод из PlayerStats
}
        }

        Destroy(gameObject);
    }
}