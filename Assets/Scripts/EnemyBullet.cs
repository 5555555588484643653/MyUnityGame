using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 targetPosition;

    void Start()
    {
        // Оқ атылған кезде ойыншының тұрған жерін есте сақтап, сол жаққа қарай ұшады
        Transform player = GameObject.Find("Player").transform;
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
        if (collision.name == "Player")
        {
            Debug.Log("Ойыншыға оқ тиді! Өмір азайды.");
            Destroy(gameObject);
        }
    }
}