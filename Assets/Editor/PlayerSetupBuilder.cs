using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

[InitializeOnLoad]
public static class PlayerSetupBuilder
{
    private const string WalkSheetPath = "Assets/Art/Characters/Survivor/Processed/Survivor_Walk.png";
    private const string JumpSheetPath = "Assets/Art/Characters/Survivor/Processed/Survivor_JumpFall.png";
    private const string AnimationFolder = "Assets/Animations/Player";
    private const string MaterialPath = "Assets/Materials/PlayerCheckerboardKey.mat";
    private const string ControllerPath = AnimationFolder + "/PlayerAnimator.controller";

    static PlayerSetupBuilder()
    {
        EditorApplication.delayCall += BuildOnceWhenReady;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += BuildOnceWhenReady;
    }

    [MenuItem("Tools/Zombie Game/Configurar personaje jugable")]
    public static void BuildPlayer()
    {
        ConfigureSpriteSheet(WalkSheetPath, 8, "Walk");
        ConfigureSpriteSheet(JumpSheetPath, 6, "JumpFall");

        Sprite[] walkSprites = LoadSprites(WalkSheetPath);
        Sprite[] jumpSprites = LoadSprites(JumpSheetPath);
        if (walkSprites.Length != 8 || jumpSprites.Length != 6)
        {
            Debug.LogError("No se pudo configurar el jugador: las hojas no contienen 8 y 6 sprites respectivamente.");
            return;
        }

        EnsureFolder("Assets/Animations");
        EnsureFolder(AnimationFolder);
        EnsureFolder("Assets/Materials");

        AnimationClip idleClip = CreateClip(AnimationFolder + "/Player_Idle.anim", new[] { walkSprites[0] }, 1f, true);
        AnimationClip walkClip = CreateClip(AnimationFolder + "/Player_Walk.anim", walkSprites, 10f, true);
        AnimationClip jumpClip = CreateClip(AnimationFolder + "/Player_Jump.anim", jumpSprites.Take(4).ToArray(), 8f, false);
        AnimationClip fallClip = CreateClip(AnimationFolder + "/Player_Fall.anim", jumpSprites.Skip(4).ToArray(), 6f, false);
        AnimatorController controller = CreateAnimatorController(idleClip, walkClip, jumpClip, fallClip);
        Material playerMaterial = CreatePlayerMaterial();

        int groundLayer = EnsureLayer("Ground");
        CreateGround(groundLayer);
        CreatePlayer(walkSprites[0], controller, playerMaterial, groundLayer);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("Personaje jugable configurado: caminar, saltar, caer, girar y colisionar con el suelo.");
    }

    private static void BuildOnceWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (AssetDatabase.LoadAssetAtPath<Texture2D>(WalkSheetPath) == null ||
            AssetDatabase.LoadAssetAtPath<Texture2D>(JumpSheetPath) == null)
            return;

        GameObject player = GameObject.Find("Player");
        SpriteRenderer renderer = player != null ? player.GetComponent<SpriteRenderer>() : null;
        string currentSpritePath = renderer != null && renderer.sprite != null
            ? AssetDatabase.GetAssetPath(renderer.sprite)
            : string.Empty;

