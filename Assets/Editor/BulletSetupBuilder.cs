using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

[InitializeOnLoad]
public static class BulletSetupBuilder
{
    private const string SpriteSheetPath = "Assets/Art/Projectiles/Bullet_Flight.png";
    private const string AnimationFolder = "Assets/Animations/Projectiles";
    private const string AnimationPath = AnimationFolder + "/Bullet_Flight.anim";
    private const string ControllerPath = AnimationFolder + "/Bullet.controller";
    private const string MaterialPath = "Assets/Materials/BulletAdditive.mat";
    private const string PrefabPath = "Assets/Prefabs/Bullet.prefab";

    private static bool isBuilding;

    static BulletSetupBuilder()
    {
        EditorApplication.delayCall += BuildOnceWhenReady;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    [MenuItem("Tools/Zombie Game/Configurar balas")]
    public static void BuildBullet()
    {
        if (isBuilding || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        isBuilding = true;
        try
        {
            ConfigureSpriteSheet();
            Sprite[] sprites = LoadSprites();
            if (sprites.Length != 4)
            {
                Debug.LogError("No se pudo configurar la bala: la hoja debe contener cuatro sprites.");
                return;
            }

            EnsureFolder("Assets/Animations");
            EnsureFolder(AnimationFolder);
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Prefabs");

            Material material = CreateMaterial();
            AnimationClip clip = CreateClip(sprites);
            AnimatorController controller = CreateController(clip);
            GameObject prefab = CreatePrefab(sprites[0], material, controller);
            AssignBulletToPlayer(prefab);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Balas configuradas: el jugador dispara proyectiles que causan dano al zombie.");
        }
        finally
        {
            isBuilding = false;
        }
    }

    private static void BuildOnceWhenReady()
    {
        if (isBuilding || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteSheetPath) == null)
            return;

        GameObject player = GameObject.Find("Player");
        PlayerController2D playerController = player != null
            ? player.GetComponent<PlayerController2D>()
            : null;
        bool assignmentMissing = true;
        if (playerController != null)
        {
            var serializedPlayer = new SerializedObject(playerController);
            SerializedProperty bulletProperty = serializedPlayer.FindProperty("bulletPrefab");
            assignmentMissing = bulletProperty == null || bulletProperty.objectReferenceValue == null;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null || assignmentMissing)
            BuildBullet();
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += BuildOnceWhenReady;
    }

    private static void ConfigureSpriteSheet()
    {
        TextureImporter importer = AssetImporter.GetAtPath(SpriteSheetPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("No se encontro la hoja de sprites de la bala.");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 700f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteSheetPath);
        const int frameCount = 4;
        float frameWidth = texture.width / (float)frameCount;
        var spriteRects = new SpriteRect[frameCount];
        for (int index = 0; index < frameCount; index++)
        {
            spriteRects[index] = new SpriteRect
            {
                name = $"Bullet_Flight_{index:00}",
                rect = new Rect(index * frameWidth, 0f, frameWidth, texture.height),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
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

    private static Sprite[] LoadSprites()
    {
        return AssetDatabase.LoadAllAssetsAtPath(SpriteSheetPath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();
    }

    private static Material CreateMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Shader shader = Shader.Find("ZombieGame/AdditiveProjectile");
        if (shader == null)
            throw new InvalidOperationException("No se encontro el shader aditivo de la bala.");

        if (material == null)
        {
            material = new Material(shader) { name = "BulletAdditiveMaterial" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else
        {
            material.shader = shader;
            material.name = "BulletAdditiveMaterial";
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    private static AnimationClip CreateClip(Sprite[] sprites)
    {
        var generated = new AnimationClip { frameRate = 14f };
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
        serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = true;
        serializedClip.ApplyModifiedPropertiesWithoutUndo();

        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimationPath);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(generated, AnimationPath);
            return generated;
        }

        EditorUtility.CopySerialized(generated, existing);
        UnityEngine.Object.DestroyImmediate(generated);
        EditorUtility.SetDirty(existing);
        return existing;
    }

    private static AnimatorController CreateController(AnimationClip clip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states)
            machine.RemoveState(child.state);

        AnimatorState flight = machine.AddState("Flight", new Vector3(300f, 80f));
        flight.motion = clip;
        machine.defaultState = flight;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static GameObject CreatePrefab(Sprite sprite, Material material,
        AnimatorController controller)
    {
        var root = new GameObject("Bullet");

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = 20;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.5f, 0.15f);

        Animator animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        root.AddComponent<BulletProjectile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static void AssignBulletToPlayer(GameObject prefab)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
            throw new InvalidOperationException("No se encontro el objeto Player.");

        PlayerController2D controller = player.GetComponent<PlayerController2D>();
        if (controller == null)
            throw new InvalidOperationException("Player no tiene PlayerController2D.");

        var serializedPlayer = new SerializedObject(controller);
        serializedPlayer.FindProperty("bulletPrefab").objectReferenceValue = prefab;
        serializedPlayer.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
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
