using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PopUpPlatform : ConcreteActivable
{

    //------------ UNITY EDITOR ------------//
    [SerializeField] protected Sprite SpriteOn;
    [SerializeField] protected Sprite SpriteOff;
    [SerializeField] protected PlatfromType Type = PlatfromType.Red;

    //------------ CLASS VARIABLES ------------//
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    //------------ ENUMS ------------//
    public enum PlatfromType
    {
        Red,
        Blue
    }

    //------------ UNITY METHODS ------------//
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
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
    public override void Activate()
    {
        _spriteRenderer.sprite = SpriteOn;
        _collider.enabled = true;
        IsActivated = true;
    }

    public override void Deactivate()
    {
        _spriteRenderer.sprite = SpriteOff;
        _collider.enabled = false;
        IsActivated = false;
    }

    public PlatfromType GetPlatformType()
    {
        return Type;
    }

    //------------ HANDLERS ------------//
    private void HandleOnPopUpPlatformButtonSwitched(PopUpPlatformButton.ButtonState state)
    {
        if (state == PopUpPlatformButton.ButtonState.Red && Type == PlatfromType.Red)
        {
            Activate();
        }
        else if (state == PopUpPlatformButton.ButtonState.Blue && Type == PlatfromType.Blue)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }
}
