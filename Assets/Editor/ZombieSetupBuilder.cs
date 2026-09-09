using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

[InitializeOnLoad]
// Builds the reusable enemy prefab after Unity finishes importing its animation sheets.
public static class ZombieSetupBuilder
{
    private const string WalkSheetPath = "Assets/Art/Characters/Zombies/WorkerZombie_Walk.png";
    private const string AttackSheetPath = "Assets/Art/Characters/Zombies/WorkerZombie_Attack.png";
    private const string DeathSheetPath = "Assets/Art/Characters/Zombies/WorkerZombie_Death.png";
    private const string EatSheetPath = "Assets/Art/Characters/Zombies/WorkerZombie_Eat.png";
    private const string AnimationFolder = "Assets/Animations/Zombie";
    private const string WalkAnimationPath = AnimationFolder + "/WorkerZombie_Walk.anim";
    private const string AttackAnimationPath = AnimationFolder + "/WorkerZombie_Attack.anim";
    private const string DeathAnimationPath = AnimationFolder + "/WorkerZombie_Death.anim";
    private const string EatAnimationPath = AnimationFolder + "/WorkerZombie_Eat.anim";
    private const string ControllerPath = AnimationFolder + "/WorkerZombie.controller";
    private const string MaterialPath = "Assets/Materials/WorkerZombieCheckerboardKey.mat";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PrefabPath = PrefabFolder + "/WorkerZombie.prefab";
    private const string InstanceName = "Worker Zombie";
    private static readonly string[] ZombieSoundPaths =
    {
        "Assets/Audio/Zombie/Zombie_01.mp3",
        "Assets/Audio/Zombie/Zombie_02.mp3",
        "Assets/Audio/Zombie/Zombie_03.mp3",
        "Assets/Audio/Zombie/Zombie_04.mp3",
        "Assets/Audio/Zombie/Zombie_06.mp3",
        "Assets/Audio/Zombie/Zombie_07.mp3"
    };

    private static bool isBuilding;

