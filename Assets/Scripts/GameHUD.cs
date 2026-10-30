using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    private static readonly Color PanelColor = new(0.06f, 0.08f, 0.13f, 0.94f);
    private static readonly Color AccentColor = new(0.8f, 0.12f, 0.16f, 1f);

    private PlayerStats playerStats;
    private Transform player;
    private Slider healthBar;
    private Text healthText;
    private Text missionTitle;
    private Text missionProgress;
    private Text recordText;
    private Text waveText;
    private Text waveDetailText;
    private Text specialAttackText;
    private Text waveAnnouncementText;
    private Text upgradeTitle;
    private TMP_Text gameOverTitle;
    private TMP_Text gameOverSubtitle;
    private TMP_Text bossResultText;
    private TMP_Text zombieResultValue;
    private TMP_Text levelResultValue;
    private TMP_Text xpResultValue;
    private TMP_Text timeResultValue;
    private TMP_Text recordResultText;
    private TMP_Text restartButtonText;
    private Image bossResultPanel;
    private Image resultAccentImage;
    private GameObject upgradeOverlay;
    private GameObject gameOverOverlay;
    private GameObject waveAnnouncement;
    private CanvasGroup waveAnnouncementGroup;
    private Action<int> onUpgradeSelected;
    private PlayerAttack playerAttack;
    private Camera minimapCamera;
    private RenderTexture minimapTexture;
    private SpriteRenderer playerSpriteRenderer;
    private Image minimapPlayerMarker;

    public void Initialize(PlayerStats stats, Slider existingHealthBar)
    {
        playerStats = stats;
        player = stats.transform;
        playerAttack = stats.GetComponent<PlayerAttack>();
        playerSpriteRenderer = stats.GetComponentInChildren<SpriteRenderer>();

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new("Game HUD Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        CreateHealthBar(canvas.transform, existingHealthBar);
        CreateWavePanel(canvas.transform);
        CreateMissionPanel(canvas.transform);
        CreateMinimap(canvas.transform);
        CreateWaveAnnouncement(canvas.transform);
        CreateUpgradeOverlay(canvas.transform);
        CreateGameOverOverlay(canvas.transform);
    }

    private void Update()
    {
        if (playerStats == null)
        {
            return;
        }

        if (healthBar != null)
        {
            healthBar.maxValue = playerStats.MaxHealth;
            healthBar.value = playerStats.CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"HP  {playerStats.CurrentHealth} / {playerStats.MaxHealth}";
        }

        if (missionTitle != null)
        {
            missionTitle.text = $"{playerStats.MissionLevel}-ДЕҢГЕЙ  •  {playerStats.MissionTitle}";
        }

        if (missionProgress != null)
        {
            missionProgress.text =
                $"ЗОМБИ {playerStats.ZombiesKilledThisLevel}/{playerStats.ZombiesRequiredForMissionLevel}" +
                $"  |  XP {playerStats.currentXP}/{playerStats.xpToNextLevel}";
        }

        if (recordText != null)
        {
            recordText.text =
                $"РЕКОРД: {PlayerStats.BestZombieKills} ЗОМБИ  |  ДЕҢГЕЙ {PlayerStats.BestPlayerLevel}";
        }

        if (specialAttackText != null && playerAttack != null)
        {
            float cooldown = playerAttack.SpecialCooldownRemaining;
            specialAttackText.text = cooldown <= 0f
                ? "Q  СУПЕР ШАБУЫЛ: ДАЙЫН"
                : $"Q  СУПЕР ШАБУЫЛ: {cooldown:0.0} С";
        }

        if (minimapCamera != null && player != null)
        {
            Vector3 position = player.position;
            minimapCamera.transform.position = new Vector3(position.x, position.y, -20f);
        }

        if (minimapPlayerMarker != null && playerSpriteRenderer != null)
        {
            if (minimapPlayerMarker.sprite != playerSpriteRenderer.sprite)
            {
                minimapPlayerMarker.sprite = playerSpriteRenderer.sprite;
            }

            minimapPlayerMarker.rectTransform.localScale =
                new Vector3(playerSpriteRenderer.flipX ? -1f : 1f, 1f, 1f);
        }
    }

    private void OnDestroy()
    {
        if (minimapCamera != null)
        {
            Destroy(minimapCamera.gameObject);
        }

        if (minimapTexture != null)
        {
            minimapTexture.Release();
            Destroy(minimapTexture);
        }
    }

    public void ShowLevelUp(int level, Action<int> onSelected)
    {
        onUpgradeSelected = onSelected;
        upgradeTitle.text = $"ЖАҢА ДЕҢГЕЙ {level} — ДАҒДЫ ТАҢДА";
        upgradeOverlay.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowGameOver()
    {
        upgradeOverlay.SetActive(false);
        gameOverTitle.text = "ОЙЫН АЯҚТАЛДЫ";
        gameOverSubtitle.text = "КЕЛЕСІ ЖОЛЫ ЖЕҢІСКЕ ЖЕТЕСІҢ";
        bossResultText.text = "ҰРЫС АЯҚТАЛДЫ";
        bossResultPanel.color = new Color(0.35f, 0.13f, 0.18f, 0.96f);
        resultAccentImage.color = new Color(0.88f, 0.24f, 0.28f, 1f);
        restartButtonText.text = "ҚАЙТА ОЙНАУ";
        UpdateRunResults();
        gameOverOverlay.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetWaveStatus(int wave, int totalWaves, string detail)
    {
        if (waveText != null)
        {
            waveText.text = $"ТОЛҚЫН {wave}/{totalWaves}";
        }

        if (waveDetailText != null)
        {
            waveDetailText.text = detail;
        }
    }

    public void ShowWaveAnnouncement(int wave, bool isBossWave, int damageBonus, int xpBonus)
    {
        if (waveAnnouncementText == null)
        {
            return;
        }

        waveAnnouncementText.text = isBossWave
            ? $"5-ТОЛҚЫН БАСТАЛУДА!!!\nБОССҚА ДАЙЫНДАЛ!\nШАБУЫЛ +{damageBonus} | ӘР ЗОМБИДАН XP +{xpBonus}"
            : $"{wave}-ТОЛҚЫН БАСТАЛУДА!!!\nШАБУЫЛ +{damageBonus} | ӘР ЗОМБИДАН XP +{xpBonus}";
        waveAnnouncement.SetActive(true);
        if (waveAnnouncementGroup != null)
        {
            waveAnnouncementGroup.alpha = 1f;
        }

        StopCoroutine(nameof(HideWaveAnnouncement));
        StartCoroutine(HideWaveAnnouncement());
    }

    public void ShowVictory()
    {
        upgradeOverlay.SetActive(false);
        gameOverTitle.text = "СЕН ЖЕҢДІҢ!";
        gameOverSubtitle.text = "5 ТОЛҚЫННАН ӨТТІҢ";
        bossResultText.text = "БОСС ЖЕҢІЛДІ  •  ҚҰТТЫҚТАЙМЫЗ!";
        bossResultPanel.color = new Color(0.12f, 0.38f, 0.24f, 0.96f);
        resultAccentImage.color = new Color(0.3f, 0.82f, 0.48f, 1f);
        restartButtonText.text = "ҚАЙТА ОЙНАУ";
        UpdateRunResults();
        gameOverOverlay.SetActive(true);
        PlayerMovement movement = playerStats.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateRunResults()
    {
        zombieResultValue.text = playerStats.ZombiesKilledThisRun.ToString();
        levelResultValue.text = playerStats.currentLevel.ToString();
        xpResultValue.text = $"{playerStats.currentXP}/{playerStats.xpToNextLevel}";
        timeResultValue.text = FormatDuration(Time.timeSinceLevelLoad);
        recordResultText.text =
            $"РЕКОРД  {PlayerStats.BestZombieKills} ЗОМБИ   •   ЕҢ ЖОҒАРЫ ДЕҢГЕЙ  {PlayerStats.BestPlayerLevel}";
    }

    private static string FormatDuration(float elapsedSeconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private void CreateWavePanel(Transform parent)
    {
        RectTransform panel = CreateRect(parent, "WavePanel", new Vector2(24, -82),
            new Vector2(470, 116), new Vector2(0, 1), new Vector2(0, 1));
        panel.pivot = new Vector2(0, 1);
        panel.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.04f, 0.065f, 0.84f);

        waveText = CreateText(panel, "WaveText", "ТОЛҚЫН 1/5", new Vector2(0, -25),
            new Vector2(450, 38), 28, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        waveText.fontStyle = FontStyle.Bold;
        AddTextOutline(waveText);

        waveDetailText = CreateText(panel, "WaveDetailText", "ДАЙЫНДАЛ...",
            new Vector2(0, -62), new Vector2(450, 30), 20, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        waveDetailText.fontStyle = FontStyle.Bold;
        AddTextOutline(waveDetailText);

        specialAttackText = CreateText(panel, "SpecialAttackText", "Q  СУПЕР ШАБУЫЛ",
            new Vector2(0, -94), new Vector2(450, 28), 19, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        specialAttackText.fontStyle = FontStyle.Bold;
        AddTextOutline(specialAttackText);
    }

    private void CreateWaveAnnouncement(Transform parent)
    {
        RectTransform panel = CreateRect(parent, "WaveAnnouncement", Vector2.zero,
            new Vector2(1100, 220), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panel.gameObject.AddComponent<Image>().color = new Color(0.015f, 0.025f, 0.045f, 0.9f);
        waveAnnouncementGroup = panel.gameObject.AddComponent<CanvasGroup>();
        waveAnnouncement = panel.gameObject;

        waveAnnouncementText = CreateText(panel, "WaveAnnouncementText", string.Empty,
            Vector2.zero, new Vector2(1060, 190), 48, TextAnchor.MiddleCenter);
        waveAnnouncementText.fontStyle = FontStyle.Bold;
        waveAnnouncementText.resizeTextForBestFit = true;
        waveAnnouncementText.resizeTextMinSize = 32;
        waveAnnouncementText.resizeTextMaxSize = 48;
        AddTextOutline(waveAnnouncementText);
        waveAnnouncement.SetActive(false);
    }

    private System.Collections.IEnumerator HideWaveAnnouncement()
    {
        yield return new WaitForSecondsRealtime(1.8f);
        float elapsed = 0f;
        const float fadeDuration = 0.35f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            waveAnnouncementGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        waveAnnouncement.SetActive(false);
    }

    private void CreateHealthBar(Transform parent, Slider existingHealthBar)
    {
        if (existingHealthBar == null)
        {
            RectTransform root = CreateRect(parent, "HealthBar", new Vector2(24, -24),
                new Vector2(320, 30), new Vector2(0, 1), new Vector2(0, 1));
            root.gameObject.AddComponent<Image>().color = new Color(0.13f, 0.04f, 0.05f, 0.95f);
            healthBar = root.gameObject.AddComponent<Slider>();

            RectTransform fill = CreateRect(root, "Fill", Vector2.zero, Vector2.zero,
                Vector2.zero, Vector2.one);
            fill.offsetMin = new Vector2(3, 3);
            fill.offsetMax = new Vector2(-3, -3);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = AccentColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            healthBar.fillRect = fill;
            healthBar.targetGraphic = fillImage;
        }
        else
        {
            healthBar = existingHealthBar;
            healthBar.gameObject.name = "HealthBar";
            RectTransform root = healthBar.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(0, 1);
            root.anchorMax = new Vector2(0, 1);
            root.pivot = new Vector2(0, 1);
            root.anchoredPosition = new Vector2(24, -24);
            root.sizeDelta = new Vector2(320, 30);

            Image background = healthBar.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(0.13f, 0.04f, 0.05f, 0.95f);
            }

            if (healthBar.fillRect != null)
            {
                Image fill = healthBar.fillRect.GetComponent<Image>();
                if (fill != null)
                {
                    fill.color = AccentColor;
                }
            }
        }

        healthBar.minValue = 0;
        healthBar.wholeNumbers = true;
        healthBar.interactable = false;
        healthBar.handleRect = null;
        healthText = CreateText(parent, "HealthText", "HP", new Vector2(244, -26),
            new Vector2(420, 42), 36, TextAnchor.MiddleCenter);
        AddTextOutline(healthText);
    }

    private void CreateMissionPanel(Transform parent)
    {
        RectTransform panel = CreateRect(parent, "MissionPanel", new Vector2(-24, -320),
            new Vector2(560, 132), Vector2.one, Vector2.one);
        panel.pivot = Vector2.one;

        missionTitle = CreateText(panel, "MissionTitle", string.Empty, new Vector2(0, -25),
            new Vector2(540, 44), 33, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        missionTitle.fontStyle = FontStyle.Bold;
        AddTextOutline(missionTitle);

        missionProgress = CreateText(panel, "MissionProgress", string.Empty, new Vector2(0, -70),
            new Vector2(540, 36), 28, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        missionProgress.fontStyle = FontStyle.Bold;
        AddTextOutline(missionProgress);

        recordText = CreateText(panel, "RecordText", string.Empty, new Vector2(0, -111),
            new Vector2(540, 32), 24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        recordText.fontStyle = FontStyle.Bold;
        AddTextOutline(recordText);
    }

    private void CreateMinimap(Transform parent)
    {
        Canvas canvas = parent.GetComponent<Canvas>();
        float canvasScale = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
        float targetSize = Mathf.Clamp(
            Mathf.Min(Screen.width, Screen.height) * 0.32f,
            120f,
            360f);
        float frameSize = targetSize / canvasScale;

        RectTransform frame = CreateRect(parent, "MinimapFrame", new Vector2(-24, -24),
            new Vector2(frameSize, frameSize), Vector2.one, Vector2.one);
        frame.pivot = Vector2.one;
        frame.gameObject.AddComponent<Image>().color = PanelColor;

        RectTransform mapRect = CreateRect(frame, "MinimapView", Vector2.zero,
            new Vector2(frameSize - 16f, frameSize - 16f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RawImage mapImage = mapRect.gameObject.AddComponent<RawImage>();
        mapImage.raycastTarget = false;

        minimapTexture = new RenderTexture(512, 512, 16)
        {
            name = "Runtime Minimap",
            antiAliasing = 1,
            filterMode = FilterMode.Bilinear
        };
        minimapTexture.Create();
        mapImage.texture = minimapTexture;

        GameObject cameraObject = new("Minimap Camera");
        minimapCamera = cameraObject.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 9f;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 1f);
        minimapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
        minimapCamera.targetTexture = minimapTexture;
        minimapCamera.depth = -2;

        RectTransform playerMarker = CreateRect(frame, "PlayerMarker", Vector2.zero,
            new Vector2(frameSize * 0.12f, frameSize * 0.16f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        minimapPlayerMarker = playerMarker.gameObject.AddComponent<Image>();
        minimapPlayerMarker.preserveAspect = true;
        minimapPlayerMarker.raycastTarget = false;
        CreateText(frame, "MinimapLabel", "КАРТА", new Vector2(-8, -8),
            new Vector2(120, 32), 22, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(1, 1));
    }

    private void CreateUpgradeOverlay(Transform parent)
    {
        RectTransform overlay = CreateRect(parent, "LevelUpOverlay", Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.one);
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.72f);
        upgradeOverlay = overlay.gameObject;

        RectTransform card = CreateRect(overlay, "LevelUpCard", Vector2.zero,
            new Vector2(860, 360), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.13f, 1f);
        upgradeTitle = CreateText(card, "LevelUpTitle", string.Empty, new Vector2(0, 136),
            new Vector2(800, 68), 36, TextAnchor.MiddleCenter);

        string[] labels =
        {
            "ШАБУЫЛ КҮШІ\n+10 ЗИЯН",
            "ҚОЗҒАЛУ\n+0.75 ЖЫЛДАМДЫҚ",
            "ДЕНСАУЛЫҚ\n+25 HP\nТОЛЫҚ ЕМДЕЛУ"
        };

        for (int i = 0; i < labels.Length; i++)
        {
            int choice = i;
            Button button = CreateButton(card, $"Upgrade{choice}", labels[choice],
                new Vector2(-280 + 280 * choice, -24), new Vector2(250, 178), 30);
            button.onClick.AddListener(() => SelectUpgrade(choice));
        }

        upgradeOverlay.SetActive(false);
    }

    private void CreateGameOverOverlay(Transform parent)
    {
        RectTransform overlay = CreateRect(parent, "GameOverOverlay", Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.one);
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.gameObject.AddComponent<Image>().color = new Color(0.008f, 0.016f, 0.03f, 0.84f);
        gameOverOverlay = overlay.gameObject;

        RectTransform card = CreateRect(overlay, "GameOverCard", Vector2.zero,
            new Vector2(1120, 800), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.043f, 0.073f, 0.98f);

        RectTransform topAccent = CreateRect(card, "ResultAccent",
            new Vector2(0, 396), new Vector2(1120, 8),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        resultAccentImage = topAccent.gameObject.AddComponent<Image>();
        resultAccentImage.color = new Color(0.3f, 0.82f, 0.48f, 1f);

        gameOverTitle = CreateResultText(card, "GameOverTitle", "ОЙЫН АЯҚТАЛДЫ",
            new Vector2(0, 310), new Vector2(1000, 92), 68, Color.white);
        gameOverSubtitle = CreateResultText(card, "GameOverSubtitle", string.Empty,
            new Vector2(0, 250), new Vector2(900, 48), 30, new Color(0.7f, 0.76f, 0.84f));

        RectTransform divider = CreateRect(card, "ResultDivider", new Vector2(0, 208),
            new Vector2(760, 2), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        divider.gameObject.AddComponent<Image>().color = new Color(0.26f, 0.35f, 0.47f, 0.8f);

        RectTransform bossBanner = CreateRect(card, "BossResultBanner", new Vector2(0, 135),
            new Vector2(920, 88), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        bossResultPanel = bossBanner.gameObject.AddComponent<Image>();
        bossResultPanel.color = new Color(0.12f, 0.38f, 0.24f, 0.96f);
        TMP_Text bossBannerText = CreateResultText(bossBanner, "BossResultText", string.Empty,
            Vector2.zero, new Vector2(880, 72), 30, Color.white);
        bossResultText = bossBannerText;

        zombieResultValue = CreateResultStat(card, "Zombies", "ЗОМБИ ӨЛТІРІЛДІ",
            new Vector2(-360, -34));
        levelResultValue = CreateResultStat(card, "Level", "ЖЕТКЕН ДЕҢГЕЙ",
            new Vector2(-120, -34));
        xpResultValue = CreateResultStat(card, "XP", "XP / КЕЛЕСІ ДЕҢГЕЙ",
            new Vector2(120, -34));
        timeResultValue = CreateResultStat(card, "Time", "ҰРЫС УАҚЫТЫ",
            new Vector2(360, -34));
        recordResultText = CreateResultText(card, "RecordResult",
            string.Empty, new Vector2(0, -184), new Vector2(980, 48), 24,
            new Color(0.65f, 0.74f, 0.85f));

        Button restartButton = CreateResultButton(card, "RestartButton", "ҚАЙТА ОЙНАУ",
            new Vector2(-205, -314), new Vector2(360, 72), true);
        restartButtonText = restartButton.GetComponentInChildren<TMP_Text>();
        restartButton.onClick.AddListener(RestartGame);

        Button menuButton = CreateResultButton(card, "MainMenuButton", "БАС МӘЗІР",
            new Vector2(205, -314), new Vector2(360, 72), false);
        menuButton.onClick.AddListener(ReturnToMainMenu);
        gameOverOverlay.SetActive(false);
    }

    private TMP_Text CreateResultStat(Transform parent, string name, string label, Vector2 position)
    {
        RectTransform panel = CreateRect(parent, $"{name}ResultCard", position,
            new Vector2(220, 148), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panel.gameObject.AddComponent<Image>().color = new Color(0.055f, 0.082f, 0.12f, 1f);
        CreateResultText(panel, $"{name}ResultLabel", label, new Vector2(0, 39),
            new Vector2(204, 40), 17, new Color(0.64f, 0.72f, 0.82f));
        return CreateResultText(panel, $"{name}ResultValue", "0", new Vector2(0, -24),
            new Vector2(204, 62), 38, Color.white);
    }

    private static Button CreateResultButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Vector2 size,
        bool primary)
    {
        RectTransform rect = CreateRect(parent, name, position, size,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = primary
            ? new Color(0.23f, 0.62f, 0.36f, 1f)
            : new Color(0.12f, 0.17f, 0.25f, 1f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = primary
            ? new Color(0.3f, 0.76f, 0.45f, 1f)
            : new Color(0.19f, 0.27f, 0.39f, 1f);
        colors.pressedColor = primary
            ? new Color(0.16f, 0.48f, 0.28f, 1f)
            : new Color(0.09f, 0.13f, 0.2f, 1f);
        button.colors = colors;

        TMP_Text text = CreateResultText(rect, $"{name}Text", label, Vector2.zero,
            size - new Vector2(24, 12), 25, Color.white);
        text.fontStyle = FontStyles.Bold;
        return button;
    }

    private static TMP_Text CreateResultText(
        Transform parent,
        string name,
        string value,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color)
    {
        RectTransform rect = CreateRect(parent, name, position, size,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (text.font == null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static void RestartGame()
    {
        PlayerStats.BeginNewRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void SelectUpgrade(int choice)
    {
        upgradeOverlay.SetActive(false);
        onUpgradeSelected?.Invoke(choice);
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, int fontSize)
    {
        RectTransform rect = CreateRect(parent, name, position, size,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.34f, 1f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateText(rect, $"{name}Text", label, Vector2.zero, size, fontSize, TextAnchor.MiddleCenter);
        return button;
    }

    private static Text CreateText(Transform parent, string name, string value,
        Vector2 position, Vector2 size, int fontSize, TextAnchor alignment,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        RectTransform rect = CreateRect(parent, name, position, size,
            anchorMin ?? new Vector2(0.5f, 0.5f), anchorMax ?? new Vector2(0.5f, 0.5f));
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void AddTextOutline(Text text)
    {
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 position,
        Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject child = new(name, typeof(RectTransform));
        child.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }
}