        if (!string.Equals(currentSpritePath, WalkSheetPath, StringComparison.Ordinal))
            BuildPlayer();
        else
            AlignExistingPlayer(player);
    }

    private static void ConfigureSpriteSheet(string path, int frameCount, string prefix)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("No se encontro la hoja de sprites: " + path);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 300f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        float frameWidth = texture.width / (float)frameCount;
        var spriteRects = new SpriteRect[frameCount];

        for (int index = 0; index < frameCount; index++)
        {
            spriteRects[index] = new SpriteRect
            {
                name = $"Player_{prefix}_{index:00}",
                rect = new Rect(index * frameWidth, 0f, frameWidth, texture.height),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, 0.18f),
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
                time = index / frameRate,
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

    private static AnimatorController CreateAnimatorController(
        AnimationClip idleClip,
        AnimationClip walkClip,
        AnimationClip jumpClip,
        AnimationClip fallClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        controller.parameters = Array.Empty<AnimatorControllerParameter>();
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states)
            machine.RemoveState(child.state);
        foreach (AnimatorStateTransition transition in machine.anyStateTransitions)
            machine.RemoveAnyStateTransition(transition);

        AnimatorState idle = machine.AddState("Idle", new Vector3(220, 50));
        AnimatorState walk = machine.AddState("Walk", new Vector3(450, 50));
        AnimatorState jump = machine.AddState("Jump", new Vector3(330, -90));
        AnimatorState fall = machine.AddState("Fall", new Vector3(560, -90));
        idle.motion = idleClip;
        walk.motion = walkClip;
        jump.motion = jumpClip;
        fall.motion = fallClip;
        machine.defaultState = idle;

        AddTransition(idle, walk, AnimatorConditionMode.Greater, 0.1f, "Speed");
        AddTransition(walk, idle, AnimatorConditionMode.Less, 0.1f, "Speed");

        AnimatorStateTransition toJump = machine.AddAnyStateTransition(jump);
        ConfigureTransition(toJump);
        toJump.canTransitionToSelf = false;
        toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
        toJump.AddCondition(AnimatorConditionMode.Greater, 0.05f, "VerticalSpeed");

        AnimatorStateTransition toFall = machine.AddAnyStateTransition(fall);
        ConfigureTransition(toFall);
        toFall.canTransitionToSelf = false;
        toFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
        toFall.AddCondition(AnimatorConditionMode.Less, -0.05f, "VerticalSpeed");

        AddGroundedTransition(jump, idle, false);
        AddGroundedTransition(jump, walk, true);
        AddGroundedTransition(fall, idle, false);
        AddGroundedTransition(fall, walk, true);

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void AddTransition(AnimatorState source, AnimatorState destination,
        AnimatorConditionMode mode, float threshold, string parameter)
    {
        AnimatorStateTransition transition = source.AddTransition(destination);
        ConfigureTransition(transition);
        transition.AddCondition(mode, threshold, parameter);
    }

    private static void AddGroundedTransition(AnimatorState source, AnimatorState destination, bool moving)
    {
        AnimatorStateTransition transition = source.AddTransition(destination);
        ConfigureTransition(transition);
        transition.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
        transition.AddCondition(moving ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
            0.1f, "Speed");
    }

    private static void ConfigureTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0.04f;
    }

    private static Material CreatePlayerMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            throw new InvalidOperationException("No se encontro el shader predeterminado de sprites.");

        if (material == null)
        {
            material = new Material(shader) { name = "PlayerSpriteMaterial" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else
        {
            material.shader = shader;
            material.name = "PlayerSpriteMaterial";
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    private static void CreateGround(int groundLayer)
    {
        GameObject ground = GameObject.Find("GroundCollider");
        if (ground == null)
            ground = new GameObject("GroundCollider");

        ground.layer = groundLayer;
        ground.transform.position = new Vector3(0f, -2.52f, 0f);
        BoxCollider2D collider = ground.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = ground.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(18f, 0.35f);
    }

    private static void CreatePlayer(Sprite idleSprite, AnimatorController controller,
        Material material, int groundLayer)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
            player = new GameObject("Player");

        player.transform.position = new Vector3(-4f, -2.38f, 0f);

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = idleSprite;
        renderer.material = material;
        renderer.sortingOrder = 10;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body == null)
            body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3.2f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D collider = player.GetComponent<CapsuleCollider2D>();
        if (collider == null)
            collider = player.AddComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = new Vector2(0.72f, 1.72f);
        collider.offset = new Vector2(0f, 0.9f);

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
            animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        if (player.GetComponent<PlayerController2D>() == null)
            player.AddComponent<PlayerController2D>();

        var controllerComponent = player.GetComponent<PlayerController2D>();
        var serializedController = new SerializedObject(controllerComponent);
        serializedController.FindProperty("groundMask").intValue = 1 << groundLayer;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);
    }

    private static void AlignExistingPlayer(GameObject player)
    {
        if (player == null)
            return;

        CapsuleCollider2D collider = player.GetComponent<CapsuleCollider2D>();
        if (collider == null)
            return;

        Vector2 desiredOffset = new Vector2(0f, 0.9f);
        Vector3 desiredPosition = new Vector3(-4f, -2.38f, 0f);
        if (collider.offset == desiredOffset && player.transform.position == desiredPosition)
            return;

        collider.offset = desiredOffset;
        player.transform.position = desiredPosition;
        EditorUtility.SetDirty(collider);
        EditorUtility.SetDirty(player.transform);
        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Debug.Log("Alineacion del jugador corregida: los pies coinciden con el suelo.");
    }

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0)
            return existing;

        UnityEngine.Object tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        var tagManager = new SerializedObject(tagManagerAsset);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int index = 8; index < layers.arraySize; index++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(layer.stringValue))
                continue;

            layer.stringValue = layerName;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            return index;
        }

        throw new InvalidOperationException("No hay una capa libre para Ground.");
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