    static ZombieSetupBuilder()
    {
        EditorApplication.delayCall += BuildOnceWhenReady;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    [MenuItem("Tools/Zombie Game/Agregar un zombie")]
    public static void BuildZombie()
    {
        if (isBuilding || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        isBuilding = true;
        try
        {
            ConfigureSpriteSheet(WalkSheetPath, 8, "Walk");
            ConfigureSpriteSheet(AttackSheetPath, 6, "Attack");
            ConfigureSpriteSheet(DeathSheetPath, 6, "Death");
            ConfigureSpriteSheet(EatSheetPath, 6, "Eat");
            Sprite[] walkSprites = LoadSprites(WalkSheetPath);
            Sprite[] attackSprites = LoadSprites(AttackSheetPath);
            Sprite[] deathSprites = LoadSprites(DeathSheetPath);
            Sprite[] eatSprites = LoadSprites(EatSheetPath);
            AudioClip[] zombieSounds = ZombieSoundPaths
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .ToArray();
            if (walkSprites.Length != 8 || attackSprites.Length != 6 || deathSprites.Length != 6 ||
                eatSprites.Length != 6 ||
                zombieSounds.Any(clip => clip == null))
            {
                Debug.LogError("No se pudo configurar el zombie: las hojas no contienen los frames esperados.");
                return;
            }

            EnsureFolder("Assets/Animations");
            EnsureFolder(AnimationFolder);
            EnsureFolder("Assets/Materials");
            EnsureFolder(PrefabFolder);

            Material material = CreateMaterial();
            AnimationClip walkClip = CreateClip(WalkAnimationPath, walkSprites, 9f, true);
            AnimationClip attackClip = CreateClip(AttackAnimationPath, attackSprites, 10f, false);
            AnimationClip deathClip = CreateClip(DeathAnimationPath, deathSprites, 8f, false);
            AnimationClip eatClip = CreateClip(EatAnimationPath, eatSprites, 7f, true);
            AnimatorController controller = CreateAnimatorController(walkClip, attackClip, deathClip, eatClip);
            GameObject prefab = CreatePrefab(walkSprites[0], material, controller, zombieSounds);
            PlaceSingleInstance(prefab);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Zombie actualizado: camina, persigue y reproduce su ataque antes de alcanzar al jugador.");
        }
        finally
        {
            isBuilding = false;
        }
    }

    private static void BuildOnceWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || isBuilding)
            return;

        if (AssetDatabase.LoadAssetAtPath<Texture2D>(WalkSheetPath) == null ||
            AssetDatabase.LoadAssetAtPath<Texture2D>(AttackSheetPath) == null ||
            AssetDatabase.LoadAssetAtPath<Texture2D>(DeathSheetPath) == null ||
            AssetDatabase.LoadAssetAtPath<Texture2D>(EatSheetPath) == null ||
            ZombieSoundPaths.Any(path => AssetDatabase.LoadAssetAtPath<AudioClip>(path) == null))
            return;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        bool healthMissing = prefab == null || prefab.GetComponent<ZombieHealth>() == null;
        bool audioMissing = prefab == null || prefab.GetComponent<ZombieAudio>() == null;
        bool pivotsNeedUpdate = !HasExpectedPivots(EatSheetPath, new[]
        {
            0.2314f, 0.2314f, 0.2314f, 0.2314f, 0.2314f, 0.2314f
        }) || !HasExpectedPivots(DeathSheetPath, new[]
        {
            0.1804f, 0.1804f, 0.1804f, 0.1765f, 0.1582f, 0.1582f
        });

        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(AttackAnimationPath) == null ||
            AssetDatabase.LoadAssetAtPath<AnimationClip>(DeathAnimationPath) == null ||
            AssetDatabase.LoadAssetAtPath<AnimationClip>(EatAnimationPath) == null ||
            healthMissing ||
            audioMissing ||
            pivotsNeedUpdate ||
            GameObject.Find(InstanceName) == null)
            BuildZombie();
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += BuildOnceWhenReady;
    }

    private static void ConfigureSpriteSheet(string path, int frameCount, string animationName)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("No se encontro la hoja de sprites del zombie.");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 300f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        float frameWidth = texture.width / (float)frameCount;
        var spriteRects = new SpriteRect[frameCount];

        for (int index = 0; index < frameCount; index++)
        {
            float pivotY = 0.18f;
            if (animationName == "Death")
            {
                float[] deathPivotY = { 0.1804f, 0.1804f, 0.1804f, 0.1765f, 0.1582f, 0.1582f };
                pivotY = deathPivotY[index];
            }
            else if (animationName == "Eat")
            {
                pivotY = 0.2314f;
            }

            spriteRects[index] = new SpriteRect
            {
                name = $"WorkerZombie_{animationName}_{index:00}",
                rect = new Rect(index * frameWidth, 0f, frameWidth, texture.height),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, pivotY),
                spriteID = GUID.Generate()
            };
        }

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        provider.SetSpriteRects(spriteRects);

        ISpriteNameFileIdDataProvider nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider.SetNameFileIdPairs(spriteRects.Select(
            spriteRect => new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static Sprite[] LoadSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool HasExpectedPivots(string path, float[] expectedPivotY)
    {
        Sprite[] sprites = LoadSprites(path);
        if (sprites.Length != expectedPivotY.Length)
            return false;

        for (int index = 0; index < sprites.Length; index++)
        {
            float normalizedPivotY = sprites[index].pivot.y / sprites[index].rect.height;
            if (!Mathf.Approximately(normalizedPivotY, expectedPivotY[index]))
                return false;
        }

        return true;
    }

    private static Material CreateMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Shader shader = Shader.Find("ZombieGame/CheckerboardKey");
        if (shader == null)
            throw new InvalidOperationException("No se encontro el shader para limpiar el fondo del zombie.");

        if (material == null)
        {
            material = new Material(shader) { name = "WorkerZombieMaterial" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else
        {
            material.shader = shader;
            material.name = "WorkerZombieMaterial";
        }

        material.SetFloat("_Cutoff", 0.55f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static AnimationClip CreateClip(string path, Sprite[] sprites, float frameRate, bool loop)
    {
        var generated = new AnimationClip { frameRate = frameRate };
        var binding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };
        var frames = new ObjectReferenceKeyframe[sprites.Length];
        for (int index = 0; index < sprites.Length; index++)
        {
            frames[index] = new ObjectReferenceKeyframe
            {
                time = index / generated.frameRate,
                value = sprites[index]
            };
        }
        AnimationUtility.SetObjectReferenceCurve(generated, binding, frames);

        var serializedClip = new SerializedObject(generated);
        serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = loop;
        serializedClip.ApplyModifiedPropertiesWithoutUndo();

        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(generated, path);
            return generated;
        }

        EditorUtility.CopySerialized(generated, existing);
        UnityEngine.Object.DestroyImmediate(generated);
        EditorUtility.SetDirty(existing);
        return existing;
    }

    private static AnimatorController CreateAnimatorController(AnimationClip walkClip, AnimationClip attackClip,
        AnimationClip deathClip, AnimationClip eatClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        controller.parameters = Array.Empty<AnimatorControllerParameter>();
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Eat", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states)
            machine.RemoveState(child.state);
        foreach (AnimatorStateTransition transition in machine.anyStateTransitions)
            machine.RemoveAnyStateTransition(transition);

        AnimatorState walkState = machine.AddState("Walk", new Vector3(300f, 80f));
        AnimatorState attackState = machine.AddState("Attack", new Vector3(540f, 80f));
        AnimatorState deathState = machine.AddState("Death", new Vector3(540f, 210f));
        AnimatorState eatState = machine.AddState("Eat", new Vector3(780f, 80f));
        walkState.motion = walkClip;
        attackState.motion = attackClip;
        deathState.motion = deathClip;
        eatState.motion = eatClip;
        machine.defaultState = walkState;

        AnimatorStateTransition toAttack = walkState.AddTransition(attackState);
        toAttack.hasExitTime = false;
        toAttack.hasFixedDuration = true;
        toAttack.duration = 0.04f;
        toAttack.canTransitionToSelf = false;
        toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        AnimatorStateTransition toWalk = attackState.AddTransition(walkState);
        toWalk.hasExitTime = true;
        toWalk.exitTime = 0.95f;
        toWalk.hasFixedDuration = true;
        toWalk.duration = 0.04f;

        AnimatorStateTransition toDeath = machine.AddAnyStateTransition(deathState);
        toDeath.hasExitTime = false;
        toDeath.hasFixedDuration = true;
        toDeath.duration = 0.03f;
        toDeath.canTransitionToSelf = false;
        toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Die");

        AnimatorStateTransition toEat = machine.AddAnyStateTransition(eatState);
        toEat.hasExitTime = false;
        toEat.hasFixedDuration = true;
        toEat.duration = 0.04f;
        toEat.canTransitionToSelf = false;
        toEat.AddCondition(AnimatorConditionMode.If, 0f, "Eat");
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static GameObject CreatePrefab(Sprite idleSprite, Material material,
        AnimatorController controller, AudioClip[] zombieSounds)
    {
        var root = new GameObject(InstanceName);

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = idleSprite;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = 9;
        renderer.flipX = true;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 3.2f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = new Vector2(0.72f, 1.72f);
        collider.offset = new Vector2(0f, 0.86f);

        Animator animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 0.65f;
        audioSource.spatialBlend = 0.2f;
        ZombieAudio zombieAudio = root.AddComponent<ZombieAudio>();
        zombieAudio.Configure(
            new[] { zombieSounds[0], zombieSounds[1] },
            new[] { zombieSounds[2], zombieSounds[3] },
            zombieSounds[4],
            zombieSounds[5]);
        root.AddComponent<ZombiePatrol>();
        root.AddComponent<ZombieHealth>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static void PlaceSingleInstance(GameObject prefab)
    {
        GameObject existing = GameObject.Find(InstanceName);
        if (existing != null)
            UnityEngine.Object.DestroyImmediate(existing);

        GameObject enemies = GameObject.Find("Enemies");
        if (enemies == null)
            enemies = new GameObject("Enemies");

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
            throw new InvalidOperationException("No se pudo crear la instancia del zombie.");

        instance.name = InstanceName;
        instance.transform.SetParent(enemies.transform, false);
        instance.transform.position = new Vector3(5.25f, -2.38f, 0f);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = path.Substring(0, path.LastIndexOf('/'));
        string folder = path.Substring(path.LastIndexOf('/') + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
