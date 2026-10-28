using UnityEngine;
using UnityEngine.UI; // UI элементтерімен жұмыс істеу үшін қажет

public class PlayerStats : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;

    public Slider xpBar; // Экрандағы жолақ
    [SerializeField, Min(1)] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }
    private bool isDead;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = maxHealth;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (xpBar != null)
        {
            xpBar.minValue = 0;
            xpBar.maxValue = maxHealth;
            xpBar.wholeNumbers = true;
            xpBar.interactable = false;
            xpBar.value = CurrentHealth;
        }
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentXP += amount;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (xpBar != null)
        {
            xpBar.value = CurrentHealth;
        }

        Debug.Log($"Ойыншы зақым алды. Қалған HP: {CurrentHealth}/{maxHealth}");
        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.enabled = false;
        }

        Debug.Log("Ойын аяқталды: ойыншының HP-і таусылды.");
        Time.timeScale = 0f;
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0;
        xpToNextLevel += 5; // Әр деңгей сайын кристалл көп керек болады

        Debug.Log("ЖАҢА ДЕҢГЕЙ! Қазіргі деңгей: " + currentLevel);
    }
}