using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    [Min(1)] public int damage = 10;
    private Vector2 targetPosition;

    void Start()
    {
        // Оқ атылған кезде ойыншының тұрған жерін есте сақтап, сол жаққа қарай ұшады
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject == null)
        {
            Debug.LogError("EnemyBullet: Player табылмады.");
            Destroy(gameObject);
            return;
        }

        Transform player = playerObject.transform;
        targetPosition = new Vector2(player.position.x, player.position.y);
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Мақсатқа жеткенде оқ өшіп қалады
        if ((Vector2)transform.position == targetPosition)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerStats playerStats = collision.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}