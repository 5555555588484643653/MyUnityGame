using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class PlayerAnimationSetup
{
    private const string SourcePath = "Assets/animat.jpg";
    private const string OutputFolder = "Assets/Animations";
    private const string SpriteFolder = OutputFolder + "/Sprites";
    private const string ControllerPath = OutputFolder + "/Player.controller";
    private const string ZombieControllerPath = OutputFolder + "/Zombie.controller";
    private const string ZombiePrefabPath = "Assets/Scripts/ZombiePrefab.prefab";
    private const string GameScenePath = "Assets/nurzhaaaaaaaaaaaaan 17.10.206.unity";
    private const string ControllerParameters = "Speed";

    private sealed class AnimationRow
    {
        public readonly string Name;
        public readonly int Count;
        public readonly int StartX;
        public readonly int CellWidth;
        public readonly int Top;
        public readonly int FrameRate;
        public readonly bool Loops;
        public readonly int[] FrameWidths;

        public AnimationRow(string name, int count, int startX, int cellWidth, int top, int frameRate, bool loops, int[] frameWidths = null)
        {
            Name = name;
            Count = count;
            StartX = startX;
            CellWidth = cellWidth;
            Top = top;
            FrameRate = frameRate;
            Loops = loops;
            FrameWidths = frameWidths;
        }

        public int GetFrameStartX(int frameIndex)
        {
            if (FrameWidths == null)
            {
                return StartX + frameIndex * CellWidth;
            }

            int x = StartX;
            for (int i = 0; i < frameIndex; i++)
            {
                x += FrameWidths[i];
            }

            return x;
        }

        public int GetFrameWidth(int frameIndex)
        {
            return FrameWidths == null ? CellWidth : FrameWidths[frameIndex];
        }
    }

    private static readonly AnimationRow[] Rows =
    {
        new AnimationRow("Idle", 9, 28, 134, 56, 8, true),
        new AnimationRow("Walk", 10, 28, 134, 239, 12, true),
        new AnimationRow("Attack", 10, 28, 134, 422, 16, false),
        new AnimationRow("Special", 6, 28, 0, 607, 10, false, new[] { 134, 133, 199, 196, 176, 169 })
    };

    static PlayerAnimationSetup()
    {
        EditorApplication.delayCall += GenerateIfMissing;
    }

    [MenuItem("Tools/Player Animations/Generate or Rebuild")]
    private static void GenerateFromMenu()
    {
        Generate();
    }

    private static void GenerateIfMissing()
    {
        DateTime scriptModified = File.GetLastWriteTimeUtc("Assets/Editor/PlayerAnimationSetup.cs");
        bool setupIsStale = !File.Exists(ControllerPath) ||
            scriptModified > File.GetLastWriteTimeUtc(ControllerPath);
        setupIsStale |= !File.Exists(ZombieControllerPath) ||
            scriptModified > File.GetLastWriteTimeUtc(ZombieControllerPath);
        if (File.Exists(GameScenePath))
        {
            setupIsStale |= File.ReadAllText(GameScenePath).Contains("&-9223372036854775808");
        }

        if (setupIsStale && File.Exists(SourcePath))
        {
            Generate();
        }
    }

    private static void Generate()
    {
        if (!File.Exists(SourcePath))
        {
            throw new FileNotFoundException("Animation source sheet was not found.", SourcePath);
        }

        EnsureFolder(OutputFolder);
        EnsureFolder(SpriteFolder);

        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(source, File.ReadAllBytes(SourcePath)))
        {
            UnityEngine.Object.DestroyImmediate(source);
            throw new InvalidDataException("Could not read the animation source sheet.");
        }

        if (source.width != 1377 || source.height != 768)
        {
            UnityEngine.Object.DestroyImmediate(source);
            throw new InvalidDataException($"Expected a 1377x768 animation sheet, got {source.width}x{source.height}.");
        }

        var sourcePixels = source.GetPixels32();
        UnityEngine.Object.DestroyImmediate(source);

        var clips = new Dictionary<string, AnimationClip>();
        foreach (AnimationRow row in Rows)
        {
            var sprites = new Sprite[row.Count];
            for (int i = 0; i < row.Count; i++)
            {
                string spritePath = $"{SpriteFolder}/{row.Name}_{i + 1:00}.png";
                byte[] png = ExtractFrame(sourcePixels, row, i);
                File.WriteAllBytes(spritePath, png);
                AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
                ConfigureSprite(spritePath);
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprites[i] == null)
                {
                    throw new InvalidOperationException($"Unity failed to import animation frame: {spritePath}");
                }
            }

            clips.Add(row.Name, CreateClip(row, sprites));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AnimatorController controller = CreateController(clips);
        AnimatorController zombieController = CreateZombieController();
        AssetDatabase.SaveAssets();
        InstallControllerInScene(controller);
        InstallZombiePrefab(zombieController);
        AssetDatabase.SaveAssets();
        Debug.Log("Player sword animations and zombie melee animations were created and connected.");
    }

    private static byte[] ExtractFrame(Color32[] sourcePixels, AnimationRow row, int frameIndex)
    {
        const int sourceWidth = 1377;
        const int sourceHeight = 768;
        const int horizontalInset = 3;
        const int verticalInset = 3;
        const int cellHeight = 131;

        int frameWidth = row.GetFrameWidth(frameIndex);
        int width = frameWidth - horizontalInset * 2;
        int height = cellHeight - verticalInset * 2;
        int sourceX = row.GetFrameStartX(frameIndex) + horizontalInset;
        int sourceTop = row.Top + verticalInset;
        int sourceBottom = sourceHeight - sourceTop - height;
        var pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            Array.Copy(sourcePixels, (sourceBottom + y) * sourceWidth + sourceX, pixels, y * width, width);
        }

        RemoveConnectedWhiteBackground(pixels, width, height);

        for (int y = 0; y < Math.Min(28, height); y++)
        {
            for (int x = Math.Max(0, width - 36); x < width; x++)
            {
                int index = y * width + x;
                pixels[index].a = 0;
            }
        }

        var frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        frame.SetPixels32(pixels);
        frame.Apply(false, false);
        byte[] result = ImageConversion.EncodeToPNG(frame);
        UnityEngine.Object.DestroyImmediate(frame);
        return result;
    }

    private static void RemoveConnectedWhiteBackground(Color32[] pixels, int width, int height)
    {
        var background = new bool[pixels.Length];
        var queue = new int[pixels.Length];
        int head = 0;
        int tail = 0;

        for (int x = 0; x < width; x++)
        {
            AddIfBackground(x);
            AddIfBackground((height - 1) * width + x);
        }

        for (int y = 1; y < height - 1; y++)
        {
            AddIfBackground(y * width);
            AddIfBackground(y * width + width - 1);
        }

        while (head < tail)
        {
            int index = queue[head++];
            int x = index % width;
            int y = index / width;

            if (x > 0) AddIfBackground(index - 1);
            if (x + 1 < width) AddIfBackground(index + 1);
            if (y > 0) AddIfBackground(index - width);
            if (y + 1 < height) AddIfBackground(index + width);
        }

        for (int i = 0; i < pixels.Length; i++)
        {
            if (background[i])
            {
                pixels[i].a = 0;
            }
        }

        void AddIfBackground(int index)
        {
            if (!background[index] && IsBackground(pixels[index]))
            {
                background[index] = true;
                queue[tail++] = index;
            }
        }
    }

    private static bool IsBackground(Color32 color)
    {
        byte minimum = Math.Min(color.r, Math.Min(color.g, color.b));
        byte maximum = Math.Max(color.r, Math.Max(color.g, color.b));
        return minimum >= 220 && maximum - minimum <= 45;
    }

    private static void ConfigureSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 50;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static AnimationClip CreateClip(AnimationRow row, Sprite[] sprites)
    {
        string path = $"{OutputFolder}/{row.Name}.anim";
        AssetDatabase.DeleteAsset(path);

        var clip = new AnimationClip
        {
            name = row.Name,
            frameRate = row.FrameRate,
            wrapMode = row.Loops ? WrapMode.Loop : WrapMode.Once
        };

        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / (float)row.FrameRate,
                value = sprites[i]
            };
        }

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = row.Loops;
        settings.startTime = 0f;
        settings.stopTime = sprites.Length / (float)row.FrameRate;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorController CreateController(Dictionary<string, AnimationClip> clips)
    {
        AssetDatabase.DeleteAsset(ControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.parameters = new[]
        {
            new AnimatorControllerParameter { name = ControllerParameters, type = AnimatorControllerParameterType.Float },
            new AnimatorControllerParameter { name = "Attack", type = AnimatorControllerParameterType.Trigger },
            new AnimatorControllerParameter { name = "Special", type = AnimatorControllerParameterType.Trigger }
        };

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.AddState("Idle");
        AnimatorState walk = machine.AddState("Walk");
        AnimatorState attack = machine.AddState("Attack");
        AnimatorState special = machine.AddState("Special");
        machine.defaultState = idle;
        idle.motion = clips["Idle"];
        walk.motion = clips["Walk"];
        attack.motion = clips["Attack"];
        special.motion = clips["Special"];

        AnimatorStateTransition idleToWalk = idle.AddTransition(walk);
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.08f;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, ControllerParameters);

        AnimatorStateTransition walkToIdle = walk.AddTransition(idle);
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.08f;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, ControllerParameters);

        AddActionTransition(machine, attack, "Attack");
        AddActionTransition(machine, special, "Special");
        AddReturnTransition(attack, idle);
        AddReturnTransition(special, idle);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void AddActionTransition(AnimatorStateMachine machine, AnimatorState state, string trigger)
    {
        AnimatorStateTransition transition = machine.AddAnyStateTransition(state);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    private static void AddReturnTransition(AnimatorState action, AnimatorState idle)
    {
        AnimatorStateTransition transition = action.AddTransition(idle);
        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = 0.04f;
    }

    private static AnimatorController CreateZombieController()
    {
        const string zombieAnimationFolder = OutputFolder + "/Zombie";
        EnsureFolder(zombieAnimationFolder);
        AssetDatabase.DeleteAsset(ZombieControllerPath);

        AnimationClip idleClip = CreateTransformClip(
            zombieAnimationFolder + "/Zombie_Idle.anim",
            0.8f,
            true,
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 0.035f),
                new Keyframe(0.4f, 0f),
                new Keyframe(0.6f, -0.02f),
                new Keyframe(0.8f, 0f)
            },
            new[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.4f, 1.06f),
                new Keyframe(0.8f, 1f)
            },
            new[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.4f, 0.95f),
                new Keyframe(0.8f, 1f)
            },
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 2f),
                new Keyframe(0.4f, 0f),
                new Keyframe(0.6f, -2f),
                new Keyframe(0.8f, 0f)
            });

        AnimationClip walkClip = CreateTransformClip(
            zombieAnimationFolder + "/Zombie_Walk.anim",
            0.48f,
            true,
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.12f, 0.16f),
                new Keyframe(0.24f, 0f),
                new Keyframe(0.36f, 0.14f),
                new Keyframe(0.48f, 0f)
            },
            new[]
            {
                new Keyframe(0f, 0.88f),
                new Keyframe(0.12f, 1.12f),
                new Keyframe(0.24f, 0.88f),
                new Keyframe(0.36f, 1.12f),
                new Keyframe(0.48f, 0.88f)
            },
            new[]
            {
                new Keyframe(0f, 1.12f),
                new Keyframe(0.12f, 0.88f),
                new Keyframe(0.24f, 1.12f),
                new Keyframe(0.36f, 0.88f),
                new Keyframe(0.48f, 1.12f)
            },
            new[]
            {
                new Keyframe(0f, -8f),
                new Keyframe(0.12f, 8f),
                new Keyframe(0.24f, -8f),
                new Keyframe(0.36f, 8f),
                new Keyframe(0.48f, -8f)
            });

        AnimationClip attackClip = CreateTransformClip(
            zombieAnimationFolder + "/Zombie_Attack.anim",
            0.5f,
            false,
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.12f, -0.035f),
                new Keyframe(0.24f, 0.06f),
                new Keyframe(0.36f, -0.015f),
                new Keyframe(0.5f, 0f)
            },
            new[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.12f, 0.9f),
                new Keyframe(0.24f, 1.14f),
                new Keyframe(0.36f, 0.96f),
                new Keyframe(0.5f, 1f)
            },
            new[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.12f, 1.08f),
                new Keyframe(0.24f, 0.9f),
                new Keyframe(0.36f, 1.04f),
                new Keyframe(0.5f, 1f)
            },
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.12f, -12f),
                new Keyframe(0.24f, 14f),
                new Keyframe(0.36f, -5f),
                new Keyframe(0.5f, 0f)
            });

        AnimationClip hitClip = CreateTransformClip(
            zombieAnimationFolder + "/Zombie_Hit.anim",
            0.18f,
            false,
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.045f, 0.045f),
                new Keyframe(0.09f, -0.045f),
                new Keyframe(0.135f, 0.025f),
                new Keyframe(0.18f, 0f)
            },
            new[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.045f, 1.08f),
                new Keyframe(0.09f, 0.95f),
                new Keyframe(0.18f, 1f)
            },
            new[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.045f, -10f),
                new Keyframe(0.09f, 10f),
                new Keyframe(0.135f, -4f),
                new Keyframe(0.18f, 0f)
            },
            new[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.045f, 0.94f),
                new Keyframe(0.09f, 1.05f),
                new Keyframe(0.18f, 1f)
            });

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ZombieControllerPath);
        controller.parameters = new[]
        {
            new AnimatorControllerParameter { name = ControllerParameters, type = AnimatorControllerParameterType.Float },
            new AnimatorControllerParameter { name = "Attack", type = AnimatorControllerParameterType.Trigger },
            new AnimatorControllerParameter { name = "Hit", type = AnimatorControllerParameterType.Trigger }
        };

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.AddState("Idle");
        AnimatorState walk = machine.AddState("Walk");
        AnimatorState attack = machine.AddState("Attack");
        AnimatorState hit = machine.AddState("Hit");
        machine.defaultState = idle;
        idle.motion = idleClip;
        walk.motion = walkClip;
        attack.motion = attackClip;
        hit.motion = hitClip;

        AnimatorStateTransition idleToWalk = idle.AddTransition(walk);
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, ControllerParameters);

        AnimatorStateTransition walkToIdle = walk.AddTransition(idle);
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, ControllerParameters);

        AddActionTransition(machine, attack, "Attack");
        AddActionTransition(machine, hit, "Hit");
        AddReturnTransition(attack, idle);
        AddReturnTransition(hit, idle);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimationClip CreateTransformClip(
        string path,
        float duration,
        bool loops,
        Keyframe[] positionYKeys,
        Keyframe[] scaleXKeys,
        Keyframe[] scaleYKeys,
        Keyframe[] rotationZKeys)
    {
        AssetDatabase.DeleteAsset(path);
        var clip = new AnimationClip
        {
            name = Path.GetFileNameWithoutExtension(path),
            frameRate = 24f,
            wrapMode = loops ? WrapMode.Loop : WrapMode.Once
        };
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("Visual", typeof(Transform), "m_LocalPosition.y"),
            new AnimationCurve(positionYKeys));
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("Visual", typeof(Transform), "m_LocalScale.x"),
            new AnimationCurve(scaleXKeys));
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("Visual", typeof(Transform), "m_LocalScale.y"),
            new AnimationCurve(scaleYKeys));
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("Visual", typeof(Transform), "localEulerAnglesRaw.z"),
            new AnimationCurve(rotationZKeys));
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loops;
        settings.startTime = 0f;
        settings.stopTime = duration;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void InstallZombiePrefab(AnimatorController controller)
    {
        if (!File.Exists(ZombiePrefabPath))
        {
            throw new FileNotFoundException("Zombie prefab was not found.", ZombiePrefabPath);
        }

        GameObject root = PrefabUtility.LoadPrefabContents(ZombiePrefabPath);
        try
        {
            RangedEnemy enemy = root.GetComponent<RangedEnemy>();
            SpriteRenderer currentRenderer = root.GetComponent<SpriteRenderer>();
            Transform visual = root.transform.Find("Visual");
            if (enemy == null || (currentRenderer == null && visual == null))
            {
                throw new InvalidDataException("Zombie prefab needs its RangedEnemy and SpriteRenderer components.");
            }

            if (visual == null)
            {
                var visualObject = new GameObject("Visual");
                visual = visualObject.transform;
                visual.SetParent(root.transform, false);
                SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
                EditorUtility.CopySerialized(currentRenderer, visualRenderer);
                UnityEngine.Object.DestroyImmediate(currentRenderer);
            }
            else if (visual.GetComponent<SpriteRenderer>() == null)
            {
                throw new InvalidDataException("Zombie prefab's Visual child has no SpriteRenderer.");
            }

            Animator animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                animator = root.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var serializedEnemy = new SerializedObject(enemy);
            SetFloat(serializedEnemy, "speed", 2.5f);
            SetFloat(serializedEnemy, "attackRange", 1.8f);
            SetFloat(serializedEnemy, "attackCooldown", 1.1f);
            SetInt(serializedEnemy, "damage", 12);
            SetFloat(serializedEnemy, "hitDelay", 0.22f);
            SetInt(serializedEnemy, "maxHealth", 100);
            serializedEnemy.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, ZombiePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            throw new MissingFieldException($"Could not find serialized float field '{propertyName}'.");
        }

        property.floatValue = value;
    }

    private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            throw new MissingFieldException($"Could not find serialized integer field '{propertyName}'.");
        }

        property.intValue = value;
    }

    private static void InstallControllerInScene(AnimatorController controller)
    {
        if (!File.Exists(GameScenePath))
        {
            throw new FileNotFoundException("The enabled game scene was not found.", GameScenePath);
        }

        string scene = File.ReadAllText(GameScenePath);
        string[] blocks = Regex.Split(scene, @"(?=^--- !u!)", RegexOptions.Multiline);
        int playerGameObjectId = 0;
        int playerBlockIndex = -1;

        for (int i = 0; i < blocks.Length; i++)
        {
            Match match = Regex.Match(blocks[i], @"^--- !u!1 &(\d+)", RegexOptions.Multiline);
            if (match.Success && Regex.IsMatch(blocks[i], @"(?m)^  m_Name: Player\s*$"))
            {
                playerGameObjectId = int.Parse(match.Groups[1].Value);
                playerBlockIndex = i;
                break;
            }
        }

        if (playerBlockIndex < 0)
        {
            throw new InvalidDataException("Could not find the Player object in the enabled game scene.");
        }

        long nextFileId = 900000000;
        while (Regex.IsMatch(scene, $@"&{nextFileId}(\s|$)"))
        {
            nextFileId++;
        }

        bool hasAnimator = false;
        int animatorBlockIndex = -1;
        for (int i = 0; i < blocks.Length; i++)
        {
            if (Regex.IsMatch(blocks[i], @"^--- !u!95 &-?\d+", RegexOptions.Multiline) &&
                blocks[i].Contains($"m_GameObject: {{fileID: {playerGameObjectId}}}"))
            {
                hasAnimator = true;
                animatorBlockIndex = i;
                break;
            }
        }

        string newline = scene.Contains("\r\n") ? "\r\n" : "\n";
        string controllerGuid = AssetDatabase.AssetPathToGUID(ControllerPath);
        if (hasAnimator)
        {
            string animatorBlock = blocks[animatorBlockIndex];
            Match animatorIdMatch = Regex.Match(animatorBlock, @"^--- !u!95 &(-?\d+)", RegexOptions.Multiline);
            long oldFileId = long.Parse(animatorIdMatch.Groups[1].Value);
            blocks[playerBlockIndex] = blocks[playerBlockIndex]
                .Replace($"fileID: {oldFileId}", $"fileID: {nextFileId}");
            animatorBlock = Regex.Replace(animatorBlock, @"(?m)(^--- !u!95 &)-?\d+", $"${{1}}{nextFileId}");
            animatorBlock = Regex.Replace(
                animatorBlock,
                @"(?m)^  m_Controller:.*$",
                $"  m_Controller: {{fileID: 9100000, guid: {controllerGuid}, type: 2}}");
            blocks[animatorBlockIndex] = animatorBlock;
            scene = string.Concat(blocks);
        }
        else
        {
            string playerBlock = blocks[playerBlockIndex];
            playerBlock = Regex.Replace(
                playerBlock,
                @"(?m)(^  m_Layer: \d+\s*$)",
                $"  - component: {{fileID: {nextFileId}}}{newline}$1",
                RegexOptions.None,
                TimeSpan.FromSeconds(1));
            blocks[playerBlockIndex] = playerBlock;
            scene = string.Concat(blocks);
            scene +=
                $"{newline}--- !u!95 &{nextFileId}{newline}" +
                $"Animator:{newline}" +
                $"  serializedVersion: 5{newline}" +
                $"  m_ObjectHideFlags: 0{newline}" +
                $"  m_CorrespondingSourceObject: {{fileID: 0}}{newline}" +
                $"  m_PrefabInstance: {{fileID: 0}}{newline}" +
                $"  m_PrefabAsset: {{fileID: 0}}{newline}" +
                $"  m_GameObject: {{fileID: {playerGameObjectId}}}{newline}" +
                $"  m_Enabled: 1{newline}" +
                $"  m_Avatar: {{fileID: 0}}{newline}" +
                $"  m_Controller: {{fileID: 9100000, guid: {controllerGuid}, type: 2}}{newline}" +
                $"  m_CullingMode: 0{newline}" +
                $"  m_UpdateMode: 0{newline}" +
                $"  m_ApplyRootMotion: 0{newline}" +
                $"  m_LinearVelocityBlending: 0{newline}" +
                $"  m_StabilizeFeet: 0{newline}" +
                $"  m_WarningMessage:{newline}" +
                $"  m_HasTransformHierarchy: 1{newline}" +
                $"  m_AllowConstantClipSamplingOptimization: 1{newline}";
        }

        File.WriteAllText(GameScenePath, scene, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(GameScenePath, ImportAssetOptions.ForceUpdate);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
