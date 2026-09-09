using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
public sealed class PlayerController2D : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 55f;
    [SerializeField] private float jumpForce = 11.5f;
    [SerializeField] private float maximumFallSpeed = 18f;

    [Header("Game Feel")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float shortJumpMultiplier = 0.5f;

    [Header("Suelo")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.12f;

    [Header("Disparo")]
    [SerializeField] private Material shootMaterial;
    [SerializeField] private float shootAnimationDuration = 0.5f;

    private Rigidbody2D body;
    private CapsuleCollider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float horizontalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool isGrounded;
    private bool isDroppingThroughPlatform;
    private bool isDead;
    private Material defaultMaterial;
    private Coroutine shootMaterialRoutine;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int GroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        defaultMaterial = spriteRenderer.sharedMaterial;
    }

    private void Update()
    {
        if (isDead)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        horizontalInput = (right ? 1f : 0f) - (left ? 1f : 0f);

        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        jumpHeld = keyboard.spaceKey.isPressed;

        if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            TryDropThroughPlatform();

        bool shootPressed = keyboard.fKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame;
        if (shootPressed && isGrounded && Mathf.Abs(horizontalInput) < 0.01f && animator != null)
        {
            animator.SetTrigger(ShootHash);
            if (shootMaterialRoutine != null)
                StopCoroutine(shootMaterialRoutine);
            shootMaterialRoutine = StartCoroutine(UseShootMaterialTemporarily());
        }

        if (horizontalInput > 0.01f)
            spriteRenderer.flipX = false;
        else if (horizontalInput < -0.01f)
            spriteRenderer.flipX = true;

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocityY);
            return;
        }

        isGrounded = CheckGrounded();
        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - Time.fixedDeltaTime;

        float targetSpeed = horizontalInput * moveSpeed;
        float changeRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float nextHorizontalSpeed = Mathf.MoveTowards(body.linearVelocityX, targetSpeed, changeRate * Time.fixedDeltaTime);
        body.linearVelocity = new Vector2(nextHorizontalSpeed, Mathf.Max(body.linearVelocityY, -maximumFallSpeed));

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            body.linearVelocity = new Vector2(body.linearVelocityX, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            isGrounded = false;
        }

        if (!jumpHeld && body.linearVelocityY > 0f)
            body.linearVelocity = new Vector2(body.linearVelocityX, body.linearVelocityY * shortJumpMultiplier);
    }

    private bool CheckGrounded()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 size = new Vector2(bounds.size.x * 0.82f, 0.08f);
        return Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, groundMask);
    }

    private void TryDropThroughPlatform()
    {
        if (isDroppingThroughPlatform || bodyCollider == null)
            return;

        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 size = new Vector2(bounds.size.x * 0.8f, 0.25f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, groundMask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.GetComponent<PlatformEffector2D>() == null)
                continue;

            StartCoroutine(DropThroughTemporarily(hit));
            break;
        }
    }

    private IEnumerator DropThroughTemporarily(Collider2D platformCollider)
    {
        isDroppingThroughPlatform = true;
        Physics2D.IgnoreCollision(bodyCollider, platformCollider, true);
        body.linearVelocity = new Vector2(body.linearVelocityX, -2f);

        yield return new WaitForSeconds(0.35f);

        if (bodyCollider != null && platformCollider != null)
            Physics2D.IgnoreCollision(bodyCollider, platformCollider, false);

        isDroppingThroughPlatform = false;
    }

    private IEnumerator UseShootMaterialTemporarily()
    {
        if (shootMaterial != null)
            spriteRenderer.sharedMaterial = shootMaterial;

        yield return new WaitForSeconds(shootAnimationDuration);

        if (spriteRenderer != null && defaultMaterial != null)
            spriteRenderer.sharedMaterial = defaultMaterial;

        shootMaterialRoutine = null;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        horizontalInput = 0f;
        body.linearVelocity = new Vector2(0f, body.linearVelocityY);

        if (shootMaterialRoutine != null)
        {
            StopCoroutine(shootMaterialRoutine);
            shootMaterialRoutine = null;
        }

        if (shootMaterial != null)
            spriteRenderer.sharedMaterial = shootMaterial;

        if (animator != null)
        {
            animator.ResetTrigger(ShootHash);
            animator.SetFloat(SpeedHash, 0f);
            animator.SetFloat(VerticalSpeedHash, 0f);
            animator.SetBool(GroundedHash, true);
            animator.SetTrigger(DieHash);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, Mathf.Abs(horizontalInput));
        animator.SetFloat(VerticalSpeedHash, body != null ? body.linearVelocityY : 0f);
        animator.SetBool(GroundedHash, isGrounded);
    }
}
