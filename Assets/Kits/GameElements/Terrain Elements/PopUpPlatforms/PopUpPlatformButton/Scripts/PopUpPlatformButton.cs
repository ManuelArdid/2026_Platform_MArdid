using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PopUpPlatformButton : MonoBehaviour, IResetable
{
    //------------ UNITY EDITOR ------------//
    [SerializeField] protected Sprite SpriteRed;
    [SerializeField] protected Sprite SpriteBlue;
    [SerializeField] protected float PressedTime = 0.2f;

    //------------ CLASS VARIABLES ------------//
    private ButtonState _currentState;
    private SpriteRenderer _spriteRenderer;
    private bool _canChangeState = true;
    private Coroutine _currentPressedTimeCoroutine;
    protected readonly ButtonState InitialState = ButtonState.Blue; //Make sure is the same of _initialDeactivatedType in PopUpPlatform.cs

    //------------ EVENTS ------------//
    public static event Action<ButtonState> OnPopUpPlatformButtonSwitched;

    //------------ ENUMS ------------//
    public enum ButtonState
    {
        Red,
        Blue
    }

    //------------ UNITY METHODS -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentState = InitialState;

        //Set initial sprite based on the initial state
        switch (InitialState)
        {
            case ButtonState.Red:
                _spriteRenderer.sprite = SpriteRed;
                break;
            case ButtonState.Blue:
                _spriteRenderer.sprite = SpriteBlue;
                break;
        }
    }

    void Start()
    {
        OnPopUpPlatformButtonSwitched?.Invoke(_currentState);
    }

    void OnEnable()
    {
        OnPopUpPlatformButtonSwitched += HandleOnPopUpPlatformButtonSwitched;

        Player.OnPlayerReset += HandleOnPlayerReset;
    }

    void OnDisable()
    {
        OnPopUpPlatformButtonSwitched -= HandleOnPopUpPlatformButtonSwitched;

        Player.OnPlayerReset -= HandleOnPlayerReset;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _canChangeState)
        {
            SwitchButtonState();
            _currentPressedTimeCoroutine = StartCoroutine(PressedTimeCoroutine());
        }
    }

    //------------ PUBLIC METHODS -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//

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
        _canChangeState = true;
        _currentState = InitialState;

        if (InitialState == ButtonState.Red)
        {
            _spriteRenderer.sprite = SpriteRed;
            OnPopUpPlatformButtonSwitched?.Invoke(ButtonState.Red);
        }
        else if (InitialState == ButtonState.Blue)
        {
            _spriteRenderer.sprite = SpriteBlue;
            OnPopUpPlatformButtonSwitched?.Invoke(ButtonState.Blue);
        }
    }

    //------------ HANDLERS -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//

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

    private void HandleOnPlayerReset()
    {
        Reset();
    }

    //----------- COROUTINES -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    /// <summary>
    /// Pressed Time Coroutine
    /// </summary>
    private IEnumerator PressedTimeCoroutine()
    {
        if (_currentPressedTimeCoroutine != null)
        {
            StopCoroutine(_currentPressedTimeCoroutine);
        }

        _canChangeState = false;
        yield return new WaitForSecondsRealtime(PressedTime);
        _canChangeState = true;

        _currentPressedTimeCoroutine = null;
    }
}