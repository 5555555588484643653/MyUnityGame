using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    public float speed = 2f;
    public float stoppingDistance = 5f; // Осы қашықтыққа келгенде тоқтайды
    public GameObject enemyBulletPrefab; // Жаудың оғы
    public float fireRate = 2f; // Әр 2 секунд сайын атады

    private float nextFireTime;
    private Transform player;

    void Start()
    {
        // Ойыншыны аты арқылы тауып алады (аты дәл "Player" болуы шарт)
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            // Ойыншы мен жаудың арақашықтығын есептеу
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > stoppingDistance)
            {
                // Егер алыс болса, ойыншыға қарай жүру
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
            else
            {
                // Егер жақын келсе (қашықтық 5-тен аз болса), тоқтап, оқ ату
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
        // Оқты жаудың тұрған жерінде пайда қылу
        Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
    }
}