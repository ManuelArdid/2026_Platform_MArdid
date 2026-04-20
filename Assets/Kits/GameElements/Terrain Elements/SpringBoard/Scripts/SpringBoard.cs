using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SpringBoard : MonoBehaviour
{
    //-------------------UNITY EDITOR--------------------//
    [SerializeField] protected float SpringForceLowVelocity = 5f;
    [SerializeField] protected float SpringForce = 4.3f;
    [SerializeField] protected Sprite SpriteUsed;

    //-------------------CLASS VARIABLES--------------------//
    private bool _isUsed = false;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;

    /// Cutrada máxima
    private float _lowVelocityUmbral = 5.9f;

    //-------------------UNITY METHODS--------------------//
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalSprite = _spriteRenderer.sprite;
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isUsed)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Get player rigidbody and its y velocity
            Rigidbody2D playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
            float _rawVelocity = playerRigidbody.linearVelocity.y;

            //Ignore if player is moving upwards
            if (_rawVelocity > 0)
                return;

            _isUsed = true;

            // Change sprite to used sprite
            _spriteRenderer.sprite = SpriteUsed;

            // Disable collider to prevent multiple triggers
            _boxCollider.enabled = false;

            // Get player
            Player player = collision.gameObject.GetComponent<Player>();

            // Get absolute value of player's y velocity
            float playerYVelocity = Mathf.Abs(_rawVelocity);            

            // Make player jump
            MakePlayerJump(player, playerYVelocity);
        }
    }

    //------------------- PRIVATE METHODS --------------------//

    private void MakePlayerJump(Player player, float velocity)
    {
        float _springForceToApply;

        if (velocity < _lowVelocityUmbral)
        {
            _springForceToApply = SpringForceLowVelocity;
        }
        else
        {
            _springForceToApply = SpringForce;
        }

        player.PerformJump(velocity * _springForceToApply);
    }

    //------------------- EVENT HANDLERS --------------------//
    private void HandlePlayerReset()
    {
        _spriteRenderer.sprite = _originalSprite;
        _isUsed = false;
        _boxCollider.enabled = true;
    }
}
