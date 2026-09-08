using UnityEngine;

/// <summary>
/// Creates a persistent music player when the game starts.
/// </summary>
public static class BackgroundMusic
{
    private const string MusicResourcePath = "Audio/BecomeTheAssassin";
    private const float MusicVolume = 0.3f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartMusic()
    {
        if (GameObject.Find("Background Music") != null)
        {
            return;
        }

        AudioClip musicClip = Resources.Load<AudioClip>(MusicResourcePath);
        if (musicClip == null)
        {
            Debug.LogWarning($"Background music was not found at Resources/{MusicResourcePath}.");
            return;
        }

        GameObject musicObject = new GameObject("Background Music");
        Object.DontDestroyOnLoad(musicObject);

        AudioSource audioSource = musicObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = MusicVolume;
        audioSource.spatialBlend = 0f;
        audioSource.Play();
    }
}
