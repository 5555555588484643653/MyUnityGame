using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee Attack")]
    [SerializeField, Min(0f)] private float attackCooldown = 0.5f;
    [SerializeField, Min(0f)] private float hitDelay = 0.1f;
    [SerializeField, Min(1)] private int damage = 100;
    [SerializeField, Min(0f)] private float attackReach = 1.2f;
    [SerializeField, Min(0f)] private float attackRadius = 1.1f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0.01f)] private float fireRate = 0.25f;
    [SerializeField, Min(1)] private int bulletDamage = 35;
    private float nextFireTime;

    [Header("Special Attack")]
    [SerializeField, Min(0f)] private float specialCooldown = 10f;
    [SerializeField, Min(0f)] private float specialHitDelay = 0.45f;
    [SerializeField, Min(0f)] private float specialRadius = 5f;
    [SerializeField, Min(1)] private int specialDamage = 250;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private float nextAttackTime;
    private float nextSpecialTime;
    private const string AttackState = "Base Layer.Attack";
    private const string SpecialState = "Base Layer.Special";

    public int Damage => damage;
    public float SpecialCooldownRemaining => Mathf.Max(0f, nextSpecialTime - Time.time);

    public void UpgradeDamage(int amount)
    {
        int upgrade = Mathf.Max(0, amount);
        damage += upgrade;
        bulletDamage += upgrade;
    }

    public void RestoreDamage(int savedDamage)
    {
        int upgrade = Mathf.Max(0, savedDamage - damage);
        damage = Mathf.Max(1, savedDamage);
        bulletDamage += upgrade;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame &&
            Time.time >= nextSpecialTime)
        {
            nextSpecialTime = Time.time + specialCooldown;
            if (animator != null)
            {
                animator.Play(SpecialState, 0, 0f);
            }

            StartCoroutine(DealSpecialDamage());
        }

        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            return;
        }

        Vector2 direction = GetMouseDirection();
        if (direction.sqrMagnitude == 0f)
        {
            return;
        }

        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }

        if (bulletPrefab != null && Time.time >= nextFireTime)
        {
            ShootBullet(direction);
            nextFireTime = Time.time + fireRate;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            bulletPrefab == null &&
            Time.time >= nextAttackTime)
        {
            if (animator != null)
            {
                animator.Play(AttackState, 0, 0f);
            }

            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(DealMeleeDamage(direction));
        }

    }

    private Vector2 GetMouseDirection()
    {
        if (mainCamera == null)
        {
            Debug.LogError("PlayerAttack: Main Camera табылмады.");
            return Vector2.zero;
        }

        Ray mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane playerPlane = new Plane(Vector3.forward, transform.position);
        if (!playerPlane.Raycast(mouseRay, out float distance))
        {
            return Vector2.zero;
        }

        Vector3 target = mouseRay.GetPoint(distance);
        Vector2 direction = (Vector2)(target - transform.position);
        return direction.sqrMagnitude > 0f ? direction.normalized : Vector2.zero;
    }

    private IEnumerator DealMeleeDamage(Vector2 direction)
    {
        yield return new WaitForSeconds(hitDelay);

        Vector2 hitCenter = (Vector2)transform.position + direction * attackReach;
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackRadius);
        var damagedEnemies = new HashSet<RangedEnemy>();

        foreach (Collider2D hit in hits)
        {
            RangedEnemy enemy = hit.GetComponentInParent<RangedEnemy>();
            if (enemy != null && damagedEnemies.Add(enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void ShootBullet(Vector2 direction)
    {
        GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet == null)
        {
            Debug.LogError("PlayerAttack: Bullet префабында Bullet компоненті жоқ.", bulletObject);
            Destroy(bulletObject);
            return;
        }

        bullet.Setup(direction, bulletDamage);
    }

    private IEnumerator DealSpecialDamage()
    {
        yield return new WaitForSeconds(specialHitDelay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, specialRadius);
        var damagedEnemies = new HashSet<RangedEnemy>();
        foreach (Collider2D hit in hits)
        {
            RangedEnemy enemy = hit.GetComponentInParent<RangedEnemy>();
            if (enemy != null && damagedEnemies.Add(enemy))
            {
                enemy.TakeDamage(specialDamage);
            }
        }
    }
}
