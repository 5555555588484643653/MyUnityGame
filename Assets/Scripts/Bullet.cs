using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 100;
    private Vector3 targetDir;

    public void Setup(Vector3 direction)
    {
        targetDir = direction;
        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        transform.position += targetDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RangedEnemy enemy = collision.GetComponentInParent<RangedEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}