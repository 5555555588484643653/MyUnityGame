﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    private const string SavePrefix = "ZombieSurvival.";
    private const string BestZombieKillsKey = SavePrefix + "BestZombieKills";
    private const string BestPlayerLevelKey = SavePrefix + "BestPlayerLevel";

    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;

    [SerializeField] private Slider healthBar;
    [SerializeField, Min(1)] private int maxHealth = 100;

    public Slider xpBar;
    public int CurrentHealth { get; private set; }
    public int Kills { get; private set; }
    public int MaxHealth => maxHealth;
    public int MissionLevel { get; private set; } = 1;
    public int ZombiesKilledThisLevel { get; private set; }
    public int ZombiesKilledThisRun { get; private set; }
    public int WaveDamageBonus { get; private set; }
    public int WaveXPBonusPerGem { get; private set; }
    public GameHUD Hud => gameHUD;
    public static int BestZombieKills => PlayerPrefs.GetInt(BestZombieKillsKey, 0);
    public static int BestPlayerLevel => PlayerPrefs.GetInt(BestPlayerLevelKey, 1);
    public int ZombiesRequiredForMissionLevel => 3 + (MissionLevel - 1) * 2;
    public string MissionTitle => MissionLevel switch
    {
        1 => "ОҚУ-ЖАТТЫҒУ",
        2 => "АЛҒАШҚЫ ҰРЫС",
        3 => "ЗОМБИЛЕРГЕ ҚАРСЫ",
        _ => "ҚАРСЫЛЫҚТЫ БҰЗ"
    };

    private bool isDead;
    private int pendingLevelUps;
    private SpriteRenderer playerSprite;
    private Vector3 originalVisualScale;
    private Quaternion originalVisualRotation;
    private Color originalVisualColor;
    private Coroutine hurtAnimation;
    private Animator playerAnimator;
    private GameHUD gameHUD;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        xpToNextLevel = Mathf.Max(1, xpToNextLevel);
        LoadProgress();
        CurrentHealth = maxHealth;
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        if (playerSprite != null)
        {
            originalVisualScale = playerSprite.transform.localScale;
            originalVisualRotation = playerSprite.transform.localRotation;
            originalVisualColor = playerSprite.color;
        }
        Time.timeScale = 1f;
        UpdateBestPlayerLevel();
    }

    private void Start()
    {
        gameHUD = GetComponent<GameHUD>();
        if (gameHUD == null)
        {
            gameHUD = gameObject.AddComponent<GameHUD>();
        }

        gameHUD.Initialize(this, healthBar);
        if (pendingLevelUps > 0)
        {
            gameHUD.ShowLevelUp(currentLevel - pendingLevelUps + 1, ChooseUpgrade);
        }
    }

    public void AddXP(int amount)
    {
        if (amount <= 0 || isDead)
        {
            return;
        }

        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel += 5;
            pendingLevelUps++;
        }

        UpdateBestPlayerLevel();
        SaveProgress();
        if (pendingLevelUps > 0 && gameHUD != null)
        {
            gameHUD.ShowLevelUp(currentLevel - pendingLevelUps + 1, ChooseUpgrade);
        }
    }

    public void RegisterZombieKill()
    {
        if (isDead)
        {
            return;
        }

        ZombiesKilledThisLevel++;
        ZombiesKilledThisRun++;
        UpdateBestZombieKills();
        if (ZombiesKilledThisLevel < ZombiesRequiredForMissionLevel)
        {
            SaveProgress();
            return;
        }

        MissionLevel++;
        ZombiesKilledThisLevel = 0;
        SaveProgress();
        Debug.Log($"Миссия деңгейі көтерілді: {MissionLevel}. Келесі мақсат: " +
            $"{ZombiesRequiredForMissionLevel} зомби.");
    }

    public void ApplyWaveBonus()
    {
        const int damagePerWave = 5;
        const int xpPerWave = 5;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.UpgradeDamage(damagePerWave);
        }

        WaveDamageBonus += damagePerWave;
        WaveXPBonusPerGem += xpPerWave;
        Debug.Log($"Толқын бонусы: шабуыл +{damagePerWave}, әр XP тасына +{xpPerWave} XP.");
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead) return;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null && movement.IsInvincible) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        Debug.Log($"Ойыншы зақым алды. Қалған HP: {CurrentHealth}/{maxHealth}");
        if (CurrentHealth == 0)
        {
            Die();
            return;
        }

        if (hurtAnimation != null)
        {
            StopCoroutine(hurtAnimation);
        }

        hurtAnimation = StartCoroutine(PlayHurtAnimation());
    }

    public void Heal(int amount)
    {
        if (amount > 0 && !isDead)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        }
    }

    public void RegisterKill()
    {
        if (!isDead)
        {
            Kills++;
        }
    }

    private void Die()
    {
        isDead = true;
        UpdateBestZombieKills();
        ClearRunProgress();
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        if (hurtAnimation != null)
        {
            StopCoroutine(hurtAnimation);
            hurtAnimation = null;
        }

        if (playerSprite != null)
        {
            playerSprite.enabled = true;
            playerSprite.color = originalVisualColor;
            playerSprite.transform.localScale = originalVisualScale;
            playerSprite.transform.localRotation = originalVisualRotation;
        }

        if (playerAnimator != null)
        {
            playerAnimator.ResetTrigger("Attack");
            playerAnimator.ResetTrigger("Special");
            playerAnimator.SetTrigger("Death");
        }
        else
        {
            Debug.LogWarning("Ойыншыда Animator жоқ, өлу анимациясын ойнату мүмкін болмады.");
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }
        Debug.Log("Ойын аяқталды: ойыншының HP-і таусылды.");
        StartCoroutine(ShowGameOverAfterDeathAnimation());
    }

    private IEnumerator ShowGameOverAfterDeathAnimation()
    {
        yield return new WaitForSecondsRealtime(1.9f);
        Time.timeScale = 0f;
        gameHUD.ShowGameOver();
    }

    private IEnumerator PlayHurtAnimation()
    {
        if (playerSprite == null)
        {
            yield break;
        }

        Transform visual = playerSprite.transform;
        const float duration = 0.24f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float pulse = Mathf.Sin(progress * Mathf.PI);
            playerSprite.color = Color.Lerp(originalVisualColor, new Color(1f, 0.2f, 0.2f), pulse);
            visual.localScale = originalVisualScale * (1f - pulse * 0.08f);
            visual.localRotation = originalVisualRotation *
                Quaternion.Euler(0f, 0f, Mathf.Sin(progress * Mathf.PI * 4f) * 5f * (1f - progress));
            yield return null;
        }

        playerSprite.color = originalVisualColor;
        visual.localScale = originalVisualScale;
        visual.localRotation = originalVisualRotation;
        hurtAnimation = null;
    }

    private void ChooseUpgrade(int choice)
    {
        switch (choice)
        {
            case 0:
                PlayerAttack attack = GetComponent<PlayerAttack>();
                if (attack != null)
                {
                    attack.UpgradeDamage(10);
                }
                break;
            case 1:
                PlayerMovement movement = GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.UpgradeSpeed(0.75f);
                }
                break;
            case 2:
                maxHealth += 25;
                CurrentHealth = maxHealth;
                break;
            default:
                Debug.LogError($"Белгісіз дағды таңдауы: {choice}");
                Time.timeScale = 1f;
                return;
        }

        pendingLevelUps--;
        SaveProgress();
        if (pendingLevelUps > 0)
        {
            gameHUD.ShowLevelUp(currentLevel - pendingLevelUps + 1, ChooseUpgrade);
            return;
        }

        Time.timeScale = 1f;
        Debug.Log($"ЖАҢА ДЕҢГЕЙ! Қазіргі деңгей: {currentLevel}");
    }

    public static void BeginNewRun()
    {
        ClearRunProgress();
    }

    private static void ClearRunProgress()
    {
        PlayerPrefs.DeleteKey(SavePrefix + "PlayerLevel");
        PlayerPrefs.DeleteKey(SavePrefix + "CurrentXP");
        PlayerPrefs.DeleteKey(SavePrefix + "XPToNextLevel");
        PlayerPrefs.DeleteKey(SavePrefix + "MissionLevel");
        PlayerPrefs.DeleteKey(SavePrefix + "MissionKills");
        PlayerPrefs.DeleteKey(SavePrefix + "RunZombieKills");
        PlayerPrefs.DeleteKey(SavePrefix + "MaxHealth");
        PlayerPrefs.DeleteKey(SavePrefix + "PendingLevelUps");
        PlayerPrefs.DeleteKey(SavePrefix + "AttackDamage");
        PlayerPrefs.DeleteKey(SavePrefix + "MoveSpeed");
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        currentLevel = Mathf.Max(1, PlayerPrefs.GetInt(SavePrefix + "PlayerLevel", currentLevel));
        currentXP = Mathf.Max(0, PlayerPrefs.GetInt(SavePrefix + "CurrentXP", currentXP));
        xpToNextLevel = Mathf.Max(1, PlayerPrefs.GetInt(SavePrefix + "XPToNextLevel", xpToNextLevel));
        MissionLevel = Mathf.Max(1, PlayerPrefs.GetInt(SavePrefix + "MissionLevel", MissionLevel));
        ZombiesKilledThisLevel = Mathf.Clamp(PlayerPrefs.GetInt(
            SavePrefix + "MissionKills", ZombiesKilledThisLevel), 0, ZombiesRequiredForMissionLevel - 1);
        ZombiesKilledThisRun = Mathf.Max(0, PlayerPrefs.GetInt(SavePrefix + "RunZombieKills", 0));
        maxHealth = Mathf.Max(1, PlayerPrefs.GetInt(SavePrefix + "MaxHealth", maxHealth));
        pendingLevelUps = Mathf.Max(0, PlayerPrefs.GetInt(SavePrefix + "PendingLevelUps", 0));

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.RestoreDamage(PlayerPrefs.GetInt(SavePrefix + "AttackDamage", attack.Damage));
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.RestoreSpeed(PlayerPrefs.GetFloat(SavePrefix + "MoveSpeed", movement.Speed));
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(SavePrefix + "PlayerLevel", currentLevel);
        PlayerPrefs.SetInt(SavePrefix + "CurrentXP", currentXP);
        PlayerPrefs.SetInt(SavePrefix + "XPToNextLevel", xpToNextLevel);
        PlayerPrefs.SetInt(SavePrefix + "MissionLevel", MissionLevel);
        PlayerPrefs.SetInt(SavePrefix + "MissionKills", ZombiesKilledThisLevel);
        PlayerPrefs.SetInt(SavePrefix + "RunZombieKills", ZombiesKilledThisRun);
        PlayerPrefs.SetInt(SavePrefix + "MaxHealth", maxHealth);
        PlayerPrefs.SetInt(SavePrefix + "PendingLevelUps", pendingLevelUps);

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            PlayerPrefs.SetInt(SavePrefix + "AttackDamage", attack.Damage);
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            PlayerPrefs.SetFloat(SavePrefix + "MoveSpeed", movement.Speed);
        }

        PlayerPrefs.Save();
    }

    private void UpdateBestZombieKills()
    {
        if (ZombiesKilledThisRun > BestZombieKills)
        {
            PlayerPrefs.SetInt(BestZombieKillsKey, ZombiesKilledThisRun);
        }
    }

    private void UpdateBestPlayerLevel()
    {
        if (currentLevel > BestPlayerLevel)
        {
            PlayerPrefs.SetInt(BestPlayerLevelKey, currentLevel);
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveProgress();
        }
    }

    private void OnApplicationQuit()
    {
        UpdateBestZombieKills();
        SaveProgress();
    }

}
