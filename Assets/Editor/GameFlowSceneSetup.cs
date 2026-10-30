using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameFlowSceneSetup
{
    private const string BootScenePath = "Assets/Boot.unity";
    private const string IntroScenePath = "Assets/Intro.unity";
    private const string MainMenuScenePath = "Assets/MainMenu.unity";
    private const string GameScenePath = "Assets/nurzhaaaaaaaaaaaaan 17.10.206.unity";
    private const string BootBackgroundSpritePath =
        "Assets/Scripts/Gemini_Generated_Image_ntipbntipbntipbn.jpg";
    private const string IntroBackgroundSpritePath =
        "Assets/Gemini_Generated_Image_yrqxr0yrqxr0yrqx.jpg";
    private const string CharacterPortraitSpritePath = "Assets/IntroCharacter.png";
    private const string LoadingZombieSpritePath =
        "Assets/png-transparent-pixel-art-animation-2d-computer-graphics-zombie-2d-fictional-character-cartoon-zombie-removebg-preview.png";

    [MenuItem("Tools/Game Flow/Generate Boot And Intro Scenes")]
    public static void GenerateGameFlowScenes()
    {
        Sprite bootBackground = AssetDatabase.LoadAssetAtPath<Sprite>(BootBackgroundSpritePath);
        Sprite introBackground = AssetDatabase.LoadAssetAtPath<Sprite>(IntroBackgroundSpritePath);
        Sprite characterPortrait =
            AssetDatabase.LoadAssetAtPath<Sprite>(CharacterPortraitSpritePath);
        Sprite loadingZombie = AssetDatabase.LoadAssetAtPath<Sprite>(LoadingZombieSpritePath);
        if (bootBackground == null || introBackground == null || characterPortrait == null ||
            loadingZombie == null)
        {
            throw new InvalidOperationException(
                "Boot, intro немесе кейіпкер суреті табылмады.");
        }

        CreateBootScene(bootBackground, loadingZombie);
        CreateIntroScene(introBackground, characterPortrait);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ConfigureBuildSettings();
        Debug.Log("Boot and mission intro scenes were generated successfully.");
    }

    private static void CreateBootScene(Sprite background, Sprite loadingZombie)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Canvas canvas = CreateCanvas("BootCanvas");
        CreateFullScreenImage(canvas.transform, "Background", background, Color.white);
        CreateFullScreenImage(canvas.transform, "BackgroundTint", null,
            new Color(0.015f, 0.025f, 0.045f, 0.7f));

        CreateText(canvas.transform, "GameTitle", "",
            new Vector2(0f, 170f), new Vector2(1500f, 150f), 104f,
            FontStyles.Bold, new Color(1f, 0.92f, 0.88f, 1f));
        CreateText(canvas.transform, "Subtitle", "ТІРІ ҚАЛУ — БАСТЫ МАҚСАТ",
            new Vector2(0f, 75f), new Vector2(1200f, 64f), 34f,
            FontStyles.Bold, Color.white);

        Slider progressBar = CreateProgressBar(canvas.transform);
        Image zombieRunner = CreateLoadingZombie(progressBar, loadingZombie);
        CreateText(canvas.transform, "LoadingLabel", "ЗОМБИЛЕР ЖАҚЫНДАП КЕЛЕДІ...",
            new Vector2(0f, -360f), new Vector2(900f, 50f), 27f,
            FontStyles.Bold, Color.white);

        GameObject loaderObject = new("BootLoader");
        BootLoader loader = loaderObject.AddComponent<BootLoader>();
        SerializedObject serializedLoader = new(loader);
        serializedLoader.FindProperty("progressBar").objectReferenceValue = progressBar;
        serializedLoader.FindProperty("loadingZombie").objectReferenceValue = zombieRunner;
        serializedLoader.FindProperty("nextScene").stringValue = "MainMenu";
        serializedLoader.FindProperty("minimumDisplayTime").floatValue = 2f;
        serializedLoader.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, BootScenePath);
    }

    private static void CreateIntroScene(Sprite background, Sprite characterPortrait)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Canvas canvas = CreateCanvas("IntroCanvas");
        CreateFullScreenImage(canvas.transform, "Background", background, Color.white);
        CreateFullScreenImage(canvas.transform, "BackgroundTint", null,
            new Color(0.008f, 0.018f, 0.035f, 0.56f));

        GameObject introObject = new("MissionIntro");
        MissionIntro intro = introObject.AddComponent<MissionIntro>();
        SerializedObject serializedIntro = new(intro);
        CreateCamera();
        CreateIntroContent(canvas.transform, characterPortrait, intro, serializedIntro);
        serializedIntro.FindProperty("nextScene").stringValue = "nurzhaaaaaaaaaaaaan 17.10.206";
        serializedIntro.FindProperty("fadeDuration").floatValue = 0.45f;
        serializedIntro.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, IntroScenePath);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera", typeof(Transform), typeof(Camera));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.008f, 0.018f, 0.035f, 1f);
    }

    private static void CreateIntroContent(Transform parent, Sprite character, MissionIntro intro,
        SerializedObject serializedIntro)
    {
        RectTransform panelRect = CreateRect(parent, "DialoguePanel",
            Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(1f, 0.54f), new Vector2(0.5f, 0f));
        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.008f, 0.02f, 0.04f, 0.88f);
        panelImage.raycastTarget = false;
        CanvasGroup panelGroup = panelRect.gameObject.AddComponent<CanvasGroup>();

        RectTransform portraitRect = CreateRect(panelRect, "DialogueCharacter",
            new Vector2(0f, 0f), new Vector2(-20f, -24f),
            Vector2.zero, new Vector2(0.41f, 1f), new Vector2(0.5f, 0f));
        Image portrait = portraitRect.gameObject.AddComponent<Image>();
        portrait.sprite = character;
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        portraitRect.localScale = Vector3.one * 1.3f;
        portraitRect.SetAsFirstSibling();

        RectTransform accentRect = CreateRect(panelRect, "DialogueAccent",
            Vector2.zero, new Vector2(0f, 4f),
            new Vector2(0.41f, 1f), Vector2.one, new Vector2(0.5f, 1f));
        Image accent = accentRect.gameObject.AddComponent<Image>();
        accent.color = new Color(0.28f, 0.69f, 0.9f, 1f);
        accent.raycastTarget = false;

        TMP_Text speaker = CreateText(panelRect, "SpeakerName", "ТІРІ ҚАЛҒАН ҚЫЗ",
            new Vector2(48f, -48f), new Vector2(-96f, 56f),
            34f, FontStyles.Bold, new Color(0.5f, 0.8f, 0.96f, 1f),
            new Vector2(0.41f, 1f), Vector2.one, new Vector2(0f, 1f));
        speaker.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_Text dialogue = CreateText(panelRect, "DialogueText",
            "Зомбилер қаланы басып алды! WASD-пен қозғал да, тінтуірдің сол жақ батырмасымен шабуылда.",
            new Vector2(48f, -128f), new Vector2(-96f, 170f),
            31f, FontStyles.Normal, Color.white,
            new Vector2(0.41f, 1f), Vector2.one, new Vector2(0f, 1f));
        dialogue.alignment = TextAlignmentOptions.TopLeft;

        TMP_Text continueHint = CreateText(panelRect, "ContinueHint",
            "ЖАЛҒАСТЫРУ  |  SPACE НЕМЕСЕ ШЕРТУ",
            new Vector2(-36f, 22f), new Vector2(730f, 40f),
            21f, FontStyles.Bold, new Color(0.94f, 0.7f, 0.42f, 1f),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        continueHint.alignment = TextAlignmentOptions.MidlineRight;

        SerializedProperty portraitProperty =
            serializedIntro.FindProperty("dialogueText");
        portraitProperty.objectReferenceValue = dialogue;
        serializedIntro.FindProperty("dialogueGroup").objectReferenceValue = panelGroup;
        serializedIntro.FindProperty("charactersPerSecond").floatValue = 38f;
        EditorUtility.SetDirty(intro);
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new(name, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static Image CreateFullScreenImage(Transform parent, string name, Sprite sprite,
        Color color)
    {
        RectTransform rect = CreateRect(parent, name, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Slider CreateProgressBar(Transform parent)
    {
        RectTransform root = CreateRect(parent, "ProgressBar",
            new Vector2(0f, -300f), new Vector2(900f, 30f));
        Image background = root.gameObject.AddComponent<Image>();
        background.color = new Color(0.035f, 0.045f, 0.065f, 0.96f);
        background.raycastTarget = false;

        RectTransform fillRect = CreateRect(root, "Fill", Vector2.zero, Vector2.zero);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
        Image fill = fillRect.gameObject.AddComponent<Image>();
        fill.color = new Color(0.28f, 0.9f, 0.3f, 1f);
        fill.raycastTarget = false;

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.targetGraphic = background;
        slider.fillRect = fillRect;
        slider.handleRect = null;
        return slider;
    }

    private static Image CreateLoadingZombie(Slider progressBar, Sprite zombieSprite)
    {
        RectTransform rect = CreateRect(progressBar.transform, "LoadingZombie",
            Vector2.zero, new Vector2(110f, 132f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = zombieSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value,
        Vector2 position, Vector2 size, float fontSize, FontStyles fontStyle, Color color,
        Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null)
    {
        RectTransform rect = CreateRect(parent, name, position, size, anchorMin, anchorMax, pivot);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>(
            "Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        if (font == null)
        {
            throw new InvalidOperationException("TextMeshPro әдепкі қарпі табылмады.");
        }

        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.outlineColor = new Color(0.01f, 0.015f, 0.025f, 1f);
        text.outlineWidth = 0.18f;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(Transform parent, string name,
        Vector2 position, Vector2 size, Vector2? anchorMin = null, Vector2? anchorMax = null,
        Vector2? pivot = null)
    {
        GameObject child = new(name, typeof(RectTransform));
        child.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin ?? new Vector2(0.5f, 0.5f);
        rect.anchorMax = anchorMax ?? new Vector2(0.5f, 0.5f);
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static void ConfigureBuildSettings()
    {
        string[] requiredPaths =
        {
            BootScenePath,
            MainMenuScenePath,
            IntroScenePath,
            GameScenePath
        };
        var scenes = new List<EditorBuildSettingsScene>(requiredPaths.Length + 4);
        foreach (string path in requiredPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                throw new InvalidOperationException($"Build сахнасы табылмады: {path}");
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!Array.Exists(requiredPaths, path => path == scene.path))
            {
                scenes.Add(scene);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
