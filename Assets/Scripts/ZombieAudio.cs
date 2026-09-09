using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class ZombieAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] idleClips;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private Vector2 idleDelayRange = new Vector2(3.5f, 6.5f);
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private AudioSource audioSource;
    private bool isDead;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(PlayIdleSounds());
    }

    public void Configure(AudioClip[] idle, AudioClip[] attack, AudioClip hurt, AudioClip death)
    {
        idleClips = idle;
        attackClips = attack;
        hurtClip = hurt;
        deathClip = death;
    }

    public void PlayAttack()
    {
        if (!isDead)
            PlayRandom(attackClips);
    }

    public void PlayHurt()
    {
        if (!isDead)
            PlayClip(hurtClip, false);
    }

    public float PlayDeath()
    {
        isDead = true;
        StopAllCoroutines();
        PlayClip(deathClip, true);
        return deathClip != null ? deathClip.length / Mathf.Max(0.01f, Mathf.Abs(audioSource.pitch)) : 0f;
    }

    private IEnumerator PlayIdleSounds()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(Random.Range(idleDelayRange.x, idleDelayRange.y));
            if (!audioSource.isPlaying)
                PlayRandom(idleClips);
        }
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        PlayClip(clips[Random.Range(0, clips.Length)], false);
    }

    private void PlayClip(AudioClip clip, bool interruptCurrentSound)
    {
        if (clip == null)
            return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        if (interruptCurrentSound)
            audioSource.Stop();
        audioSource.PlayOneShot(clip);
    }
}
