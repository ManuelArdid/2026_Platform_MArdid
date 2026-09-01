using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class IceCube : MonoBehaviour, IResetable
{

    //----------------- UNITY EDITOR -----------------//
    [SerializeField] protected Sprite IceCubeEmptySprite = null;
    [SerializeField] protected Sprite IceCubeFullSprite = null;

    //----------------- PRIVATE VARIABLES -----------------//
    protected SpriteRenderer _spriteRenderer = null;
    protected BoxCollider2D _boxCollider2D = null;
    protected Vector2 _originalColliderSize = Vector2.zero;

    //----------------- UNITY METHODS -----------------//

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _originalColliderSize = _boxCollider2D.size;
    }

    void Start()
    {
        _spriteRenderer.sprite = IceCubeEmptySprite;
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandleOnPlayerReset;

    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandleOnPlayerReset;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IceCubeGoFull();
        }
    }

    //----------------- PUBLIC METHODS -----------------//
    public void IceCubeGoFull()
    {
        gameObject.layer = LayerMask.NameToLayer("Ground");
        _boxCollider2D.isTrigger = false;
        _boxCollider2D.size = new Vector2(1, 1);
        _spriteRenderer.sprite = IceCubeFullSprite;
    }

    public void IceCubeGoEmpty()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        _boxCollider2D.isTrigger = true;
        _boxCollider2D.size = _originalColliderSize;
        _spriteRenderer.sprite = IceCubeEmptySprite;
    }

    //----------------- HANDLE METHODS -----------------//

    private void HandleOnPlayerReset()
    {
        Reset();
    }

    //----------------- INTERFACE IMPLEMENTATION -----------------//
    public void Reset()
    {
        IceCubeGoEmpty();
    }
}
