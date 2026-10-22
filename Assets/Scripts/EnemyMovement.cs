using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f; // Жаудың жылдамдығы
    private Transform player;

    private void Start()
    {
        // Ойын басталғанда "Player" деген нысанды автоматты түрде тауып алады
        player = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        if (player != null)
        {
            // Жауды кейіпкердің орналасқан нүктесіне қарай тұрақты түрде жылжыту
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }
}   