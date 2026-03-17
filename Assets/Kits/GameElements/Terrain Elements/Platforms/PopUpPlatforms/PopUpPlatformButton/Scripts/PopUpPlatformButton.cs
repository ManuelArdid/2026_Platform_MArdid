using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PopUpPlatformButton : MonoBehaviour
{
    //------------ UNITY EDITOR ------------//
    [SerializeField] protected Sprite SpriteRed;
    [SerializeField] protected Sprite SpriteBlue;
    [SerializeField] protected ButtonState InitialState = ButtonState.Red;

    //------------ CLASS VARIABLES ------------//
    private ButtonState _currentState;
    private SpriteRenderer _spriteRenderer;

    //------------ EVENTS ------------//
    public static event Action<ButtonState> OnPopUpPlatformButtonSwitched;

    //------------ ENUMS ------------//
    public enum ButtonState
    {
        Red,
        Blue
    }

    //------------ UNITY METHODS ------------//
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentState = InitialState;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchButtonState();
        }
    }

    //------------ PUBLIC METHODS ------------//
    public void SwitchButtonState()
    {
        OnPopUpPlatformButtonSwitched?.Invoke(_currentState);

        switch (_currentState)
        {
            //RED -> BLUE
            case ButtonState.Red:
                _spriteRenderer.sprite = SpriteBlue;
                _currentState = ButtonState.Blue;
                break;

            //BLUE -> RED
            case ButtonState.Blue:
                _spriteRenderer.sprite = SpriteRed;
                _currentState = ButtonState.Red;
                break;
        }

    }
}
