using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 13f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction = Vector2.right;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 travelDirection)
    {
        direction = travelDirection.normalized;
        if (direction == Vector2.zero)
            direction = Vector2.right;

        spriteRenderer.flipX = direction.x < 0f;
    }

    private void FixedUpdate()
    {
        body.linearVelocity = direction * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController2D>() != null)
            return;

        ZombieHealth zombieHealth = other.GetComponentInParent<ZombieHealth>();
        if (zombieHealth != null)
        {
            zombieHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
