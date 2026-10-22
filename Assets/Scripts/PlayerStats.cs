using UnityEngine;
using UnityEngine.UI; // UI элементтерімен жұмыс істеу үшін қажет

public class PlayerStats : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;

    public Slider xpBar; // Экрандағы жолақ

    private void Start()
    {
        // Ойын басталғанда жолақты баптау
        xpBar.maxValue = xpToNextLevel;
        xpBar.value = currentXP;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        xpBar.value = currentXP; // Экрандағы жолақты толтыру

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0;
        xpToNextLevel += 5; // Әр деңгей сайын кристалл көп керек болады

        xpBar.maxValue = xpToNextLevel;
        xpBar.value = currentXP;

        Debug.Log("ЖАҢА ДЕҢГЕЙ! Қазіргі деңгей: " + currentLevel);
    }
}