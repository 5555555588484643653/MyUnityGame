using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class BossAnimationSetup
{
    private const string PoseSheetPath = "Assets/Animations/Boss/BossCharacterPoses.png";
    private const string AnimationSheetPath = "Assets/Animations/Boss/BossAnimationSheet.png";
    private const string AppearanceSpritePath = "Assets/Animations/Boss/BossAppearance.png";
    private const string FrameFolder = "Assets/Animations/Boss/Frames";
    private const string AnimationFolder = "Assets/Animations/Boss";
    private const string ControllerPath = "Assets/Animations/Boss/Boss.controller";
    private const string PrefabPath = "Assets/Resources/BossPrefab.prefab";
    private const int PoseSheetWidth = 352;
    private const int PoseSheetHeight = 181;
    private const int AnimationSheetWidth = 717;
    private const int AnimationSheetHeight = 296;

    private sealed class AnimationRow
    {
        public readonly string Name;
        public readonly int FrameCount;
        public readonly int FramesPerSecond;
        public readonly bool Loops;
        public readonly RectInt[] FrameRegions;

        public AnimationRow(
            string name,
            int framesPerSecond,
            bool loops,
            params RectInt[] frameRegions)
        {
            Name = name;
            FrameRegions = frameRegions;
            FrameCount = frameRegions.Length;
            FramesPerSecond = framesPerSecond;
            Loops = loops;
        }

        public AnimationRow(string name, int frameCount, int framesPerSecond, bool loops)
        {
            Name = name;
            FrameRegions = Array.Empty<RectInt>();
            FrameCount = frameCount;
            FramesPerSecond = framesPerSecond;
            Loops = loops;
        }
    }

    private static readonly AnimationRow[] Rows =
    {
        new AnimationRow(
            "Idle",
            8,
            true,
            new RectInt(72, 30, 54, 47),
            new RectInt(153, 30, 75, 47),
            new RectInt(246, 30, 77, 47),
            new RectInt(335, 30, 74, 47),
            new RectInt(422, 29, 71, 55),
            new RectInt(510, 30, 61, 52),
            new RectInt(579, 30, 66, 47)),
        new AnimationRow(
            "Walk",
            10,
            true,
            new RectInt(71, 87, 48, 44),
            new RectInt(143, 86, 47, 45),
            new RectInt(215, 87, 47, 44),
            new RectInt(287, 87, 47, 44),
            new RectInt(357, 87, 48, 45)),
        new AnimationRow(
            "Attack",
            12,
            false,
            new RectInt(70, 139, 49, 45),
            new RectInt(138, 146, 53, 39),
            new RectInt(206, 139, 50, 45),
            new RectInt(273, 147, 62, 35),
            new RectInt(331, 156, 58, 27),
            new RectInt(390, 151, 60, 35),
            new RectInt(457, 158, 80, 28),
            new RectInt(547, 160, 65, 24)),
        new AnimationRow("Hit", 5, 12, false),
        new AnimationRow("Die", 8, 10, false),
        new AnimationRow(
            "SpecialAttack",
            10,
            false,
            new RectInt(5, 235, 55, 60),
            new RectInt(65, 230, 67, 63),
            new RectInt(143, 208, 72, 81),
            new RectInt(226, 208, 69, 82),
            new RectInt(288, 207, 112, 82),
            new RectInt(378, 199, 133, 91),
            new RectInt(500, 205, 143, 87),
            new RectInt(650, 224, 65, 67))
    };

    static BossAnimationSetup()
    {
        EditorApplication.delayCall += GenerateIfMissing;
    }

    [MenuItem("Tools/Boss/Generate Boss Animations From Sprite Sheets")]
    public static void GenerateFromMenu()
    {
        Generate();
    }

    private static void GenerateIfMissing()
    {
        bool aSheetIsNewer = File.Exists(PoseSheetPath) &&
            File.Exists(AnimationSheetPath) &&
            File.Exists(ControllerPath) &&
            (File.GetLastWriteTimeUtc(PoseSheetPath) > File.GetLastWriteTimeUtc(ControllerPath) ||
             File.GetLastWriteTimeUtc(AnimationSheetPath) > File.GetLastWriteTimeUtc(ControllerPath));
        if (File.Exists(PoseSheetPath) &&
            File.Exists(AnimationSheetPath) &&
            (!File.Exists(ControllerPath) || !File.Exists(PrefabPath) || aSheetIsNewer))
        {
            Generate();
        }
    }

    private static void Generate()
    {
        if (!File.Exists(PoseSheetPath))
        {
            throw new FileNotFoundException("Boss character pose image was not found.", PoseSheetPath);
        }

        if (!File.Exists(AnimationSheetPath))
        {
            throw new FileNotFoundException("Boss animation sheet was not found.", AnimationSheetPath);
        }

        EnsureFolder("Assets/Animations/Boss");
        EnsureFolder(FrameFolder);
        EnsureFolder("Assets/Resources");

        Texture2D poseSheet = LoadReadableSheet(PoseSheetPath, PoseSheetWidth, PoseSheetHeight);
        Texture2D animationSheet = LoadReadableSheet(
            AnimationSheetPath,
            AnimationSheetWidth,
            AnimationSheetHeight);
        if (poseSheet == null || animationSheet == null)
        {
            throw new InvalidDataException("Boss sprite sheets could not be loaded.");
        }

        Color32[] mainPose = ExtractMainPose(poseSheet.GetPixels32());
        Color32[] animationPixels = animationSheet.GetPixels32();
        var framesByName = new Dictionary<string, Sprite[]>();
        foreach (AnimationRow row in Rows)
        {
            var frames = new Sprite[row.FrameCount];
            for (int i = 0; i < row.FrameCount; i++)
            {
                string path = $"{FrameFolder}/Boss_{row.Name}_{i + 1:00}.png";
                byte[] frame = row.FrameRegions.Length > 0
                    ? CreateAnimationFrame(animationPixels, row.FrameRegions[i])
                    : CreatePoseFrame(mainPose);
                File.WriteAllBytes(path, frame);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                ConfigureFrame(path, row.Name == "Hit" || row.Name == "Die" ? 12f : 16f);
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i] == null)
                {
                    throw new InvalidOperationException($"Could not import boss frame: {path}");
                }
            }

            framesByName.Add(row.Name, frames);
            for (int i = row.FrameCount; i < 8; i++)
            {
                AssetDatabase.DeleteAsset($"{FrameFolder}/Boss_{row.Name}_{i + 1:00}.png");
            }
        }

        AssetDatabase.Refresh();
        Sprite bossAppearance = AssetDatabase.LoadAssetAtPath<Sprite>(AppearanceSpritePath);
        if (bossAppearance == null)
        {
            throw new InvalidOperationException(
                $"Boss appearance sprite could not be loaded: {AppearanceSpritePath}");
        }

        framesByName["Idle"] = new[] { bossAppearance };
        AnimatorController controller = CreateController(framesByName);
        AssetDatabase.SaveAssets();
        CreateBossPrefab(controller, bossAppearance);
        AssetDatabase.SaveAssets();
        Debug.Log("Boss sprite animations and boss prefab were generated successfully.");
    }

    private static Color32[] ExtractMainPose(Color32[] source)
    {
        const int sourceX = 14;
        const int sourceY = PoseSheetHeight - 28 - 124;
        const int sourceWidth = 145;
        const int sourceHeight = 124;
        var pixels = new Color32[sourceWidth * sourceHeight];
        for (int y = 0; y < sourceHeight; y++)
        {
            Array.Copy(
                source,
                (sourceY + y) * PoseSheetWidth + sourceX,
                pixels,
                y * sourceWidth,
                sourceWidth);
        }

        RemoveConnectedBackground(pixels, sourceWidth, sourceHeight);
        return pixels;
    }

    private static Texture2D LoadReadableSheet(string path, int expectedWidth, int expectedHeight)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidDataException($"Boss image has no texture importer: {path}");
        }

        importer.isReadable = true;
        importer.textureType = TextureImporterType.Default;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (sheet == null || sheet.width != expectedWidth || sheet.height != expectedHeight)
        {
            throw new InvalidDataException(
                $"Expected a {expectedWidth}x{expectedHeight} boss image at {path}.");
        }

        return sheet;
    }

    private static byte[] CreateAnimationFrame(Color32[] source, RectInt region)
    {
        const int frameWidth = 112;
        const int frameHeight = 80;
        int sourceY = AnimationSheetHeight - region.y - region.height;
        var pixels = new Color32[region.width * region.height];
        for (int y = 0; y < region.height; y++)
        {
            Array.Copy(
                source,
                (sourceY + y) * AnimationSheetWidth + region.x,
                pixels,
                y * region.width,
                region.width);
        }

        RemoveConnectedBackground(pixels, region.width, region.height);
        return CreateFramedSprite(pixels, region.width, region.height, frameWidth, frameHeight);
    }

    private static byte[] CreatePoseFrame(Color32[] pose)
    {
        const int sourceWidth = 145;
        const int sourceHeight = 124;
        const int frameWidth = 78;
        const int frameHeight = 62;
        int minX = sourceWidth;
        int minY = sourceHeight;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < sourceHeight; y++)
        {
            for (int x = 0; x < sourceWidth; x++)
            {
                if (pose[y * sourceWidth + x].a == 0)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            throw new InvalidDataException("The boss character pose was empty after background removal.");
        }

        return CreateFramedSprite(
            pose,
            sourceWidth,
            sourceHeight,
            frameWidth,
            frameHeight,
            minX,
            minY,
            maxX,
            maxY);
    }

    private static byte[] CreateFramedSprite(
        Color32[] source,
        int sourceWidth,
        int sourceHeight,
        int frameWidth,
        int frameHeight,
        int minX = -1,
        int minY = -1,
        int maxX = -1,
        int maxY = -1)
    {
        if (minX < 0 || minY < 0 || maxX < minX || maxY < minY)
        {
            minX = sourceWidth;
            minY = sourceHeight;
            maxX = -1;
            maxY = -1;
            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < sourceWidth; x++)
                {
                    if (source[y * sourceWidth + x].a == 0)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        if (maxX < minX || maxY < minY)
        {
            throw new InvalidDataException("A boss animation frame was empty after background removal.");
        }

        int contentWidth = maxX - minX + 1;
        int contentHeight = maxY - minY + 1;
        float scale = Mathf.Min(
            (frameWidth - 8f) / contentWidth,
            (frameHeight - 8f) / contentHeight);
        int outputWidth = Mathf.Max(1, Mathf.RoundToInt(contentWidth * scale));
        int outputHeight = Mathf.Max(1, Mathf.RoundToInt(contentHeight * scale));
        int targetX = (frameWidth - outputWidth) / 2;
        int targetY = 2;
        var framePixels = new Color32[frameWidth * frameHeight];

        for (int y = 0; y < outputHeight; y++)
        {
            for (int x = 0; x < outputWidth; x++)
            {
                int sourceFrame = (minY + y * contentHeight / outputHeight) * sourceWidth +
                    minX + x * contentWidth / outputWidth;
                framePixels[(targetY + y) * frameWidth + targetX + x] = source[sourceFrame];
            }
        }

        var texture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
        texture.SetPixels32(framePixels);
        texture.Apply(false, false);
        byte[] png = ImageConversion.EncodeToPNG(texture);
        UnityEngine.Object.DestroyImmediate(texture);
        return png;
    }

    private static void RemoveConnectedBackground(Color32[] pixels, int width, int height)
    {
        var visited = new bool[pixels.Length];
        var queue = new int[pixels.Length];
        int head = 0;
        int tail = 0;

        for (int x = 0; x < width; x++)
        {
            AddBackground(x);
            AddBackground((height - 1) * width + x);
        }

        for (int y = 1; y < height - 1; y++)
        {
            AddBackground(y * width);
            AddBackground(y * width + width - 1);
        }

        while (head < tail)
        {
            int index = queue[head++];
            int x = index % width;
            int y = index / width;
            pixels[index].a = 0;

            if (x > 0) AddBackground(index - 1);
            if (x + 1 < width) AddBackground(index + 1);
            if (y > 0) AddBackground(index - width);
            if (y + 1 < height) AddBackground(index + width);
        }

        void AddBackground(int index)
        {
            if (!visited[index] && IsBackground(pixels[index]))
            {
                visited[index] = true;
                queue[tail++] = index;
            }
        }
    }

    private static bool IsBackground(Color32 color)
    {
        return color.r < 55 && color.g < 62 && color.b < 76 &&
            color.b > color.r + 6 && color.g > color.r + 2;
    }

    private static void ConfigureFrame(string path, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidDataException($"Boss frame has no texture importer: {path}");
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spritePivot = new Vector2(0.5f, 0f);
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static AnimatorController CreateController(Dictionary<string, Sprite[]> framesByName)
    {
        AssetDatabase.DeleteAsset(ControllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.parameters = new[]
        {
            new AnimatorControllerParameter { name = "Speed", type = AnimatorControllerParameterType.Float },
            new AnimatorControllerParameter { name = "Attack", type = AnimatorControllerParameterType.Trigger },
            new AnimatorControllerParameter { name = "Hit", type = AnimatorControllerParameterType.Trigger },
            new AnimatorControllerParameter { name = "SpecialAttack", type = AnimatorControllerParameterType.Trigger },
            new AnimatorControllerParameter { name = "Die", type = AnimatorControllerParameterType.Trigger }
        };

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.AddState("Idle");
        AnimatorState walk = machine.AddState("Walk");
        AnimatorState attack = machine.AddState("Attack");
        AnimatorState hit = machine.AddState("Hit");
        AnimatorState die = machine.AddState("Die");
        AnimatorState specialAttack = machine.AddState("SpecialAttack");
        machine.defaultState = idle;
        idle.motion = CreateClip(
            "Boss_Idle",
            framesByName["Idle"],
            GetRow("Idle").FramesPerSecond,
            GetRow("Idle").Loops);
        walk.motion = CreateClip(
            "Boss_Walk",
            framesByName["Walk"],
            GetRow("Walk").FramesPerSecond,
            GetRow("Walk").Loops);
        attack.motion = CreateClip(
            "Boss_Attack",
            framesByName["Attack"],
            GetRow("Attack").FramesPerSecond,
            GetRow("Attack").Loops);
        hit.motion = CreateClip(
            "Boss_Hit",
            framesByName["Hit"],
            GetRow("Hit").FramesPerSecond,
            GetRow("Hit").Loops);
        die.motion = CreateClip(
            "Boss_Die",
            framesByName["Die"],
            GetRow("Die").FramesPerSecond,
            GetRow("Die").Loops);
        specialAttack.motion = CreateClip(
            "Boss_SpecialAttack",
            framesByName["SpecialAttack"],
            GetRow("SpecialAttack").FramesPerSecond,
            GetRow("SpecialAttack").Loops);

        AnimatorStateTransition idleToWalk = idle.AddTransition(walk);
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.08f;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
        AnimatorStateTransition walkToIdle = walk.AddTransition(idle);
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.08f;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");

        AddActionTransition(machine, attack, "Attack");
        AddActionTransition(machine, hit, "Hit");
        AddActionTransition(machine, specialAttack, "SpecialAttack");
        AddActionTransition(machine, die, "Die");
        AddReturnTransition(attack, idle);
        AddReturnTransition(hit, idle);
        AddReturnTransition(specialAttack, idle);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimationRow GetRow(string name)
    {
        foreach (AnimationRow row in Rows)
        {
            if (row.Name == name)
            {
                return row;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(name), name, "Boss animation row was not defined.");
    }

    private static AnimationClip CreateClip(string name, Sprite[] frames, int frameRate, bool loops)
    {
        var clip = new AnimationClip
        {
            name = name,
            frameRate = frameRate,
            wrapMode = loops ? WrapMode.Loop : WrapMode.Once
        };
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / (float)frameRate,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite"),
            keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loops;
        settings.stopTime = frames.Length / (float)frameRate;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AddVisualMotion(clip, name, settings.stopTime);
        string path = $"{AnimationFolder}/{name}.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void AddVisualMotion(AnimationClip clip, string animationName, float duration)
    {
        EditorCurveBinding positionY = EditorCurveBinding.FloatCurve(
            "Visual",
            typeof(Transform),
            "m_LocalPosition.y");
        EditorCurveBinding scaleX = EditorCurveBinding.FloatCurve(
            "Visual",
            typeof(Transform),
            "m_LocalScale.x");
        EditorCurveBinding scaleY = EditorCurveBinding.FloatCurve(
            "Visual",
            typeof(Transform),
            "m_LocalScale.y");

        switch (animationName)
        {
            case "Boss_Idle":
                AnimationUtility.SetEditorCurve(
                    clip,
                    positionY,
                    MakeCurve(duration, 0f, 0f, 0.5f, 0.025f, 1f, 0f));
                break;
            case "Boss_Walk":
                AnimationUtility.SetEditorCurve(
                    clip,
                    positionY,
                    MakeCurve(duration, 0f, 0f, 0.25f, 0.06f, 0.5f, 0f, 0.75f, 0.06f, 1f, 0f));
                AnimationUtility.SetEditorCurve(
                    clip,
                    scaleY,
                    MakeCurve(duration, 0f, 1f, 0.25f, 0.96f, 0.5f, 1f, 0.75f, 0.96f, 1f, 1f));
                break;
            case "Boss_Attack":
                AnimationUtility.SetEditorCurve(
                    clip,
                    positionY,
                    MakeCurve(duration, 0f, 0f, 0.35f, 0f, 0.55f, 0.08f, 0.75f, 0f, 1f, 0f));
                AnimationUtility.SetEditorCurve(
                    clip,
                    scaleX,
                    MakeCurve(duration, 0f, 1f, 0.55f, 1.08f, 0.8f, 0.98f, 1f, 1f));
                break;
            case "Boss_Hit":
                AnimationUtility.SetEditorCurve(
                    clip,
                    scaleX,
                    MakeCurve(duration, 0f, 1f, 0.12f, 0.9f, 0.4f, 1f, 1f, 1f));
                AnimationUtility.SetEditorCurve(
                    clip,
                    scaleY,
                    MakeCurve(duration, 0f, 1f, 0.12f, 1.06f, 0.4f, 1f, 1f, 1f));
                break;
            case "Boss_Die":
                EditorCurveBinding rotationZ = EditorCurveBinding.FloatCurve(
                    "Visual",
                    typeof(Transform),
                    "localEulerAnglesRaw.z");
                AnimationUtility.SetEditorCurve(
                    clip,
                    rotationZ,
                    MakeCurve(duration, 0f, 0f, 0.45f, -20f, 0.75f, -65f, 1f, -90f));
                AnimationUtility.SetEditorCurve(
                    clip,
                    positionY,
                    MakeCurve(duration, 0f, 0f, 0.5f, -0.02f, 1f, -0.04f));
                break;
            case "Boss_SpecialAttack":
                AnimationUtility.SetEditorCurve(
                    clip,
                    positionY,
                    MakeCurve(duration, 0f, 0f, 0.3f, 0.03f, 0.65f, -0.06f, 1f, 0f));
                break;
        }
    }

    private static AnimationCurve MakeCurve(float duration, params float[] timeValuePairs)
    {
        var keys = new Keyframe[timeValuePairs.Length / 2];
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i] = new Keyframe(
                timeValuePairs[i * 2] * duration,
                timeValuePairs[i * 2 + 1]);
        }

        return new AnimationCurve(keys);
    }

    private static void AddActionTransition(AnimatorStateMachine machine, AnimatorState state, string trigger)
    {
        AnimatorStateTransition transition = machine.AddAnyStateTransition(state);
        transition.hasExitTime = false;
        transition.duration = 0.04f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    private static void AddReturnTransition(AnimatorState state, AnimatorState idle)
    {
        AnimatorStateTransition transition = state.AddTransition(idle);
        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = 0.04f;
    }

    private static void CreateBossPrefab(AnimatorController controller, Sprite bossAppearance)
    {
        GameObject root = PrefabUtility.LoadPrefabContents("Assets/Scripts/ZombiePrefab.prefab");
        try
        {
            root.name = "BossPrefab";
            Animator animator = root.GetComponent<Animator>();
            SpriteRenderer renderer = root.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null || root.GetComponent<RangedEnemy>() == null)
            {
                throw new InvalidDataException("Zombie prefab is missing required boss components.");
            }

            if (animator == null)
            {
                animator = root.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            renderer.sprite = bossAppearance;
            renderer.color = Color.white;
            root.transform.localScale = new Vector3(0.6f, 0.7f, 1f);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || !AssetDatabase.IsValidFolder(parent))
        {
            throw new DirectoryNotFoundException($"Parent asset folder does not exist: {path}");
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
