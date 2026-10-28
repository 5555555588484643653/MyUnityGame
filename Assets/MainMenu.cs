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
/// Барлық батырмалар, интерактивті аймақтар, баптаулар және руналар жүйесі осы жерде жұмыс істейді.
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

    // Слоттар мен деректер
    private int selectedSaveSlot = 1;
    private TMP_FontAsset defaultFont;

    private void Awake()
    {
        // Дыбыс жүйесін инициализациялау (Процедуралық дыбыстар)
        SetupAudio();

        // Canvas іздеу
        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
        }

        // Қолданыстағы қаріпті табу
        TextMeshProUGUI existingTmp = FindAnyObjectByType<TextMeshProUGUI>();
        if (existingTmp != null)
        {
            defaultFont = existingTmp.font;
        }

        // Ескі батырмаларды тауып, жаңа стильге көшіру немесе жасыру
        CleanLegacyButtons();

        // Барлық интерактивті элементтерді жасау
        CreateAllUIElements();

        // Модальді терезелерді дайындау
        CreatePopups();
    }

    private void Start()
    {
        // Басты экран дыбысы
        PlaySound(sfxMagic, 0.5f);
        Debug.Log("Eclipse: Басты мәзір сәтті қосылды! Барлық батырмалар әзір.");
    }

    #region Негізгі Батырма Әрекеттері (Public Callbacks)

    /// <summary>
    /// ОЙЫНДЫ / СИМУЛЯЦИЯНЫ БАСТАУ
    /// </summary>
    public void PlayGame()
    {
        PlaySound(sfxStart, 0.9f);
        Debug.Log("СИМУЛЯЦИЯ БАСТАЛДЫ! Негізгі аренаға көшу...");

        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.45f);

        // Келесі сахнаны ашу (Егер бар болса, әйтпесе index 1 немесе нұсқаны ашады)
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            // Егер тек 1 сахна болса немесе атау бойынша:
            SceneManager.LoadScene(1);
        }
    }

    /// <summary>
    /// ПРОГРЕСТІ ЖҮКТЕУ ТЕРЕЗЕСІН АШУ
    /// </summary>
    public void OpenLoadProgress()
    {
        PlaySound(sfxClick, 0.7f);
        CloseAllPopups();
        popupOverlay.SetActive(true);
        loadProgressPanel.SetActive(true);
        AnimatePopup(loadProgressPanel);
    }

    /// <summary>
    /// БАПТАУЛАР ТЕРЕЗЕСІН АШУ
    /// </summary>
    public void OpenSettings()
    {
        PlaySound(sfxClick, 0.7f);
        CloseAllPopups();
        popupOverlay.SetActive(true);
        settingsPanel.SetActive(true);
        AnimatePopup(settingsPanel);
    }

    /// <summary>
    /// ОЙЫННАН ШЫҒУ ТЕРЕЗЕСІ
    /// </summary>
    public void QuitGame()
    {
        PlaySound(sfxClick, 0.7f);
        CloseAllPopups();
        popupOverlay.SetActive(true);
        quitConfirmPanel.SetActive(true);
        AnimatePopup(quitConfirmPanel);
    }

    /// <summary>
    /// ОЙЫНДЫ ТОЛЫҚ ЖАБУ (Расталған кезде)
    /// </summary>
    public void ConfirmQuit()
    {
        PlaySound(sfxClick, 0.8f);
        Debug.Log("Ойын толық аяқталды және жабылды.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// БАРЛЫҚ МОДАЛЬДІ ТЕРЕЗЕЛЕРДІ ЖАБУ
    /// </summary>
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

    #region Интерактивті Аймақтар мен Руналар (Interactivity)

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

    #region UI Элементтерін Құру (UI Builder)

    private void CleanLegacyButtons()
    {
        if (targetCanvas == null) return;

        // Ескі ақ PlayButton және QuitButton нысандарын жасыру, олар суретті жауып қалмасын
        Transform playBtn = targetCanvas.transform.Find("PlayButton");
        if (playBtn != null) playBtn.gameObject.SetActive(false);

        Transform quitBtn = targetCanvas.transform.Find("QuitButton");
        if (quitBtn != null) quitBtn.gameObject.SetActive(false);
    }

    private void CreateAllUIElements()
    {
        if (targetCanvas == null) return;

        // 1. Сол жақтағы басты мәзір батырмалары
        // Координаталар 1920x1080 салыстырмалы өлшемінде
        CreateMenuButton("Btn_RunSim", "СИМУЛЯЦИЯНЫ БАСТАУ", new Vector2(-690, 235), new Vector2(340, 75), PlayGame, true);
        CreateMenuButton("Btn_LoadProg", "ПРОГРЕСТІ ЖҮКТЕУ", new Vector2(-690, 85), new Vector2(340, 75), OpenLoadProgress, false);
        CreateMenuButton("Btn_Settings", "БАПТАУЛАР", new Vector2(-690, -65), new Vector2(340, 75), OpenSettings, false);
        CreateMenuButton("Btn_Quit", "ОЙЫННАН ШЫҒУ", new Vector2(-690, -215), new Vector2(340, 75), QuitGame, false);

        // 2. Төменгі сол жақ - Scope / Жол картасы панелі
        CreateScopeBox(new Vector2(-690, -400), new Vector2(370, 150));

        // 3. Орталық аренаның интерактивті аймақтары (Clickable Zones)
        // Ойыншының орталық аренасы
        CreateHotspot("Zone_Player", "ҚАРАҢҒЫ БАТЫР АРЕНАСЫ",
            "Ойыншының бастапқы қасиетті алаңы. Қараңғы батыр қанды орағымен гоблиндер армиясына қарсы тұруға сақадай сай.",
            "Денсаулық (HP): 100/100 | Шабуыл күші: 25 | Жылдамдық: 5.0 | Қабілет: 'Көлеңкелі жарылыс'",
            new Vector2(0, -90), new Vector2(160, 160));

        // Басты Аймақ (Main Area)
        CreateHotspot("Zone_MainArea", "БАСТЫ ҰРЫС АЙМАҒЫ",
            "Аренаның негізгі тас шеңбері. Барлық жаулар мен құбыжықтар осы аймаққа қарай ағылады.",
            "Қауіп деңгейі: Орташа | Сыйлықтар: XP Кристалдары және Ежелгі руналар",
            new Vector2(-280, -95), new Vector2(130, 90));

        // Жақыннан шабуылдайтын гоблиндер (Melee Goblin)
        CreateHotspot("Zone_Melee", "ҚАНҚҰМАР ГОБЛИНДЕР АЙМАҒЫ",
            "Жақыннан топтасып шабуылдайтын кіші гоблиндер. Жылдам қимылдап, қоршауға алуға тырысады.",
            "HP: 40 | Зақым: 10 | Жылдамдық: 3.5 | Әлсіздік: Аймақтық от сиқыры",
            new Vector2(-225, 75), new Vector2(140, 120));

        // Алыстан шабуылдайтын гоблиндер (Ranged Goblin)
        CreateHotspot("Zone_Ranged", "МЕРГЕН ГОБЛИНДЕР СПАВН АЙМАҒЫ",
            "Алыстан улы жебелер ататын гоблин мергендері. Батырды қашықтықтан атқылайды.",
            "HP: 30 | Оқ зақымы: 15 | Қашықтық: 9 метр | Әлсіздік: Шапшаң соққылар",
            new Vector2(-70, 160), new Vector2(150, 120));

        // Гоблиндер Патшасы (Goblins Patshasy - Boss)
        CreateHotspot("Zone_Boss", "ГОБЛИНДЕР ПАТШАСЫ (БАС БОСС)",
            "Тақ үстінде масаттанған алып Гоблиндер Патшасы. Жойқын тас балғасымен тұтас аренаны сілкіндіреді!",
            "HP: 1,500 | Зақым: 50 | Фазалар: 3 түрлі кезең | Олжа: Аңызға айналған 'Күн Тұтылу' тәжі",
            new Vector2(360, 80), new Vector2(220, 200));

        // Элиталық Гоблин (Elite Goblin)
        CreateHotspot("Zone_Elite", "ЭЛИТАЛЫҚ ГОБЛИН (ЧЕМПИОН)",
            "Патшаның бас оққағары. Сауыт киген, шабуылдары ауыр және батырдың соққыларына төзімді.",
            "HP: 450 | Зақым: 28 | Қабілет: Ауыр найза екпіні | Олжа: Руналық сиқыр тасы",
            new Vector2(380, -180), new Vector2(160, 140));

        // 4. Оң жақтағы Ежелгі Тас Плита - Руналық Алхимия Матрицасы
        CreateRuneElement("Rune_Fire", "ОТ РУНАСЫ (ҚЫЗЫЛ)",
            "Негізгі элемент: От", "Жалын зақымы: +15% | Алаңды өртеу әсері",
            new Vector2(560, -135), new Vector2(65, 65), new Color(0.9f, 0.2f, 0.2f, 0.3f));

        CreateRuneElement("Rune_Water", "СУ / МҰЗ РУНАСЫ (КӨК)",
            "Негізгі элемент: Мұз бен Су", "Баяулату: -25% жау жылдамдығы | Мұздату мүмкіндігі: 20%",
            new Vector2(685, -135), new Vector2(65, 65), new Color(0.1f, 0.6f, 0.95f, 0.3f));

        CreateRuneElement("Rune_Nature", "ТАБИҒАТ / ҚАЛҚАН РУНАСЫ (ЖАСЫЛ)",
            "Негізгі элемент: Жер және Табиғат", "Қорғаныс қалқаны: +30 | Әр секунд сайын 5 HP қалпына келтіру",
            new Vector2(810, -135), new Vector2(65, 65), new Color(0.2f, 0.85f, 0.3f, 0.3f));

        // Біріккен Руналар (Fusions)
        CreateRuneElement("Rune_VoidStrike", "БОС КЕҢІСТІК СОҚҚЫСЫ (VOID STRIKE)",
            "От + Мұз синтезі арқылы пайда болған күлгін кеңістік жыртылуы.",
            "Аймақтық зақым: 85 | Жауларды орталыққа тартып жұту әсері",
            new Vector2(560, -325), new Vector2(65, 65), new Color(0.7f, 0.15f, 0.9f, 0.35f));

        CreateRuneElement("Rune_BloodHarvest", "ҚАНДЫ ОРАҚ (BLOOD HARVEST)",
            "От + Табиғат синтезі. Жаулардың тіршілік күшін сорып алу сиқыры.",
            "Зақым: 60 | Өмір сүру (Lifesteal): Келтірілген зақымның 35%-ын батырға қайтару",
            new Vector2(685, -325), new Vector2(65, 65), new Color(0.85f, 0.1f, 0.2f, 0.35f));

        CreateRuneElement("Rune_SoulSiphon", "ЖАН СІҢІРУ (SOUL SIPHON)",
            "Мұз + Табиғат синтезі. Рухани кристалдарды тартып, батырды күшейтеді.",
            "Қарсыласты қатыру: 2.5 сек | Кристалл табу мүмкіндігі: 2x есе артады",
            new Vector2(810, -325), new Vector2(65, 65), new Color(0.1f, 0.9f, 0.6f, 0.35f));

        // 5. Төменгі орталық слоттар [1] [2] [3] [4]
        CreateSlotButton("Slot_1", 1, "Қараңғы Қылыш", "Жылдам тілу соққысы. Зақым: 25", new Vector2(-195, -450));
        CreateSlotButton("Slot_2", 2, "Отты Шар", "Жалынды атыс. Зақым: 45", new Vector2(-65, -450));
        CreateSlotButton("Slot_3", 3, "Мұз Қалқаны", "Уақытша мызғымас қорған. +50 Қорғаныс", new Vector2(65, -450));
        CreateSlotButton("Slot_4", 4, "Өлім Орағы", "Айналмалы соққы. Барлық жақын жауларға 70 зақым", new Vector2(195, -450));
    }

    private void CreateMenuButton(string name, string text, Vector2 anchoredPos, Vector2 size, Action onClick, bool isPrimary)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = btnObj.GetComponent<Image>();
        // Жартылай мөлдір тас түстес стиль (суреттің үстіне үйлесімді)
        img.color = isPrimary ? new Color(0.18f, 0.22f, 0.30f, 0.85f) : new Color(0.12f, 0.15f, 0.20f, 0.75f);

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = img.color;
        cb.highlightedColor = isPrimary ? new Color(0.28f, 0.40f, 0.65f, 0.95f) : new Color(0.25f, 0.30f, 0.42f, 0.9f);
        cb.pressedColor = new Color(0.40f, 0.55f, 0.85f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        btn.onClick.AddListener(() => onClick?.Invoke());

        // Анимациялық тыңдаушы (Hover scale effect)
        AddHoverAnimation(btnObj);

        // Мәтін қосу
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = text;
        tmp.fontSize = isPrimary ? 20 : 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = isPrimary ? new Color(0.95f, 0.98f, 1f) : new Color(0.85f, 0.88f, 0.92f);
    }

    private void CreateScopeBox(Vector2 anchoredPos, Vector2 size)
    {
        GameObject box = new GameObject("ScopeBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        box.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = box.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = box.GetComponent<Image>();
        img.color = new Color(0.08f, 0.10f, 0.14f, 0.75f);

        Button btn = box.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            ShowInfoModal("ЖОБА ЖОЛ КАРТАСЫ (ROADMAP)",
                "• 1-АПТА: Негізгі шайқас және Гоблиндер\n• 2-АПТА: Басқару динамикасы мен Әрекеттер\n• 3-АПТА: Жаңа жаулар мен Марапат жүйесі\n• 4-АПТА: Босс шайқасы және Даму сатысы",
                "Ағымдағы нұсқа: MVP Protocol 1-4 | Дайындық: 100%");
        });

        GameObject textObj = new GameObject("ScopeText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(box.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-20, -10);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = "<b>1-АПТА:</b> Негізгі шайқас және Гоблиндер\n<b>2-АПТА:</b> Басқару және Әрекеттер\n<b>3-АПТА:</b> Жаңа жаулар мен Марапаттар\n<b>4-АПТА:</b> Босс шайқасы және Прогресс";
        tmp.fontSize = 13.5f;
        tmp.lineSpacing = 10f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(0.80f, 0.85f, 0.90f, 0.95f);
    }

    private void CreateHotspot(string name, string title, string desc, string stats, Vector2 anchoredPos, Vector2 size)
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
        // Тыныш күйде көрінбейтін немесе өте жұмсақ аура
        img.color = new Color(0.3f, 0.6f, 1f, 0.05f);

        Button btn = hotspot.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.3f, 0.6f, 1f, 0.05f);
        cb.highlightedColor = new Color(0.4f, 0.7f, 1f, 0.35f);
        cb.pressedColor = new Color(0.9f, 0.3f, 0.3f, 0.5f);
        btn.colors = cb;

        btn.onClick.AddListener(() => OnZoneClicked(title, desc, stats));
        AddHoverAnimation(hotspot);
    }

    private void CreateRuneElement(string name, string runeName, string combo, string stats, Vector2 anchoredPos, Vector2 size, Color glowColor)
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
        img.color = glowColor;

        Button btn = runeObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = glowColor;
        cb.highlightedColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.75f);
        cb.pressedColor = Color.white;
        btn.colors = cb;

        btn.onClick.AddListener(() => OnRuneClicked(runeName, combo, stats));
        AddHoverAnimation(runeObj);
    }

    private void CreateSlotButton(string name, int slotIndex, string itemName, string itemDesc, Vector2 anchoredPos)
    {
        GameObject slotObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        slotObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = slotObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(75, 75);

        Image img = slotObj.GetComponent<Image>();
        img.color = new Color(0.15f, 0.18f, 0.24f, 0.5f);

        Button btn = slotObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = img.color;
        cb.highlightedColor = new Color(0.35f, 0.45f, 0.65f, 0.8f);
        cb.pressedColor = new Color(0.5f, 0.7f, 1f, 1f);
        btn.colors = cb;

        btn.onClick.AddListener(() => OnSlotClicked(slotIndex, itemName, itemDesc));
        AddHoverAnimation(slotObj);

        // Сан белгісі
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(slotObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = slotIndex.ToString();
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.85f, 0.9f, 1f, 0.8f);
    }

    #endregion

    #region Модальді Терезелер (Popups & Modals)

    private void CreatePopups()
    {
        if (targetCanvas == null) return;

        // Қараңғы артқы фон
        popupOverlay = new GameObject("PopupOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        popupOverlay.transform.SetParent(targetCanvas.transform, false);

        RectTransform overlayRt = popupOverlay.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;

        Image overlayImg = popupOverlay.GetComponent<Image>();
        overlayImg.color = new Color(0.02f, 0.03f, 0.05f, 0.82f);

        Button overlayBtn = popupOverlay.GetComponent<Button>();
        overlayBtn.onClick.AddListener(CloseAllPopups);

        // 1. БАПТАУЛАР ТЕРЕЗЕСІ
        settingsPanel = CreateModalFrame("SettingsPanel", "БАПТАУЛАР", new Vector2(580, 520));
        CreateSettingsContent(settingsPanel);

        // 2. ПРОГРЕСТІ ЖҮКТЕУ ТЕРЕЗЕСІ
        loadProgressPanel = CreateModalFrame("LoadProgressPanel", "ПРОГРЕСТІ ЖҮКТЕУ", new Vector2(620, 520));
        CreateLoadProgressContent(loadProgressPanel);

        // 3. ОЙЫННАН ШЫҒУДЫ РАСТАУ ТЕРЕЗЕСІ
        quitConfirmPanel = CreateModalFrame("QuitConfirmPanel", "ОЙЫННАН ШЫҒУ", new Vector2(480, 280));
        CreateQuitConfirmContent(quitConfirmPanel);

        // 4. СИПАТТАМА / АҚПАРАТ ТЕРЕЗЕСІ
        infoPanel = CreateModalFrame("InfoPanel", "АҚПАРАТ", new Vector2(560, 420));
        CreateInfoContent(infoPanel);

        // Бастапқыда барлығын жабу
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
        img.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

        // Тақырып
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
        titleTmp.color = new Color(0.95f, 0.85f, 0.45f); // Алтын түстес

        // Жабу батырмасы (X)
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
        // 1. Дыбыс деңгейі
        CreateSettingSlider(panel, "Жалпы дыбыс (Master Volume):", 80, 0.8f, val =>
        {
            AudioListener.volume = val;
            Debug.Log($"Дыбыс өзгертілді: {val * 100}%");
        });

        // 2. SFX деңгейі
        CreateSettingSlider(panel, "Эффектілер дыбысы (SFX):", 0, 0.9f, val =>
        {
            if (audioSource != null) audioSource.volume = val;
        });

        // 3. Экран режимі (Толық экран)
        CreateSettingToggle(panel, "Толық экран режимі (Fullscreen):", -80, Screen.fullScreen, isFull =>
        {
            Screen.fullScreen = isFull;
            PlaySound(sfxClick, 0.6f);
        });

        // 4. Тіл таңдауы
        CreateSettingLabel(panel, "Тіл (Language): Қазақ тілі (Таңдалған)", -140);

        // Сақтау батырмасы
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

        // Slider негізі
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
        // 3 Сақтау слоты
        CreateSaveSlotRow(panel, 1, "СЛОТ 1: АРХИВ БАТЫРЫ", "5-Деңгей | Тірі қалған уақыт: 14:20 | 2,450 Алтын", 70);
        CreateSaveSlotRow(panel, 2, "СЛОТ 2: ҚАНДЫ ТАҢДАУ", "2-Деңгей | Тірі қалған уақыт: 05:12 | 680 Алтын", -25);
        CreateSaveSlotRow(panel, 3, "СЛОТ 3: БОС САҚТАУ", "[Жаңа ойынға арналған бос орын]", -120);

        // Жүктеу батырмасы
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
            Debug.Log($"Сақтау слоты таңдалды: {slotNum}");
        });

        // Мәтіндер
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
        // Тақырып
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

        // Негізгі сипаттама мәтіні
        GameObject bodyObj = new GameObject("InfoBody", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        bodyObj.transform.SetParent(panel.transform, false);

        RectTransform bodyRt = bodyObj.GetComponent<RectTransform>();
        bodyRt.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRt.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRt.anchoredPosition = new Vector2(0, 15);
        bodyRt.sizeDelta = new Vector2(480, 95);

        infoBodyText = bodyObj.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) infoBodyText.font = defaultFont;
        infoBodyText.text = "Таңдалған аймақ немесе элемент туралы толық мәлімет.";
        infoBodyText.fontSize = 16.5f;
        infoBodyText.lineSpacing = 8f;
        infoBodyText.alignment = TextAlignmentOptions.TopLeft;
        infoBodyText.color = new Color(0.88f, 0.92f, 0.98f);

        // Статистика / Қасиеттер мәтіні
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
        infoStatsText.color = new Color(0.4f, 0.85f, 0.65f); // Жасыл жарық

        // Түсінікті батырмасы
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

    #region Анимация және Дыбыс Синтезі (Animations & Procedural Audio)

    private void AddHoverAnimation(GameObject target)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) =>
        {
            PlaySound(sfxHover, 0.55f);
            StopCoroutine("ScaleRoutine");
            StartCoroutine(ScaleRoutine(target.transform, 1.06f, 0.12f));
        });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit
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
        t.localScale = new Vector3(0.85f, 0.85f, 1f);
        float elapsed = 0f;
        float dur = 0.2f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / dur;
            // Серіппелі әсер (Spring overshoot)
            float scale = Mathf.Lerp(0.85f, 1.0f, progress) + Mathf.Sin(progress * Mathf.PI) * 0.05f;
            t.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Синтезделген сапалы дыбыстарды жасау (No external audio files needed!)
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

            // Envelope (Шабуыл және өшу)
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