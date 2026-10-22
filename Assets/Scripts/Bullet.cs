using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public GameObject xpPrefab; // XP шаблонын салатын орын
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
        if (collision.CompareTag("Enemy"))
        {
            // Жаудың дәл сол тұрған орнына XP кристалын шығару
            if (xpPrefab != null)
            {
                Instantiate(xpPrefab, collision.transform.position, Quaternion.identity);
            }

            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}