using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Оқ параметрлері")]
    public float speed = 12f;
    public int damage = 35;
    public bool isExplosive = false;
    public float explosionRadius = 2.5f;

    private Vector3 targetDir;

    public void Setup(Vector3 direction, int bulletDamage = 35, bool explosive = false)
    {
        targetDir = direction.normalized;
        damage = bulletDamage;
        isExplosive = explosive;
        Destroy(gameObject, 3.5f);
    }

    private void Update()
    {
        transform.position += targetDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ойыншының өзіне тимеуі керек
        if (collision.name == "Player" || collision.CompareTag("Player")) return;

        bool hitEnemy = false;

        // 1. EnemyMovement (Кәдімгі зомби немесе гоблин)
        EnemyMovement meleeEnemy = collision.GetComponentInParent<EnemyMovement>();
        if (meleeEnemy != null)
        {
            meleeEnemy.TakeDamage(damage);
            hitEnemy = true;
        }

        // 2. RangedEnemy (Алыстан ататын жау)
        RangedEnemy rangedEnemy = collision.GetComponentInParent<RangedEnemy>();
        if (rangedEnemy != null)
        {
            rangedEnemy.TakeDamage(damage);
            hitEnemy = true;
        }

        if (hitEnemy)
        {
            if (isExplosive)
            {
                Explode();
            }
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        CameraFollow.Shake(0.18f, 0.25f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D col in colliders)
        {
            EnemyMovement em = col.GetComponentInParent<EnemyMovement>();
            if (em != null) em.TakeDamage((int)(damage * 0.75f));

            RangedEnemy re = col.GetComponentInParent<RangedEnemy>();
            if (re != null) re.TakeDamage((int)(damage * 0.75f));
        }
    }
}