using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player" || collision.CompareTag("Player"))
        {
            // Ойыншының ішінен PlayerStats скриптін тауып, XP қосу
            collision.GetComponent<PlayerStats>().AddXP(xpValue);
            Destroy(gameObject);
        }
    }
}