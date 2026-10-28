using System.Collections;
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Қозғалыс және Шабуыл")]
    public float speed = 2f;
    public float stoppingDistance = 5f;
    public GameObject enemyBulletPrefab;
    public float fireRate = 2f;

    [Header("Денсаулық")]
    public int maxHealth = 70;
    private int currentHealth;
    private float nextFireTime;
    private Transform player;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > stoppingDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
            else
            {
                if (Time.time > nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }
            }

            if (sr != null)
            {
                sr.flipX = player.position.x < transform.position.x;
            }
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab != null)
        {
            Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        bool isCrit = Random.value < 0.25f;
        int finalDamage = isCrit ? (int)(damageAmount * 1.8f) : damageAmount;

        currentHealth -= finalDamage;
        DamagePopup.Create(transform.position, finalDamage, isCrit);
        CameraFollow.Shake(0.08f, 0.1f);

        StartCoroutine(FlashWhiteRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashWhiteRoutine()
    {
        if (sr != null)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            if (sr != null) sr.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayerStats stats = FindAnyObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.RegisterKill();
            stats.AddXP(30);
        }

        Destroy(gameObject);
    }
}