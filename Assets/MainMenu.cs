using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Eclipse ойынының басты мәзірі (Main Menu Controller).
/// Барлық батырмалар табиғи тас плиталардың үстіне тұнық әрі анық орналасқан.
/// Ешқандай артық қарайған қораптарсыз, таза және сочный визуал!
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Негізгі баптаулар")]
    public Canvas targetCanvas;
    public Image backgroundImage;

    [Header("Дыбыс көзі")]
    private AudioSource audioSource;
    private AudioClip sfxClick;
    private AudioClip sfxHover;
    private AudioClip sfxMagic;
    private AudioClip sfxStart;
    private AudioClip sfxClose;

    // Модальді терезелер (Popups)
    private GameObject popupOverlay;
    private GameObject settingsPanel;
    private GameObject loadProgressPanel;
    private GameObject quitConfirmPanel;
    private GameObject infoPanel;
    private TextMeshProUGUI infoTitleText;
    private TextMeshProUGUI infoBodyText;
    private TextMeshProUGUI infoStatsText;

    private int selectedSaveSlot = 1;
    private TMP_FontAsset defaultFont;

    private void Awake()
    {
        SetupAudio();

        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
        }

        TextMeshProUGUI existingTmp = FindAnyObjectByType<TextMeshProUGUI>();
        if (existingTmp != null)
        {
            defaultFont = existingTmp.font;
        }

        // Ескі ақ батырмаларды жасыру
        CleanLegacyButtons();

        // Барлық интерактивті элементтерді орнату
        CreateAllUIElements();

        // Модальді терезелерді дайындау
        CreatePopups();
    }

    private void Start()
    {
        PlaySound(sfxMagic, 0.45f);
        Debug.Log("Eclipse: Басты мәзір сәтті қосылды! Барлық батырмалар әзір.");
    }

    #region Негізгі Батырма Әрекеттері

    public void PlayGame()
    {
        PlaySound(sfxStart, 0.9f);
        Debug.Log("СИМУЛЯЦИЯ БАСТАЛДЫ! Негізгі ойынға көшу...");
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.4f);

        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void OpenLoadProgress()
    {
        PlaySound(sfxClick, 0.7f);
        CloseAllPopups();
        popupOverlay.SetActive(true);
        loadProgressPanel.SetActive(true);
        AnimatePopup(loadProgressPanel);
    }

    public void OpenSettings()
    {
        PlaySound(sfxClick, 0.7f);
        CloseAllPopups();
        popupOverlay.SetActive(true);
        settingsPanel.SetActive(true);
        AnimatePopup(settingsPanel);
    }

    public void QuitGame()
    {
        PlaySound(sfxClick, 0.7f);
        CloseAllPopups();
        popupOverlay.SetActive(true);
        quitConfirmPanel.SetActive(true);
        AnimatePopup(quitConfirmPanel);
    }

    public void ConfirmQuit()
    {
        PlaySound(sfxClick, 0.8f);
        Debug.Log("Ойын толық аяқталды.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CloseAllPopups()
    {
        PlaySound(sfxClose, 0.6f);
        if (popupOverlay != null) popupOverlay.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadProgressPanel != null) loadProgressPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    #endregion

    #region Интерактивті Аймақтар мен Руналар

    public void OnZoneClicked(string zoneTitle, string zoneDesc, string zoneStats)
    {
        PlaySound(sfxMagic, 0.75f);
        ShowInfoModal(zoneTitle, zoneDesc, zoneStats);
    }

    public void OnRuneClicked(string runeName, string combo, string stats)
    {
        PlaySound(sfxMagic, 0.85f);
        ShowInfoModal(runeName, "СИНТЕЗ ЖӘНЕ ҚАСИЕТІ:\n" + combo, stats);
    }

    public void OnSlotClicked(int slotIndex, string itemName, string itemDesc)
    {
        PlaySound(sfxClick, 0.7f);
        ShowInfoModal($"СЛОТ [{slotIndex}]: {itemName}", itemDesc, "Статус: Белсенді жауынгерлік қару");
    }

    private void ShowInfoModal(string title, string body, string stats)
    {
        CloseAllPopups();
        popupOverlay.SetActive(true);
        infoPanel.SetActive(true);

        if (infoTitleText != null) infoTitleText.text = title;
        if (infoBodyText != null) infoBodyText.text = body;
        if (infoStatsText != null) infoStatsText.text = stats;

        AnimatePopup(infoPanel);
    }

    #endregion

    #region UI Элементтерін Құру

    private void CleanLegacyButtons()
    {
        if (targetCanvas == null) return;

        // Ескі батырмаларды өшіру
        Transform playBtn = targetCanvas.transform.Find("PlayButton");
        if (playBtn != null) playBtn.gameObject.SetActive(false);

        Transform quitBtn = targetCanvas.transform.Find("QuitButton");
        if (quitBtn != null) quitBtn.gameObject.SetActive(false);
    }

    private void CreateAllUIElements()
    {
        if (targetCanvas == null) return;

        // 1. Сол жақтағы тас плиталарға тура үйлесетін батырмалар
        // Фон мөлдір - табиғи 3D тас суреті көрінеді, тек қаріп жарқырап тұрады!
        CreateCleanStoneButton("Btn_RunSim", "СИМУЛЯЦИЯНЫ БАСТАУ", new Vector2(-640, 210), new Vector2(380, 72), PlayGame);
        CreateCleanStoneButton("Btn_LoadProg", "ПРОГРЕСТІ ЖҮКТЕУ", new Vector2(-640, 65), new Vector2(380, 72), OpenLoadProgress);
        CreateCleanStoneButton("Btn_Settings", "БАПТАУЛАР", new Vector2(-640, -80), new Vector2(380, 72), OpenSettings);
        CreateCleanStoneButton("Btn_Quit", "ОЙЫННАН ШЫҒУ", new Vector2(-640, -225), new Vector2(380, 72), QuitGame);
        CreateMenuSubtitle();

        // 2. Төменгі сол жақ - Scope / Жол картасы (Табиғи тас панеліне орналасқан таза мәтін)
        CreateCleanScopeText(new Vector2(-640, -380), new Vector2(390, 125));

        // 3. Орталық аренаның интерактивті аймақтары (Тыныш күйде мөлдір, үстіне барғанда ғана жарқырайды)
        CreateCleanHotspot("Zone_Player", "ҚАРАҢҒЫ БАТЫР АРЕНАСЫ",
            "Ойыншының бастапқы алаңы. Қараңғы батыр зомбилер армиясына қарсы тұруға сақадай сай.",
            "Денсаулық (HP): 100/100 | Шабуыл: 25 | Жылдамдық: 5.0 | Қабілет: 'Көлеңкелі жарылыс'",
            new Vector2(0, -95), new Vector2(160, 160));

        CreateCleanHotspot("Zone_MainArea", "БАСТЫ ҰРЫС АЙМАҒЫ",
            "Аренаның негізгі тас шеңбері. Барлық жаулар осы аймаққа қарай ағылады.",
            "Қауіп деңгейі: Орташа | Сыйлықтар: XP Кристалдары және Ежелгі руналар",
            new Vector2(-280, -95), new Vector2(130, 90));

        CreateCleanHotspot("Zone_Melee", "ҚАНҚҰМАР ЗОМБИЛЕР АЙМАҒЫ",
            "Жақыннан шабуылдайтын зомбилер. Жылдам қимылдап, қоршауға алуға тырысады.",
            "HP: 40 | Зақым: 10 | Жылдамдық: 3.5 | Әлсіздік: Аймақтық от сиқыры",
            new Vector2(-225, 75), new Vector2(130, 100));

        CreateCleanHotspot("Zone_Ranged", "ЗОМБИ СПАВН АЙМАҒЫ",
            "Алыстан шабуылдайтын зомбилер батырды қашықтықтан көздейді.",
            "HP: 30 | Оқ зақымы: 15 | Қашықтық: 9 метр | Әлсіздік: Шапшаң соққылар",
            new Vector2(-70, 160), new Vector2(130, 100));

        CreateCleanHotspot("Zone_Boss", "ЗОМБИ-БОСС",
            "Алып зомби-босс жойқын соққыларымен тұтас аренаны сілкіндіреді!",
            "HP: 1,500 | Зақым: 50 | Фазалар: 3 түрлі кезең | Олжа: Аңызға айналған 'Күн Тұтылу' тәжі",
            new Vector2(360, 80), new Vector2(200, 180));

        CreateCleanHotspot("Zone_Elite", "ЭЛИТАЛЫҚ ЗОМБИ",
            "Сауыт киген, шабуылдары ауыр және соққыларға төзімді зомби.",
            "HP: 450 | Зақым: 28 | Қабілет: Ауыр найза екпіні | Олжа: Руналық сиқыр тасы",
            new Vector2(380, -180), new Vector2(140, 120));

        // 4. Оң жақтағы Руналар мен Синтездер (Суреттегі руна слоттарының үстінде мөлдір)
        CreateCleanRune("Rune_Fire", "ОТ РУНАСЫ (ҚЫЗЫЛ)",
            "Негізгі элемент: От", "Жалын зақымы: +15% | Алаңды өртеу әсері",
            new Vector2(430, -145), new Vector2(70, 70), new Color(1f, 0.2f, 0.2f, 0.35f));

        CreateCleanRune("Rune_Water", "СУ / МҰЗ РУНАСЫ (КӨК)",
            "Негізгі элемент: Мұз бен Су", "Баяулату: -25% жау жылдамдығы | Мұздату мүмкіндігі: 20%",
            new Vector2(530, -145), new Vector2(70, 70), new Color(0.2f, 0.7f, 1f, 0.35f));

        CreateCleanRune("Rune_Nature", "ТАБИҒАТ / ҚАЛҚАН РУНАСЫ (ЖАСЫЛ)",
            "Негізгі элемент: Жер және Табиғат", "Қорғаныс қалқаны: +30 | Әр секунд сайын 5 HP қалпына келтіру",
            new Vector2(630, -145), new Vector2(70, 70), new Color(0.2f, 0.9f, 0.3f, 0.35f));

        CreateCleanRune("Rune_VoidStrike", "БОС КЕҢІСТІК СОҚҚЫСЫ (VOID STRIKE)",
            "От + Мұз синтезі арқылы пайда болған күлгін кеңістік жыртылуы.",
            "Аймақтық зақым: 85 | Жауларды орталыққа тартып жұту әсері",
            new Vector2(430, -345), new Vector2(70, 70), new Color(0.75f, 0.2f, 1f, 0.4f));

        CreateCleanRune("Rune_BloodHarvest", "ҚАНДЫ ОРАҚ (BLOOD HARVEST)",
            "От + Табиғат синтезі. Жаулардың тіршілік күшін сорып алу сиқыры.",
            "Зақым: 60 | Өмір сүру (Lifesteal): 35% денсаулық қайтару",
            new Vector2(530, -345), new Vector2(70, 70), new Color(0.9f, 0.1f, 0.2f, 0.4f));

        CreateCleanRune("Rune_SoulSiphon", "ЖАН СІҢІРУ (SOUL SIPHON)",
            "Мұз + Табиғат синтезі. Рухани кристалдарды тартып, батырды күшейтеді.",
            "Қарсыласты қатыру: 2.5 сек | Кристалл табу: 2x есе артады",
            new Vector2(630, -345), new Vector2(70, 70), new Color(0.1f, 0.95f, 0.6f, 0.4f));

        // 5. Төменгі жедел слоттар [1] [2] [3] [4] (Сандар суретте бар, тек басу әрекеті)
        CreateCleanSlot("Slot_1", 1, "Қараңғы Қылыш", "Жылдам тілу соққысы. Зақым: 25", new Vector2(-170, -455));
        CreateCleanSlot("Slot_2", 2, "Отты Шар", "Жалынды атыс. Зақым: 45", new Vector2(-55, -455));
        CreateCleanSlot("Slot_3", 3, "Мұз Қалқаны", "Уақытша мызғымас қорған. +50 Қорғаныс", new Vector2(55, -455));
        CreateCleanSlot("Slot_4", 4, "Өлім Орағы", "Айналмалы соққы. Барлық жақын жауларға 70 зақым", new Vector2(170, -455));
    }

    private void CreateMenuSubtitle()
    {
        GameObject subtitleObject = new GameObject("MenuSubtitle",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        subtitleObject.transform.SetParent(targetCanvas.transform, false);

        RectTransform rect = subtitleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 450f);
        rect.sizeDelta = new Vector2(1300f, 72f);

        TextMeshProUGUI subtitle = subtitleObject.GetComponent<TextMeshProUGUI>();
        subtitle.font = defaultFont != null ? defaultFont : TMP_Settings.defaultFontAsset;
        subtitle.text = "ТІРІ ҚАЛУ — БАСТЫ МАҚСАТ";
        subtitle.fontSize = 36f;
        subtitle.fontStyle = FontStyles.Bold;
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.color = Color.white;
        subtitle.raycastTarget = false;
    }

    private void CreateCleanStoneButton(string name, string text, Vector2 anchoredPos, Vector2 size, Action onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        // Фон ТҰНЫҚ (Alpha = 0.001f) - суреттегі әдемі 3D тас плитасы бүлінбей көрініп тұрады!
        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.001f);

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0, 0, 0, 0.001f);
        cb.highlightedColor = new Color(0.35f, 0.65f, 1f, 0.18f); // Үстіне барғанда нәзік жарық
        cb.pressedColor = new Color(0.45f, 0.75f, 1f, 0.32f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        btn.onClick.AddListener(() => onClick?.Invoke());
        AddHoverAnimation(btnObj);

        // Тасқа қашалғандай анық, көлеңкелі қаріп
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = text;
        tmp.fontSize = 21;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.96f, 0.98f, 1f); // Жарқын ақ-күміс

        // Қаріпті анық ететін терең көлеңке
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(2f, -2f);
    }

    private void CreateCleanScopeText(Vector2 anchoredPos, Vector2 size)
    {
        GameObject box = new GameObject("ScopeBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        box.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = box.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        // Фон тұнық, тас суреті көрінеді
        Image img = box.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.001f);

        Button btn = box.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            ShowInfoModal("ЖОБА ЖОЛ КАРТАСЫ (ROADMAP)",
                "• 1-АПТА: Негізгі шайқас және зомбилер\n• 2-АПТА: Басқару және әрекеттер\n• 3-АПТА: Жаңа жаулар мен марапаттар\n• 4-АПТА: Босс шайқасы және прогресс",
                "Ағымдағы нұсқа: MVP Protocol 1-4 | Дайындық: 100%");
        });

        GameObject textObj = new GameObject("ScopeText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(box.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-16, -10);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = "<b>1-АПТА:</b> Негізгі ұрыс және зомбилер\n<b>2-АПТА:</b> Басқару және әрекеттер\n<b>3-АПТА:</b> Жаңа жаулар мен марапаттар\n<b>4-АПТА:</b> Босс шайқасы және прогресс";
        tmp.fontSize = 13.5f;
        tmp.lineSpacing = 10f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(0.92f, 0.95f, 1f);

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private void CreateCleanHotspot(string name, string title, string desc, string stats, Vector2 anchoredPos, Vector2 size)
    {
        GameObject hotspot = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hotspot.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = hotspot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = hotspot.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.001f);

        Button btn = hotspot.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0, 0, 0, 0.001f);
        cb.highlightedColor = new Color(0.4f, 0.7f, 1f, 0.25f);
        cb.pressedColor = new Color(0.9f, 0.3f, 0.3f, 0.4f);
        btn.colors = cb;

        btn.onClick.AddListener(() => OnZoneClicked(title, desc, stats));
        AddHoverAnimation(hotspot);
    }

    private void CreateCleanRune(string name, string runeName, string combo, string stats, Vector2 anchoredPos, Vector2 size, Color glowColor)
    {
        GameObject runeObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        runeObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = runeObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = runeObj.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.001f);

        Button btn = runeObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0, 0, 0, 0.001f);
        cb.highlightedColor = glowColor;
        cb.pressedColor = Color.white;
        btn.colors = cb;

        btn.onClick.AddListener(() => OnRuneClicked(runeName, combo, stats));
        AddHoverAnimation(runeObj);
    }

    private void CreateCleanSlot(string name, int slotIndex, string itemName, string itemDesc, Vector2 anchoredPos)
    {
        GameObject slotObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        slotObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = slotObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(70, 60);

        Image img = slotObj.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.001f);

        Button btn = slotObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0, 0, 0, 0.001f);
        cb.highlightedColor = new Color(0.4f, 0.6f, 0.9f, 0.3f);
        cb.pressedColor = new Color(0.6f, 0.8f, 1f, 0.6f);
        btn.colors = cb;

        btn.onClick.AddListener(() => OnSlotClicked(slotIndex, itemName, itemDesc));
        AddHoverAnimation(slotObj);
    }

    #endregion

    #region Модальді Терезелер (Popups)

    private void CreatePopups()
    {
        if (targetCanvas == null) return;

        popupOverlay = new GameObject("PopupOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        popupOverlay.transform.SetParent(targetCanvas.transform, false);

        RectTransform overlayRt = popupOverlay.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;

        Image overlayImg = popupOverlay.GetComponent<Image>();
        overlayImg.color = new Color(0.03f, 0.04f, 0.07f, 0.85f);

        Button overlayBtn = popupOverlay.GetComponent<Button>();
        overlayBtn.onClick.AddListener(CloseAllPopups);

        // 1. БАПТАУЛАР
        settingsPanel = CreateModalFrame("SettingsPanel", "БАПТАУЛАР", new Vector2(580, 520));
        CreateSettingsContent(settingsPanel);

        // 2. ПРОГРЕСТІ ЖҮКТЕУ
        loadProgressPanel = CreateModalFrame("LoadProgressPanel", "ПРОГРЕСТІ ЖҮКТЕУ", new Vector2(620, 520));
        CreateLoadProgressContent(loadProgressPanel);

        // 3. ОЙЫННАН ШЫҒУ
        quitConfirmPanel = CreateModalFrame("QuitConfirmPanel", "ОЙЫННАН ШЫҒУ", new Vector2(480, 280));
        CreateQuitConfirmContent(quitConfirmPanel);

        // 4. СИПАТТАМА / АҚПАРАТ
        infoPanel = CreateModalFrame("InfoPanel", "АҚПАРАТ", new Vector2(560, 420));
        CreateInfoContent(infoPanel);

        popupOverlay.SetActive(false);
        settingsPanel.SetActive(false);
        loadProgressPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    private GameObject CreateModalFrame(string name, string title, Vector2 size)
    {
        GameObject modal = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        modal.transform.SetParent(popupOverlay.transform, false);

        RectTransform rt = modal.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = modal.GetComponent<Image>();
        img.color = new Color(0.11f, 0.14f, 0.20f, 0.98f);

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(modal.transform, false);

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -20);
        titleRt.sizeDelta = new Vector2(-40, 50);

        TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) titleTmp.font = defaultFont;
        titleTmp.text = title;
        titleTmp.fontSize = 26;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(0.95f, 0.85f, 0.45f);

        CreateCloseButton(modal, new Vector2((size.x / 2) - 30, (size.y / 2) - 30));

        return modal;
    }

    private void CreateCloseButton(GameObject parent, Vector2 pos)
    {
        GameObject closeBtn = new GameObject("CloseBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtn.transform.SetParent(parent.transform, false);

        RectTransform rt = closeBtn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(36, 36);

        Image img = closeBtn.GetComponent<Image>();
        img.color = new Color(0.35f, 0.15f, 0.15f, 0.85f);

        Button btn = closeBtn.GetComponent<Button>();
        btn.onClick.AddListener(CloseAllPopups);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(closeBtn.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = "✕";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    private void CreateSettingsContent(GameObject panel)
    {
        CreateSettingSlider(panel, "Жалпы дыбыс (Master Volume):", 80, 0.8f, val =>
        {
            AudioListener.volume = val;
        });

        CreateSettingSlider(panel, "Эффектілер дыбысы (SFX):", 0, 0.9f, val =>
        {
            if (audioSource != null) audioSource.volume = val;
        });

        CreateSettingToggle(panel, "Толық экран режимі (Fullscreen):", -80, Screen.fullScreen, isFull =>
        {
            Screen.fullScreen = isFull;
            PlaySound(sfxClick, 0.6f);
        });

        CreateSettingLabel(panel, "Тіл (Language): Қазақ тілі (Таңдалған)", -140);

        CreateGenericButton(panel, "САҚТАУ ЖӘНЕ ЖАБУ", new Vector2(0, -210), new Vector2(240, 50), CloseAllPopups, true);
    }

    private void CreateSettingSlider(GameObject parent, string label, float posY, float defaultVal, Action<float> onValChange)
    {
        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = new Vector2(0, posY + 22);
        textRt.sizeDelta = new Vector2(460, 30);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = label;
        tmp.fontSize = 17;
        tmp.color = new Color(0.9f, 0.93f, 0.98f);

        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(parent.transform, false);

        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRt.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRt.anchoredPosition = new Vector2(0, posY - 8);
        sliderRt.sizeDelta = new Vector2(460, 20);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultVal;
        slider.onValueChanged.AddListener(v => onValChange?.Invoke(v));
    }

    private void CreateSettingToggle(GameObject parent, string label, float posY, bool defaultVal, Action<bool> onToggle)
    {
        GameObject btnObj = new GameObject("ToggleBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(460, 42);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.18f, 0.22f, 0.30f, 0.7f);

        Button btn = btnObj.GetComponent<Button>();

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-20, 0);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;

        bool state = defaultVal;
        tmp.text = $"{label} <b>{(state ? "[ҚОСУЛЫ]" : "[ӨШІРУЛІ]")}</b>";
        tmp.fontSize = 17;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        btn.onClick.AddListener(() =>
        {
            state = !state;
            tmp.text = $"{label} <b>{(state ? "[ҚОСУЛЫ]" : "[ӨШІРУЛІ]")}</b>";
            onToggle?.Invoke(state);
        });
    }

    private void CreateSettingLabel(GameObject parent, string text, float posY)
    {
        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = new Vector2(0, posY);
        textRt.sizeDelta = new Vector2(460, 32);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = text;
        tmp.fontSize = 17;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = new Color(0.75f, 0.85f, 0.95f);
    }

    private void CreateLoadProgressContent(GameObject panel)
    {
        CreateSaveSlotRow(panel, 1, "СЛОТ 1: АРХИВ БАТЫРЫ", "5-Деңгей | Тірі қалған уақыт: 14:20 | 2,450 Алтын", 70);
        CreateSaveSlotRow(panel, 2, "СЛОТ 2: ҚАНДЫ ТАҢДАУ", "2-Деңгей | Тірі қалған уақыт: 05:12 | 680 Алтын", -25);
        CreateSaveSlotRow(panel, 3, "СЛОТ 3: БОС САҚТАУ", "[Жаңа ойынға арналған бос орын]", -120);

        CreateGenericButton(panel, "ТАҢДАЛҒАНДЫ ЖҮКТЕУ ЖӘНЕ ОЙНАУ", new Vector2(0, -210), new Vector2(380, 50), () =>
        {
            PlayGame();
        }, true);
    }

    private void CreateSaveSlotRow(GameObject parent, int slotNum, string title, string sub, float posY)
    {
        GameObject row = new GameObject($"SlotRow_{slotNum}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        row.transform.SetParent(parent.transform, false);

        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(520, 75);

        Image img = row.GetComponent<Image>();
        img.color = (selectedSaveSlot == slotNum) ? new Color(0.2f, 0.35f, 0.55f, 0.9f) : new Color(0.14f, 0.17f, 0.23f, 0.75f);

        Button btn = row.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            selectedSaveSlot = slotNum;
            PlaySound(sfxClick, 0.7f);
        });

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(row.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-30, 0);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = $"<b>{title}</b>\n<size=14><color=#90a5c0>{sub}</color></size>";
        tmp.fontSize = 17;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
    }

    private void CreateQuitConfirmContent(GameObject panel)
    {
        GameObject textObj = new GameObject("ConfirmText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(panel.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = new Vector2(0, 30);
        textRt.sizeDelta = new Vector2(400, 70);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = "Ойыннан шығуға сенімдісіз бе?\nСақталмаған деректер жоғалуы мүмкін.";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.9f, 0.92f, 0.96f);

        CreateGenericButton(panel, "ИӘ, ШЫҒУ", new Vector2(-110, -65), new Vector2(170, 48), ConfirmQuit, false);
        CreateGenericButton(panel, "ЖОҚ, ОРАЛУ", new Vector2(110, -65), new Vector2(170, 48), CloseAllPopups, true);
    }

    private void CreateInfoContent(GameObject panel)
    {
        GameObject titleObj = new GameObject("InfoTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(panel.transform, false);

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0, 95);
        titleRt.sizeDelta = new Vector2(480, 50);

        infoTitleText = titleObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) infoTitleText.font = defaultFont;
        infoTitleText.text = "СИПАТТАМА";
        infoTitleText.fontSize = 22;
        infoTitleText.fontStyle = FontStyles.Bold;
        infoTitleText.alignment = TextAlignmentOptions.Center;
        infoTitleText.color = new Color(0.95f, 0.82f, 0.4f);

        GameObject bodyObj = new GameObject("InfoBody", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        bodyObj.transform.SetParent(panel.transform, false);

        RectTransform bodyRt = bodyObj.GetComponent<RectTransform>();
        bodyRt.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRt.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRt.anchoredPosition = new Vector2(0, 15);
        bodyRt.sizeDelta = new Vector2(480, 95);

        infoBodyText = bodyObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) infoBodyText.font = defaultFont;
        infoBodyText.text = "Толық мәлімет.";
        infoBodyText.fontSize = 16.5f;
        infoBodyText.lineSpacing = 8f;
        infoBodyText.alignment = TextAlignmentOptions.TopLeft;
        infoBodyText.color = new Color(0.88f, 0.92f, 0.98f);

        GameObject statsObj = new GameObject("InfoStats", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(panel.transform, false);

        RectTransform statsRt = statsObj.GetComponent<RectTransform>();
        statsRt.anchorMin = new Vector2(0.5f, 0.5f);
        statsRt.anchorMax = new Vector2(0.5f, 0.5f);
        statsRt.anchoredPosition = new Vector2(0, -65);
        statsRt.sizeDelta = new Vector2(480, 55);

        infoStatsText = statsObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) infoStatsText.font = defaultFont;
        infoStatsText.text = "Статус: Қалыпты";
        infoStatsText.fontSize = 15f;
        infoStatsText.fontStyle = FontStyles.Italic;
        infoStatsText.alignment = TextAlignmentOptions.TopLeft;
        infoStatsText.color = new Color(0.4f, 0.85f, 0.65f);

        CreateGenericButton(panel, "ТҮСІНІКТІ", new Vector2(0, -150), new Vector2(180, 45), CloseAllPopups, true);
    }

    private void CreateGenericButton(GameObject parent, string text, Vector2 anchoredPos, Vector2 size, Action onClick, bool isPrimary)
    {
        GameObject btnObj = new GameObject("Btn_" + text.Replace(" ", ""), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = btnObj.GetComponent<Image>();
        img.color = isPrimary ? new Color(0.2f, 0.45f, 0.75f, 0.95f) : new Color(0.25f, 0.28f, 0.35f, 0.9f);

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());
        AddHoverAnimation(btnObj);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = text;
        tmp.fontSize = 17;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    #endregion

    #region Анимация және Дыбыс Синтезі

    private void AddHoverAnimation(GameObject target)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) =>
        {
            PlaySound(sfxHover, 0.55f);
            StopCoroutine("ScaleRoutine");
            StartCoroutine(ScaleRoutine(target.transform, 1.05f, 0.12f));
        });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) =>
        {
            StopCoroutine("ScaleRoutine");
            StartCoroutine(ScaleRoutine(target.transform, 1.0f, 0.12f));
        });
        trigger.triggers.Add(entryExit);
    }

    private IEnumerator ScaleRoutine(Transform t, float targetScale, float duration)
    {
        if (t == null) yield break;
        Vector3 start = t.localScale;
        Vector3 end = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        if (t != null) t.localScale = end;
    }

    private void AnimatePopup(GameObject popup)
    {
        if (popup == null) return;
        StopAllCoroutines();
        StartCoroutine(PopupBounceRoutine(popup.transform));
    }

    private IEnumerator PopupBounceRoutine(Transform t)
    {
        t.localScale = new Vector3(0.88f, 0.88f, 1f);
        float elapsed = 0f;
        float dur = 0.2f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / dur;
            float scale = Mathf.Lerp(0.88f, 1.0f, progress) + Mathf.Sin(progress * Mathf.PI) * 0.04f;
            t.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        sfxClick = CreateProceduralAudio(0.08f, 420f, 140f, 0.8f, WaveType.Square);
        sfxHover = CreateProceduralAudio(0.04f, 650f, 850f, 0.25f, WaveType.Sine);
        sfxMagic = CreateProceduralAudio(0.35f, 320f, 640f, 0.6f, WaveType.SineChord);
        sfxStart = CreateProceduralAudio(0.70f, 180f, 520f, 0.9f, WaveType.DeepGong);
        sfxClose = CreateProceduralAudio(0.09f, 500f, 220f, 0.5f, WaveType.Sine);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private enum WaveType { Sine, Square, SineChord, DeepGong }

    private AudioClip CreateProceduralAudio(float duration, float startFreq, float endFreq, float maxVolume, WaveType type)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;
            float currentFreq = Mathf.Lerp(startFreq, endFreq, progress);

            float envelope = 1f - progress;
            if (progress < 0.1f) envelope = progress / 0.1f;

            float sampleVal = 0f;
            switch (type)
            {
                case WaveType.Sine:
                    sampleVal = Mathf.Sin(2 * Mathf.PI * currentFreq * t);
                    break;
                case WaveType.Square:
                    sampleVal = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * currentFreq * t)) * 0.4f;
                    break;
                case WaveType.SineChord:
                    sampleVal = (Mathf.Sin(2 * Mathf.PI * currentFreq * t) +
                                 0.5f * Mathf.Sin(2 * Mathf.PI * (currentFreq * 1.5f) * t) +
                                 0.3f * Mathf.Sin(2 * Mathf.PI * (currentFreq * 2.0f) * t)) / 1.8f;
                    break;
                case WaveType.DeepGong:
                    sampleVal = (Mathf.Sin(2 * Mathf.PI * currentFreq * t) +
                                 0.7f * Mathf.Sin(2 * Mathf.PI * (currentFreq * 0.5f) * t) +
                                 0.4f * Mathf.Sin(2 * Mathf.PI * (currentFreq * 3.0f) * t)) / 2.1f;
                    break;
            }

            samples[i] = sampleVal * envelope * maxVolume;
        }

        AudioClip clip = AudioClip.Create("SFX_" + type, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    #endregion
}