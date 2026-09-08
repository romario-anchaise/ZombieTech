using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EnvironmentPlatformInstaller
{
    private const string PlatformRootName = "Jumpable Environment";

    static EnvironmentPlatformInstaller()
    {
        EditorApplication.delayCall += InstallWhenReady;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    [MenuItem("Tools/Zombie Game/Configurar objetos saltables")]
    public static void InstallPlatforms()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
        {
            Debug.LogError("No se encontro la capa Ground para los objetos saltables.");
            return;
        }

        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
            environment = new GameObject("Environment");

        Transform obsoletePlatforms = environment.transform.Find("Deprecated Jump Platforms");
        if (obsoletePlatforms != null)
            Object.DestroyImmediate(obsoletePlatforms.gameObject);

        Transform existingRoot = environment.transform.Find(PlatformRootName);
        GameObject platformRoot = existingRoot != null
            ? existingRoot.gameObject
            : new GameObject(PlatformRootName);

        platformRoot.transform.SetParent(environment.transform, false);

        // These invisible colliders follow objects that are already painted into
        // the background, so they feel like part of the ruined city.
        CreatePlatform(platformRoot.transform, "Wrecked Car", new Vector2(-5f, -1.15f), new Vector2(2.2f, 0.3f), groundLayer);
        CreatePlatform(platformRoot.transform, "Concrete Barrier", new Vector2(0.55f, -1.6f), new Vector2(3f, 0.3f), groundLayer);

        EditorSceneManager.MarkSceneDirty(platformRoot.scene);
        EditorSceneManager.SaveScene(platformRoot.scene);
        Debug.Log("Objetos saltables configurados: automovil destruido y barrera de concreto.");
    }

    private static void InstallWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        InstallPlatforms();
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += InstallWhenReady;
    }

    private static void CreatePlatform(Transform parent, string objectName, Vector2 position,
        Vector2 size, int groundLayer)
    {
        Transform existing = parent.Find(objectName);
        GameObject platform = existing != null ? existing.gameObject : new GameObject(objectName);

        platform.transform.SetParent(parent, false);
        platform.transform.position = new Vector3(position.x, position.y, 0f);
        platform.layer = groundLayer;

        BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = platform.AddComponent<BoxCollider2D>();

        collider.isTrigger = false;
        collider.usedByEffector = true;
        collider.size = size;
        collider.offset = Vector2.zero;

        PlatformEffector2D effector = platform.GetComponent<PlatformEffector2D>();
        if (effector == null)
            effector = platform.AddComponent<PlatformEffector2D>();

        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.useSideFriction = false;
        effector.useSideBounce = false;
        effector.surfaceArc = 160f;
    }
}
