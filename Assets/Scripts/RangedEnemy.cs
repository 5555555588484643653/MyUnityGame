using System.Collections;
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Movement & Melee Attack")]
    [SerializeField, Min(0f)] private float speed = 2.5f;
    [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.1f;
    [SerializeField, Min(1)] private int damage = 12;
    [SerializeField, Min(0f)] private float hitDelay = 0.22f;
    [SerializeField, Min(0f)] private float specialAttackRange = 5.5f;
    [SerializeField, Min(0f)] private float specialAttackCooldown = 9f;
    [SerializeField, Min(0f)] private float specialAttackHitDelay = 0.5f;

    [Header("Enemy Health")]
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private GameObject xpGemPrefab;
    [SerializeField, Min(1)] private int experienceReward = 25;
    private int currentHealth;
    private bool isBoss;
    private bool isBossSpecialAttacking;

    private float nextAttackTime;
    private float nextSpecialAttackTime;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D movementCollider;
    private ContactFilter2D movementFilter;
    private readonly RaycastHit2D[] movementHits = new RaycastHit2D[32];
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int SpecialAttackTrigger = Animator.StringToHash("SpecialAttack");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movementCollider = GetComponent<Collider2D>();
        movementFilter = new ContactFilter2D();
        movementFilter.SetLayerMask(Physics2D.AllLayers);
        movementFilter.useTriggers = false;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        player = playerObject != null ? playerObject.transform : null;
    }

    public void ConfigureForWave(int wave, bool bossWave)
    {
        isBoss = bossWave;
        if (isBoss)
        {
            maxHealth = 1800;
            damage = 32;
            speed = 1.8f;
            attackRange = 2.4f;
            attackCooldown = 1.35f;
            experienceReward = 500;
            transform.localScale *= 1.5f;
            nextSpecialAttackTime = Time.time + 3f;
            if (animator != null)
            {
                animator.Play("Idle", 0, 0f);
            }
        }
        else
        {
            maxHealth = 100 + wave * 40;
            damage = 12 + wave * 2;
            speed = 2.5f + wave * 0.06f;
            experienceReward = 25;
        }

        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (player == null)
        {
            SetMovementAnimation(0f);
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (spriteRenderer != null && Mathf.Abs(toPlayer.x) > 0.01f)
        {
            spriteRenderer.flipX = toPlayer.x > 0f;
        }

        if (isBoss && !isBossSpecialAttacking &&
            distance <= specialAttackRange &&
            Time.time >= nextSpecialAttackTime)
        {
            nextSpecialAttackTime = Time.time + specialAttackCooldown;
            SetMovementAnimation(0f);
            StartCoroutine(SpecialAttackPlayer());
            return;
        }

        if (isBossSpecialAttacking)
        {
            SetMovementAnimation(0f);
            return;
        }

        if (distance > attackRange)
        {
            MoveTowardPlayer(toPlayer, distance);
            SetMovementAnimation(1f);
            return;
        }

        SetMovementAnimation(0f);
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(AttackPlayer());
        }
    }

    private void MoveTowardPlayer(Vector2 toPlayer, float distance)
    {
        Vector2 direction = toPlayer / distance;
        float movementDistance = Mathf.Min(speed * Time.deltaTime, distance);
        Vector2 movement = direction * movementDistance;

        if (movementCollider != null &&
            TryGetObstacleHit(direction, movementDistance, out RaycastHit2D obstacleHit))
        {
            float travelDistance = Mathf.Max(0f, obstacleHit.distance - 0.02f);
            Vector2 firstMove = direction * travelDistance;
            Vector2 remainingMove = movement - firstMove;
            Vector2 slideMove = remainingMove -
                Vector2.Dot(remainingMove, obstacleHit.normal) * obstacleHit.normal;

            if (slideMove.sqrMagnitude > 0.0001f &&
                TryGetObstacleHit(slideMove.normalized, slideMove.magnitude, out RaycastHit2D slideHit))
            {
                slideMove = slideMove.normalized * Mathf.Max(0f, slideHit.distance - 0.02f);
            }

            movement = firstMove + slideMove;
        }

        transform.position += (Vector3)movement;
    }

    private bool TryGetObstacleHit(Vector2 direction, float distance, out RaycastHit2D nearestHit)
    {
        nearestHit = default;
        if (movementCollider == null)
        {
            return false;
        }

        int hitCount = movementCollider.Cast(direction, movementFilter, movementHits, distance);
        float nearestDistance = distance;
        bool foundHit = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = movementHits[i];
            Collider2D hitCollider = hit.collider;
            if (hitCollider == null ||
                hitCollider.GetComponentInParent<RangedEnemy>() != null ||
                hitCollider.GetComponentInParent<PlayerStats>() != null ||
                hit.distance >= nearestDistance)
            {
                continue;
            }

            nearestHit = hit;
            nearestDistance = hit.distance;
            foundHit = true;
        }

        return foundHit;
    }

    private void SetMovementAnimation(float movementSpeed)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedParameter, movementSpeed);
        }
    }

    private IEnumerator AttackPlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }

        yield return new WaitForSeconds(hitDelay);

        if (player == null ||
            Vector2.Distance(transform.position, player.position) > attackRange + 0.25f)
        {
            yield break;
        }

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
    }

    private IEnumerator SpecialAttackPlayer()
    {
        isBossSpecialAttacking = true;
        if (animator != null)
        {
            animator.SetTrigger(SpecialAttackTrigger);
        }

        yield return new WaitForSeconds(specialAttackHitDelay);
        if (!isDead && player != null &&
            Vector2.Distance(transform.position, player.position) <= specialAttackRange)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage * 2);
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0f, 0.8f - specialAttackHitDelay));
        isBossSpecialAttacking = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= 0 || isDead)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"Зомби соққы алды. Қалған HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger(HitTrigger);
        }
    }

    private void Die()
    {
        isDead = true;

        PlayerStats playerStats = player != null
            ? player.GetComponentInParent<PlayerStats>()
            : FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.RegisterZombieKill();
        }

        if (xpGemPrefab != null)
        {
            GameObject gem = Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
            XPGem xpGem = gem.GetComponent<XPGem>();
            if (xpGem != null)
            {
                xpGem.xpValue = experienceReward;
            }
        }

        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.NotifyEnemyDefeated(this);
        }

        if (animator == null)
        {
            Destroy(gameObject);
            return;
        }

        animator.SetTrigger(DieTrigger);
        StartCoroutine(DestroyAfterDeathAnimation());
    }

    private IEnumerator DestroyAfterDeathAnimation()
    {
        yield return new WaitForSeconds(isBoss ? 0.85f : 0.55f);
        Destroy(gameObject);
    }
}