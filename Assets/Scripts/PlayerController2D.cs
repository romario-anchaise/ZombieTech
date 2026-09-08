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

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int GroundedHash = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
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

        if (horizontalInput > 0.01f)
            spriteRenderer.flipX = false;
        else if (horizontalInput < -0.01f)
            spriteRenderer.flipX = true;

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
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

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, Mathf.Abs(horizontalInput));
        animator.SetFloat(VerticalSpeedHash, body != null ? body.linearVelocityY : 0f);
        animator.SetBool(GroundedHash, isGrounded);
    }
}
