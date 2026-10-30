using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerStats playerStats = collision.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddXP(xpValue + playerStats.WaveXPBonusPerGem);
            Destroy(gameObject);
        }
    }
}