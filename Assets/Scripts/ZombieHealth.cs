using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator), typeof(Rigidbody2D))]
public sealed class ZombieHealth : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 3;
    [SerializeField] private float hitFlashDuration = 0.12f;
    [SerializeField] private float deathAnimationDuration = 0.8f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private int currentHealth;
    private Coroutine flashRoutine;
    private bool isDead;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        currentHealth = maximumHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0 || isDead)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth == 0)
        {
            StartCoroutine(Die());
            return;
        }

        ZombieAudio zombieAudio = GetComponent<ZombieAudio>();
        if (zombieAudio != null)
            zombieAudio.PlayHurt();

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashAfterHit());
    }

    private IEnumerator FlashAfterHit()
    {
        spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
        yield return new WaitForSeconds(hitFlashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    private IEnumerator Die()
    {
        isDead = true;
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        spriteRenderer.color = Color.white;

        ZombiePatrol patrol = GetComponent<ZombiePatrol>();
        if (patrol != null)
            patrol.DisableForDeath();

        if (bodyCollider != null)
            bodyCollider.enabled = false;
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(DieHash);
        }

        ZombieAudio zombieAudio = GetComponent<ZombieAudio>();
        float deathSoundDuration = zombieAudio != null ? zombieAudio.PlayDeath() : 0f;

        yield return new WaitForSeconds(Mathf.Max(deathAnimationDuration, deathSoundDuration));
        Destroy(gameObject);
    }
}
