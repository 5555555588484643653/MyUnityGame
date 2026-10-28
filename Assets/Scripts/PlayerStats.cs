using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Негізгі көрсеткіштер")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;
    [SerializeField, Min(1)] private int maxHealth = 100;

    public Slider xpBar;
    public int CurrentHealth { get; private set; }
    public int Kills { get; private set; } = 0;

    // Қабілет деңгейлері
    public bool hasLifesteal = false;
    public bool hasMagnet = false;

    private bool isDead;
    private float survivalTimer = 0f;
    private AudioSource audioSource;
    private AudioClip levelUpClip;

    // UI Элементтері
    private GameObject levelUpCanvas;
    private GameObject cardContainer;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI killsText;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = maxHealth;
        Time.timeScale = 1f;
        SetupAudio();
    }

    private void Start()
    {
        if (xpBar != null)
        {
            xpBar.minValue = 0;
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXP;
        }

        CreateInGameHUD();
        CreateLevelUpUI();
    }

    private void Update()
    {
        if (isDead) return;

        survivalTimer += Time.deltaTime;
        UpdateHUD();

        // Егер Магнит қосулы болса, жақын маңдағы барлық XP кристалдарды ойыншыға тарту
        if (hasMagnet)
        {
            AttractGems();
        }
    }

    public void AddXP(int amount)
    {
        if (amount <= 0 || isDead) return;

        currentXP += amount;

        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXP;
        }

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead) return;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null && movement.IsInvincible) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        CameraFollow.Shake(0.15f, 0.2f);

        Debug.Log($"Ойыншы зақым алды. HP: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public void RegisterKill()
    {
        Kills++;

        // Lifesteal қабілеті: Жауды өлтірген сайын емделу
        if (hasLifesteal && Random.value < 0.35f)
        {
            Heal(8);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = (int)(xpToNextLevel * 1.5f) + 5;

        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXP;
        }

        PlayLevelUpSound();
        Debug.Log("ЖАҢА ДЕҢГЕЙ! Қазіргі деңгей: " + currentLevel);

        ShowLevelUpCards();
    }

    private void Die()
    {
        isDead = true;
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false;

        Debug.Log("Ойын аяқталды: ойыншы жеңілді.");
        Time.timeScale = 0f;
    }

    #region Жаңа Қабілеттерді Таңдау Жүйесі (3 Cards Choice)

    private void ShowLevelUpCards()
    {
        Time.timeScale = 0f; // Ойынды уақытша тоқтату
        if (levelUpCanvas != null)
        {
            levelUpCanvas.SetActive(true);
            PopulateCards();
        }
    }

    private void PopulateCards()
    {
        if (cardContainer == null) return;

        // Ескі карточкаларды тазалау
        foreach (Transform child in cardContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // 3 кездейсоқ қабілет ұсыну
        List<UpgradeOption> available = GetAvailableUpgrades();
        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int randIndex = Random.Range(0, available.Count);
            UpgradeOption opt = available[randIndex];
            available.RemoveAt(randIndex);

            CreateCardButton(opt, i);
        }
    }

    private struct UpgradeOption
    {
        public string title;
        public string desc;
        public string icon;
        public System.Action onApply;
    }

    private List<UpgradeOption> GetAvailableUpgrades()
    {
        PlayerAttack attack = GetComponent<PlayerAttack>();
        PlayerMovement movement = GetComponent<PlayerMovement>();

        List<UpgradeOption> list = new List<UpgradeOption>();

        list.Add(new UpgradeOption
        {
            icon = "⚡",
            title = "НАЙЗАҒАЙ ОРБИТАСЫ",
            desc = "Ойыншыны айналып ұшатын сиқырлы шарлар. Жақындаған жауларды күйдіреді.",
            onApply = () => { if (attack != null) attack.EnableLightningOrbs(); }
        });

        list.Add(new UpgradeOption
        {
            icon = "🎯",
            title = "ҮШТІК ОҚ (MULTISHOT)",
            desc = "Бір атқанда жан-жаққа қосымша оқтар боратады (+1 оқ).",
            onApply = () => { if (attack != null) attack.bulletCount++; }
        });

        list.Add(new UpgradeOption
        {
            icon = "🔥",
            title = "ЖАРЫЛҒЫШ ОҚТАР",
            desc = "Оқ тигенде жарылып, айналасындағы жауларға аймақтық зақым келтіреді.",
            onApply = () => { if (attack != null) attack.isExplosive = true; }
        });

        list.Add(new UpgradeOption
        {
            icon = "🩸",
            title = "ҚАНДЫ ОРАҚ (LIFESTEAL)",
            desc = "Жауды өлтірген сайын ойыншының денсаулығы (HP) қалпына келеді.",
            onApply = () => { hasLifesteal = true; }
        });

        list.Add(new UpgradeOption
        {
            icon = "⏩",
            title = "КӨЛЕҢКЕЛІ ЖЫЛДАМДЫҚ",
            desc = "Жүру жылдамдығы +25% артады. Dash қайта жүктелуі жеделдейді.",
            onApply = () => { if (movement != null) movement.SpeedMultiplier += 0.25f; }
        });

        list.Add(new UpgradeOption
        {
            icon = "💖",
            title = "МАКСИМАЛДЫ HP",
            desc = "+35 қосымша максималды денсаулық және HP толық емделеді.",
            onApply = () => { maxHealth += 35; Heal(maxHealth); }
        });

        list.Add(new UpgradeOption
        {
            icon = "🧲",
            title = "XP МАГНИТІ",
            desc = "Жердегі барлық тәжірибе кристалдары автоматты түрде ойыншыға тартылады.",
            onApply = () => { hasMagnet = true; }
        });

        return list;
    }

    private void CreateCardButton(UpgradeOption opt, int index)
    {
        GameObject card = new GameObject("Card_" + index, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        card.transform.SetParent(cardContainer.transform, false);

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 340);

        Image img = card.GetComponent<Image>();
        img.color = new Color(0.12f, 0.16f, 0.24f, 0.98f);

        Button btn = card.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            opt.onApply?.Invoke();
            levelUpCanvas.SetActive(false);
            Time.timeScale = 1f; // Ойынды жалғастыру
        });

        // Карточка мәтіндері
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(card.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-24, -24);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = $"<size=42>{opt.icon}</size>\n\n<size=18><b>{opt.title}</b></size>\n\n<size=14><color=#cbd5e1>{opt.desc}</color></size>";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    #endregion

    #region In-Game HUD Және UI

    private void CreateInGameHUD()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Таймер (Төбедегі ортада)
        GameObject timerObj = new GameObject("HUD_Timer", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        timerObj.transform.SetParent(canvas.transform, false);

        RectTransform timerRt = timerObj.GetComponent<RectTransform>();
        timerRt.anchorMin = new Vector2(0.5f, 1f);
        timerRt.anchorMax = new Vector2(0.5f, 1f);
        timerRt.pivot = new Vector2(0.5f, 1f);
        timerRt.anchoredPosition = new Vector2(0, -15);
        timerRt.sizeDelta = new Vector2(220, 45);

        timerText = timerObj.GetComponent<TextMeshProUGUI>();
        timerText.fontSize = 24;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = new Color(0.95f, 0.98f, 1f);

        // 2. Өлтірілгендер есептегіші (Kill Counter)
        GameObject killsObj = new GameObject("HUD_Kills", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        killsObj.transform.SetParent(canvas.transform, false);

        RectTransform killsRt = killsObj.GetComponent<RectTransform>();
        killsRt.anchorMin = new Vector2(1f, 1f);
        killsRt.anchorMax = new Vector2(1f, 1f);
        killsRt.pivot = new Vector2(1f, 1f);
        killsRt.anchoredPosition = new Vector2(-25, -15);
        killsRt.sizeDelta = new Vector2(200, 45);

        killsText = killsObj.GetComponent<TextMeshProUGUI>();
        killsText.fontSize = 20;
        killsText.alignment = TextAlignmentOptions.Right;
        killsText.color = new Color(1f, 0.4f, 0.4f);
    }

    private void UpdateHUD()
    {
        if (timerText != null)
        {
            int min = (int)(survivalTimer / 60);
            int sec = (int)(survivalTimer % 60);
            timerText.text = $"⏱️ {min:00}:{sec:00}";
        }

        if (killsText != null)
        {
            killsText.text = $"💀 {Kills}";
        }
    }

    private void CreateLevelUpUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        levelUpCanvas = new GameObject("LevelUpUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        levelUpCanvas.transform.SetParent(canvas.transform, false);

        RectTransform rt = levelUpCanvas.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image img = levelUpCanvas.GetComponent<Image>();
        img.color = new Color(0.02f, 0.04f, 0.08f, 0.88f);

        // Тақырып
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(levelUpCanvas.transform, false);

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -90);
        titleRt.sizeDelta = new Vector2(600, 70);

        TextMeshProUGUI tmp = titleObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "ЖАҢА ДЕҢГЕЙ!\n<size=18><color=#38bdf8>ҚАБІЛЕТ ТАҢДАҢЫЗ:</color></size>";
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.85f, 0.35f);

        // Карточкалар контейнері
        cardContainer = new GameObject("CardContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        cardContainer.transform.SetParent(levelUpCanvas.transform, false);

        RectTransform contRt = cardContainer.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0.5f, 0.5f);
        contRt.anchorMax = new Vector2(0.5f, 0.5f);
        contRt.anchoredPosition = new Vector2(0, -20);
        contRt.sizeDelta = new Vector2(850, 360);

        HorizontalLayoutGroup hlg = cardContainer.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 24;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        levelUpCanvas.SetActive(false);
    }

    private void AttractGems()
    {
        XPGem[] gems = FindObjectsByType<XPGem>(FindObjectsInactive.Exclude);
        foreach (XPGem gem in gems)
        {
            if (gem != null)
            {
                gem.transform.position = Vector2.MoveTowards(gem.transform.position, transform.position, 8f * Time.deltaTime);
            }
        }
    }

    #endregion

    #region Дыбыс

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        int sampleRate = 44100;
        int count = (int)(sampleRate * 0.45f);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(440f, 880f, (float)i / count);
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f;
        }

        levelUpClip = AudioClip.Create("SFX_LevelUp", count, 1, sampleRate, false);
        levelUpClip.SetData(samples, 0);
    }

    private void PlayLevelUpSound()
    {
        if (audioSource != null && levelUpClip != null)
        {
            audioSource.PlayOneShot(levelUpClip, 0.7f);
        }
    }

    #endregion
}