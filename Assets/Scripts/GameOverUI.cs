using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameOverUI : MonoBehaviour
{
    private static GameOverUI instance;

    private CanvasGroup canvasGroup;
    private GameObject panel;
    private Coroutine showRoutine;
    private AudioSource backgroundMusic;
    private float originalMusicVolume = 0.35f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (instance != null)
            return;

        var root = new GameObject("Game Over UI");
        instance = root.AddComponent<GameOverUI>();
        DontDestroyOnLoad(root);
    }

    public static void ShowAfterDelay(float delay)
    {
        if (instance == null)
            CreateInstance();

        if (instance.showRoutine == null)
            instance.showRoutine = instance.StartCoroutine(instance.ShowSequence(delay));
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildInterface();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            instance = null;
        }
    }

    private IEnumerator ShowSequence(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        panel.SetActive(true);
        backgroundMusic = GameObject.Find("Background Music")?.GetComponent<AudioSource>();
        if (backgroundMusic != null)
            originalMusicVolume = backgroundMusic.volume;

        const float fadeDuration = 0.7f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = progress;
            if (backgroundMusic != null)
                backgroundMusic.volume = Mathf.Lerp(originalMusicVolume, originalMusicVolume * 0.25f, progress);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Time.timeScale = 0f;
        showRoutine = null;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        if (backgroundMusic != null)
            backgroundMusic.volume = originalMusicVolume;
        HideImmediately();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        HideImmediately();
    }

    private void HideImmediately()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (panel != null)
            panel.SetActive(false);
    }

    private void BuildInterface()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        panel = CreateUiObject("Dark Overlay", transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        StretchToParent(panelRect);
        Image overlay = panel.AddComponent<Image>();
        overlay.color = new Color(0.035f, 0.02f, 0.025f, 0.92f);
        canvasGroup = panel.AddComponent<CanvasGroup>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreateText(panel.transform, "GAME OVER", font, 82, new Vector2(0f, 105f),
            new Color(0.78f, 0.08f, 0.07f), FontStyle.Bold);
        CreateText(panel.transform, "EL BROTE TE CONSUMIO", font, 25, new Vector2(0f, 34f),
            new Color(0.82f, 0.76f, 0.68f), FontStyle.Normal);

        CreateButton(panel.transform, "REINTENTAR", font, new Vector2(0f, -55f), RestartGame);
        CreateButton(panel.transform, "SALIR", font, new Vector2(0f, -125f), QuitGame);

        EnsureEventSystem();
        HideImmediately();
    }

    private static void CreateText(Transform parent, string value, Font font, int fontSize,
        Vector2 position, Color color, FontStyle style)
    {
        GameObject textObject = CreateUiObject(value, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 100f);
        rect.anchoredPosition = position;

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
    }

    private static void CreateButton(Transform parent, string label, Font font,
        Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUiObject(label + " Button", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300f, 54f);
        rect.anchoredPosition = position;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.055f, 0.045f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.72f, 0.65f);
        colors.pressedColor = new Color(0.72f, 0.36f, 0.3f);
        button.colors = colors;

        CreateText(buttonObject.transform, label, font, 24, Vector2.zero, Color.white, FontStyle.Bold);
        RectTransform labelRect = buttonObject.transform.GetChild(0).GetComponent<RectTransform>();
        StretchToParent(labelRect);
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        var uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
        DontDestroyOnLoad(eventSystemObject);
    }
}
