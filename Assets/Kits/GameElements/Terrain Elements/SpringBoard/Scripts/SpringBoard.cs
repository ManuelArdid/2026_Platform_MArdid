using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SpringBoard : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite SpriteUsed;

    [Header("Settings")]
    [SerializeField] private float SpeedMultiplier = 1.2f;

    private bool _isUsed = false;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _originalSprite = _spriteRenderer.sprite;
    }

    private void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
    }

    private void OnDisable()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isUsed)
            return;

        if (!collision.CompareTag("Player"))
            return;

        Rigidbody2D playerRigidbody = collision.attachedRigidbody;
        if (playerRigidbody == null)
            return;

        float enterYVelocity = collision.gameObject.GetComponent<Player>().PlayerGetLastFallSpeed();

        // Only bounce if the player is falling onto the springboard, not if they are jumping up through it.
        if (enterYVelocity > 0f)
            return;

        _isUsed = true;

        _spriteRenderer.sprite = SpriteUsed;

        // Disable the collider to prevent multiple bounces while the player is still in contact with the springboard.
        _boxCollider.enabled = false;

        MakePlayerJump(playerRigidbody, enterYVelocity);
    }

    private void MakePlayerJump(Rigidbody2D playerRigidbody, float enterYVelocity)
    {
        // If the player fell from a height, matching the impact speed
        // sends them back to roughly the same height.
        float bounceSpeed = Mathf.Abs(enterYVelocity);
        bounceSpeed *= SpeedMultiplier;

        Vector2 velocity = playerRigidbody.linearVelocity;
        velocity.y = bounceSpeed;
        playerRigidbody.linearVelocity = velocity;
    }

    private void HandlePlayerReset()
    {
        _isUsed = false;
        _spriteRenderer.sprite = _originalSprite;
        _boxCollider.enabled = true;
    }
}