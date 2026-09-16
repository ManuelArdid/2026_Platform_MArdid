using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PopUpPlatform : ConcreteActivable
{

    //------------ UNITY EDITOR ------------//
    [SerializeField] protected Sprite RedSpriteOn;
    [SerializeField] protected Sprite BlueSpriteOn;
    [SerializeField] protected Sprite RedSpriteOff;
    [SerializeField] protected Sprite BlueSpriteOff;
    [SerializeField] protected PlatfromType Type = PlatfromType.Red;

    //------------ CLASS VARIABLES ------------//
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private PlatfromType _initialDeactivatedType = PlatfromType.Red; //Make sure is the same of InitialState in PopUpPlatformButton.cs

    //------------ ENUMS ------------//
    public enum PlatfromType
    {
        Red,
        Blue
    }

    //------------ UNITY METHODS ------------//
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (_initialDeactivatedType == Type)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    void OnEnable()
    {
        PopUpPlatformButton.OnPopUpPlatformButtonSwitched += HandleOnPopUpPlatformButtonSwitched;
    }

    void OnDisable()
    {
        PopUpPlatformButton.OnPopUpPlatformButtonSwitched -= HandleOnPopUpPlatformButtonSwitched;
    }

    //------------ PUBLIC METHODS ------------//

    /// <summary>
    /// Activates the platform, making it visible and enabling its collider.
    /// </summary>
    public override void Activate()
    {
        if (Type == PlatfromType.Red)
        {
            _spriteRenderer.sprite = RedSpriteOn;
        }
        else
        {
            _spriteRenderer.sprite = BlueSpriteOn;
        }
        _collider.enabled = true;
        IsActivated = true;
    }

    /// <summary>
    /// Deactivates the platform, making it invisible and disabling its collider.
    /// </summary>
    public override void Deactivate()
    {
        if (Type == PlatfromType.Red)
        {
            _spriteRenderer.sprite = RedSpriteOff;
        }
        else
        {
            _spriteRenderer.sprite = BlueSpriteOff;
        }
        _collider.enabled = false;
        IsActivated = false;
    }

    /// <summary>
    /// Returns the type of the platform (Red or Blue). 
    /// </summary>
    /// <returns></returns>
    public PlatfromType GetPlatformType()
    {
        return Type;
    }

    //------------ HANDLERS ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    private void HandleOnPopUpPlatformButtonSwitched(PopUpPlatformButton.ButtonState state)
    {
        if ((state == PopUpPlatformButton.ButtonState.Red && Type == PlatfromType.Red) ||
            (state == PopUpPlatformButton.ButtonState.Blue && Type == PlatfromType.Blue))
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

}