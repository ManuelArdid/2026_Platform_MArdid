using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PopUpPlatformButton : MonoBehaviour, IResetable
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

        //Set initial sprite based on the initial state
        switch (InitialState)
        {
            case ButtonState.Red:
                _spriteRenderer.sprite = SpriteRed;
                _currentState = ButtonState.Red;
                OnPopUpPlatformButtonSwitched?.Invoke(ButtonState.Red);
                break;
            case ButtonState.Blue:
                _spriteRenderer.sprite = SpriteBlue;
                _currentState = ButtonState.Blue;
                OnPopUpPlatformButtonSwitched?.Invoke(ButtonState.Blue);
                break;
        }
    }

    void OnEnable()
    {
        OnPopUpPlatformButtonSwitched += HandleOnPopUpPlatformButtonSwitched;
    }

    void OnDisable()
    {
        OnPopUpPlatformButtonSwitched -= HandleOnPopUpPlatformButtonSwitched;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchButtonState();
        }
    }

    //------------ PUBLIC METHODS ------------//

    /// <summary>
    /// Switches the button state between Red and Blue, updates the sprite accordingly, and invokes the OnPopUpPlatformButtonSwitched event with the new state.
    /// </summary>
    public void SwitchButtonState()
    {

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

        OnPopUpPlatformButtonSwitched?.Invoke(_currentState);
    }

    /// <summary>
    /// Resets the button to its initial state, updating the sprite and invoking the OnPopUpPlatformButtonSwitched event with the initial state.
    /// </summary>
    public void Reset()
    {
        _currentState = InitialState;

        switch (InitialState)
        {
            case ButtonState.Red:
                _spriteRenderer.sprite = SpriteRed;
                OnPopUpPlatformButtonSwitched?.Invoke(ButtonState.Red);
                break;
            case ButtonState.Blue:
                _spriteRenderer.sprite = SpriteBlue;
                OnPopUpPlatformButtonSwitched?.Invoke(ButtonState.Blue);
                break;
        }
    }

    //------------ HANDLERS ------------//
    /// <summary>
    /// Handles the OnPopUpPlatformButtonSwitched event by updating the button's sprite based on the new state. If the new state is Red and the current state is Blue, it switches to the Red sprite. If the new state is Blue and the current state is Red, it switches to the Blue sprite. The method ensures that the button's visual representation always matches its current state.
    /// </summary>
    /// <param name="state"></param>
    private void HandleOnPopUpPlatformButtonSwitched(ButtonState state)
    {
        if (state == ButtonState.Blue)
        {
            _spriteRenderer.sprite = SpriteBlue;
            _currentState = ButtonState.Blue;
        }
        else if (state == ButtonState.Red)
        {
            _spriteRenderer.sprite = SpriteRed;
            _currentState = ButtonState.Red;
        }
    }
}
