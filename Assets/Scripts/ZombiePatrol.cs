using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
public sealed class ZombiePatrol : MonoBehaviour
{
    [Header("Patrullaje")]
    [SerializeField] private float moveSpeed = 1.15f;
    [SerializeField] private float patrolDistance = 1.25f;

    [Header("Persecucion")]
    [SerializeField] private float chaseSpeed = 1.65f;
    [SerializeField] private float stoppingDistance = 0.9f;

    [Header("Ataque")]
    [SerializeField] private float damageDelay = 0.32f;
    [SerializeField] private float attackDuration = 0.65f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    private PlayerController2D playerController;
    private float startingPositionX;
    private float direction = -1f;
    private bool isAttacking;
    private bool hasAttacked;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        GameObject playerObject = GameObject.Find("Player");
        player = playerObject != null ? playerObject.transform : null;
        playerController = playerObject != null ? playerObject.GetComponent<PlayerController2D>() : null;
        startingPositionX = transform.position.x;
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (TryChasePlayer())
        {
            UpdateFacingDirection();
            return;
        }

        float distanceFromStart = transform.position.x - startingPositionX;
        if (distanceFromStart >= patrolDistance)
            direction = -1f;
        else if (distanceFromStart <= -patrolDistance)
            direction = 1f;

        body.linearVelocity = new Vector2(direction * moveSpeed, body.linearVelocityY);
        UpdateFacingDirection();
    }

    private bool TryChasePlayer()
    {
        if (player == null)
            return false;

        float horizontalDistance = player.position.x - transform.position.x;
        float absoluteDistance = Mathf.Abs(horizontalDistance);
        if (absoluteDistance <= stoppingDistance)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocityY);
            if (!isAttacking && !hasAttacked && playerController != null)
                StartCoroutine(AttackPlayer());
            return true;
        }

        direction = Mathf.Sign(horizontalDistance);
        body.linearVelocity = new Vector2(direction * chaseSpeed, body.linearVelocityY);
        return true;
    }

    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        if (animator != null)
            animator.SetTrigger(AttackHash);

        yield return new WaitForSeconds(damageDelay);

        if (playerController != null)
            playerController.Die();

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - damageDelay));
        hasAttacked = true;
        isAttacking = false;
    }

    private void UpdateFacingDirection()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = direction < 0f;
    }
}
