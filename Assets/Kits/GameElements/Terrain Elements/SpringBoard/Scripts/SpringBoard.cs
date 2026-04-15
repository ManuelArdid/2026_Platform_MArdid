using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpringBoard : MonoBehaviour
{
    //-------------------UNITY EDITOR--------------------//
    [SerializeField] protected float JumpForceMultiplier = 1.5f;
    [SerializeField] protected Sprite SpriteUsed;

    //-------------------CLASS VARIABLES--------------------//
    private bool _isUsed = false;
    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;

    //-------------------UNITY METHODS--------------------//
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalSprite = _spriteRenderer.sprite;
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isUsed)
            return;

        _isUsed = true;

        if (collision.gameObject.CompareTag("Player"))
        {
            
            _spriteRenderer.sprite = SpriteUsed;
            MakePlayerJump(collision.gameObject.GetComponent<Player>());
        }
    }

    //------------------- PRIVATE METHODS --------------------//

    private void MakePlayerJump(Player player)
    {
        player.PerformJump(player.PlayerGetJumpForce() * JumpForceMultiplier);
    }

    //------------------- EVENT HANDLERS --------------------//
    private void HandlePlayerReset()
    {
        _spriteRenderer.sprite = _originalSprite;
        _isUsed = false;
    }
}
