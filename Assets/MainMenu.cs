using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const string IntroSceneName = "Intro";

    private Canvas menuCanvas;
    private GameObject settingsPanel;

    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        menuCanvas = FindAnyObjectByType<Canvas>();
        if (menuCanvas == null)
        {
            Debug.LogError("MainMenu: Canvas табылмады.");
            return;
        }

        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.overrideSorting = true;
        menuCanvas.sortingOrder = 100;
        menuCanvas.pixelPerfect = true;
        CanvasScaler scaler = menuCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform navigationPanel = CreatePanel(menuCanvas.transform, "MainNavigationPanel",
            new Vector2(-650, 0), new Vector2(500, 720), new Color(0.025f, 0.035f, 0.065f, 0.88f), 1);
        CreateText(navigationPanel, "MenuTitle", "",
            new Vector2(0, 245), new Vector2(440, 130), 48, FontStyles.Bold);
        CreateText(navigationPanel, "MenuSubtitle", "ТІРІ ҚАЛУ — БАСТЫ МАҚСАТ",
            new Vector2(0, 160), new Vector2(440, 56), 26, FontStyles.Bold);

        ConfigureExistingButton("PlayButton", navigationPanel, "ОЙЫНДЫ БАСТАУ",
            new Vector2(0, 55));
        CreateButton(navigationPanel, "SettingsButton", "БАПТАУЛАР",
            new Vector2(0, -70), new Vector2(420, 82), ToggleSettings);
        ConfigureExistingButton("QuitButton", navigationPanel, "ШЫҒУ",
            new Vector2(0, -195));

        CreateInfoPanel(menuCanvas.transform);
        CreateSettingsPanel();
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(IntroSceneName))
        {
            Debug.LogError($"MainMenu: '{IntroSceneName}' сахнасы Build Settings тізімінде жоқ.");
            return;
        }

        SceneManager.LoadScene(IntroSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ConfigureExistingButton(string buttonName, Transform parent, string label,
        Vector2 position)
    {
        Transform button = menuCanvas.transform.Find(buttonName);
        if (button == null)
        {
            Debug.LogError($"MainMenu: {buttonName} батырмасы табылмады.");
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(420, 82);

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.12f, 0.18f, 0.29f, 1f);
        }

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            StyleMenuText(text, label, 27, FontStyles.Bold);
        }
    }

    private void CreateInfoPanel(Transform parent)
    {
        RectTransform panel = CreatePanel(parent, "SurvivalInfoPanel",
            new Vector2(650, 0), new Vector2(500, 620), new Color(0.025f, 0.035f, 0.065f, 0.88f), 2);

        CreateText(panel, "InfoTitle", "ТІРІ ҚАЛУ ЕРЕЖЕСІ",
            new Vector2(0, 245), new Vector2(440, 60), 30, FontStyles.Bold);
        CreateText(panel, "InfoDescription", "5 ТОЛҚЫННАН ӨТ.\nЗОМБИЛЕРДІ ЖЕҢІП, ДАҒДЫҢДЫ ЖЕТІЛДІР.",
            new Vector2(0, 150), new Vector2(430, 110), 21, FontStyles.Bold);

        CreateDivider(panel, "InfoDivider", 68);
        CreateText(panel, "MoveHint", "W A S D   —   ҚОЗҒАЛУ",
            new Vector2(0, 18), new Vector2(430, 46), 20, FontStyles.Bold);
        CreateText(panel, "AttackHint", "ТІНТУІР   —   ШАБУЫЛ",
            new Vector2(0, -48), new Vector2(430, 46), 20, FontStyles.Bold);
        CreateText(panel, "GoalHint", "Q — СУПЕР ШАБУЫЛ\n10 СЕКУНД САЛҚЫНДАУ",
            new Vector2(0, -138), new Vector2(430, 72), 20, FontStyles.Bold);
        CreateText(panel, "RecordsText",
            $"РЕКОРД: {PlayerStats.BestZombieKills} ЗОМБИ  |  ДЕҢГЕЙ {PlayerStats.BestPlayerLevel}",
            new Vector2(0, -195), new Vector2(440, 42), 18, FontStyles.Bold);
        CreateButton(panel, "StartFromInfoButton", "ҰРЫСТЫ БАСТАУ",
            new Vector2(0, -255), new Vector2(390, 64), PlayGame);
    }

    private void CreateSettingsPanel()
    {
        RectTransform panel = CreatePanel(menuCanvas.transform, "SettingsPanel",
            Vector2.zero, new Vector2(620, 420), new Color(0.018f, 0.028f, 0.052f, 0.98f), -1);
        settingsPanel = panel.gameObject;
        CreateText(panel, "SettingsTitle", "БАПТАУЛАР", new Vector2(0, 145),
            new Vector2(500, 60), 34, FontStyles.Bold);
        CreateText(panel, "VolumeLabel", "ДЫБЫС ДЕҢГЕЙІ", new Vector2(0, 80),
            new Vector2(480, 40), 22, FontStyles.Bold);

        Slider volumeSlider = CreateSlider(panel, "VolumeSlider", new Vector2(0, 35));
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = volumeSlider.value;
        volumeSlider.onValueChanged.AddListener(value =>
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
        });

        CreateButton(panel, "FullscreenButton", GetFullscreenLabel(),
            new Vector2(0, -35), new Vector2(460, 64), ToggleFullscreen);
        CreateButton(panel, "CloseSettingsButton", "ҚАЙТУ",
            new Vector2(0, -120), new Vector2(300, 58), ToggleSettings);
        settingsPanel.SetActive(false);
    }

    private void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
        Transform label = settingsPanel.transform.Find("FullscreenButton/FullscreenButtonText");
        if (label != null)
        {
            label.GetComponent<TextMeshProUGUI>().text = GetFullscreenLabel();
        }
    }

    private string GetFullscreenLabel()
    {
        return $"ТОЛЫҚ ЭКРАН: {(Screen.fullScreen ? "ҚОСУЛЫ" : "ӨШІРУЛІ")}";
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 position)
    {
        RectTransform root = CreateRect(parent, name, position, new Vector2(340, 22));
        Image background = root.gameObject.AddComponent<Image>();
        background.color = new Color(0.16f, 0.19f, 0.26f, 1f);
        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        RectTransform fill = CreateRect(root, "Fill", Vector2.zero, Vector2.zero);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = new Vector2(3, 3);
        fill.offsetMax = new Vector2(-3, -3);
        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color(0.25f, 0.62f, 0.92f, 1f);
        slider.fillRect = fill;
        slider.targetGraphic = background;
        slider.interactable = true;
        return slider;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rect = CreateRect(parent, name, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.34f, 1f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        CreateText(rect, $"{name}Text", label, Vector2.zero, size, 25, FontStyles.Bold);
        return button;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string value,
        Vector2 position, Vector2 size, int fontSize, FontStyles style)
    {
        RectTransform rect = CreateRect(parent, name, position, size);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (text.font == null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        StyleMenuText(text, value, fontSize, style);
        return text;
    }

    private static void StyleMenuText(TextMeshProUGUI text, string value, int fontSize, FontStyles style)
    {
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.96f, 0.97f, 1f, 1f);
        text.outlineColor = new Color(0.015f, 0.025f, 0.045f, 1f);
        text.outlineWidth = 0.2f;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.extraPadding = true;
    }

    private static RectTransform CreatePanel(Transform parent, string name, Vector2 position,
        Vector2 size, Color color, int siblingIndex)
    {
        RectTransform panel = CreateRect(parent, name, position, size);
        panel.gameObject.AddComponent<Image>().color = color;
        int targetIndex = siblingIndex < 0
            ? parent.childCount - 1
            : Mathf.Clamp(siblingIndex, 0, parent.childCount - 1);
        panel.SetSiblingIndex(targetIndex);
        return panel;
    }

    private static void CreateDivider(Transform parent, string name, float yPosition)
    {
        RectTransform divider = CreateRect(parent, name, new Vector2(0, yPosition), new Vector2(380, 3));
        divider.gameObject.AddComponent<Image>().color = new Color(0.55f, 0.68f, 0.86f, 0.65f);
    }

    private static RectTransform CreateRect(Transform parent, string name,
        Vector2 position, Vector2 size)
    {
        GameObject child = new(name, typeof(RectTransform));
        child.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }
}