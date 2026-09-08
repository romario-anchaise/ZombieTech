using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ZombieBackgroundInstaller
{
    private const string BackgroundAssetPath = "Assets/Art/Backgrounds/ZombieCityBackground.png";
    private const string BackgroundObjectName = "Background_ZombieCity";

    static ZombieBackgroundInstaller()
    {
        EditorApplication.delayCall += InstallOnceWhenReady;
    }

    [MenuItem("Tools/Zombie Game/Agregar fondo a la escena")]
    public static void AddBackgroundToActiveScene()
    {
        ConfigureTextureImport();

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundAssetPath);
        Camera sceneCamera = Camera.main;

        if (backgroundSprite == null || sceneCamera == null)
        {
            Debug.LogError("No se pudo agregar el fondo: falta el sprite o una camara con el tag MainCamera.");
            return;
        }

        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
            environment = new GameObject("Environment");

        Transform existingBackground = environment.transform.Find(BackgroundObjectName);
        GameObject background = existingBackground != null
            ? existingBackground.gameObject
            : new GameObject(BackgroundObjectName);

        background.transform.SetParent(environment.transform, false);
        background.transform.position = new Vector3(
            sceneCamera.transform.position.x,
            sceneCamera.transform.position.y,
            0f);

        SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = background.AddComponent<SpriteRenderer>();

        renderer.sprite = backgroundSprite;
        renderer.color = Color.white;
        renderer.sortingOrder = -100;

        float cameraHeight = sceneCamera.orthographicSize * 2f;
        float scale = cameraHeight / backgroundSprite.bounds.size.y;
        background.transform.localScale = new Vector3(scale, scale, 1f);

        EditorSceneManager.MarkSceneDirty(background.scene);
        EditorSceneManager.SaveScene(background.scene);
        Selection.activeGameObject = background;
        EditorGUIUtility.PingObject(background);

        Debug.Log("Fondo zombi agregado y ajustado a la camara en la escena activa.");
    }

    private static void InstallOnceWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || GameObject.Find(BackgroundObjectName) != null)
            return;

        AddBackgroundToActiveScene();
    }

    private static void ConfigureTextureImport()
    {
        TextureImporter importer = AssetImporter.GetAtPath(BackgroundAssetPath) as TextureImporter;
        if (importer == null)
        {
            AssetDatabase.ImportAsset(BackgroundAssetPath, ImportAssetOptions.ForceSynchronousImport);
            importer = AssetImporter.GetAtPath(BackgroundAssetPath) as TextureImporter;
        }

        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();
    }
}
