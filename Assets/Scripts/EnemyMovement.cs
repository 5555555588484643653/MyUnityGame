using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Жау баптаулары")]
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private int maxHealth = 60;
    [SerializeField] private int touchDamage = 12;
    [SerializeField] private GameObject xpGemPrefab;

    private int currentHealth;
    private Transform player;
    private SpriteRenderer sr;
    private Color originalColor;
    private float nextDamageTime;
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
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            if (sr != null)
            {
                sr.flipX = player.position.x < transform.position.x;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckDamagePlayer(collision.gameObject);
    }

    private void CheckDamagePlayer(GameObject target)
    {
        if (isDead) return;

        if (target.name == "Player" || target.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                PlayerStats stats = target.GetComponent<PlayerStats>();
                PlayerMovement movement = target.GetComponent<PlayerMovement>();

                // Егер ойыншы Dash (секіру) кезінде болса, зақым тимейді!
                if (movement != null && movement.IsInvincible) return;

                if (stats != null)
                {
                    stats.TakeDamage(touchDamage);
                    nextDamageTime = Time.time + 0.8f;
                }
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        bool isCrit = Random.value < 0.2f;
        int finalDamage = isCrit ? (int)(amount * 1.8f) : amount;

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

        // Ойыншының статистикасын жаңарту (Kill count)
        PlayerStats stats = FindAnyObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.RegisterKill();
            stats.AddXP(20);
        }

        // Кристалл тастау
        if (xpGemPrefab != null)
        {
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}