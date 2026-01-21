using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Movement : MonoBehaviour
{
    //------- Unity Editor Variables -------//
    [Header("Movement Settings")]
    [SerializeField] protected float MoveSpeed = 5f;
    [SerializeField] protected float Acceleration = 10f;
    [SerializeField] protected float Deceleration = 60f;

    [Header("Jump Settings")]
    [SerializeField] protected float JumpForce = 10f;
    [Tooltip("Multiplier to apply to upward velocity when jump is released early for variable jump height.")]
    [SerializeField] protected float JumpCutMultiplier = 0.5f;
    [SerializeField] protected float CoyoteTime = 0.2f;
    [SerializeField] protected int MaximumJumps = 5;

    [Header("Input Actions")]
    [SerializeField] protected InputActionReference MovementInputAction;
    [SerializeField] protected InputActionReference JumpInputAction;

    [Header("Ground Check Settings")]
    [SerializeField] protected LayerMask GroundLayer;
    [SerializeField] protected float GroundCheckDistance = 0.1f;
    [SerializeField] protected Transform GroundCheckPoint;

    [Header("Spawn Settings")]
    [SerializeField] protected Transform SpawnPoint;


    //------- Private Variables -------//
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _rawMovementInput;
    private bool _jumpRequested = false;
    private Vector2 _currentVelocity = Vector2.zero;
    private bool _onPlatform = false;
    private bool _isDoubleJumping = false;
    private bool _canUseCoyoteTime = false;
    private Coroutine _currentCoyoteTimeCoroutine = null;
    private int _jumpsRemaining;

    //------- Unity Methods -------//
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        //Initialize jumps
        _jumpsRemaining = MaximumJumps;
    }

    void FixedUpdate()
    {
        //RESET CHECK
        if(_jumpsRemaining < 0)
        {
            transform.position = SpawnPoint.position;
            _jumpsRemaining = MaximumJumps;            
        }

        //MOVEMENT
        Vector2 targetVelocity = _rawMovementInput * MoveSpeed;

        float currentAcceleration = _rawMovementInput == Vector2.zero
            ? Deceleration
            : Acceleration;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        _rb.linearVelocityX = _currentVelocity.x;

        //JUMP
        if (_jumpRequested)
        {
            _rb.linearVelocityY = 0f;
            _rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            _jumpsRemaining--;

            //Double jump
            if (!IsGrounded() && !_canUseCoyoteTime)
            {
                //animation trigger
                _animator.SetTrigger("PerformDoubleJump");
                _isDoubleJumping = true;
            }

            _jumpRequested = false;
        }

        //Ground check reset
        if (IsGrounded())
        {
            _isDoubleJumping = false;
            _canUseCoyoteTime = false;
        }

        //ANIMATIONS

        //Running
        _animator.SetBool("IsRunning", _currentVelocity.x != 0 && IsGrounded());

        //Falling
        _animator.SetBool("IsFalling", _rb.linearVelocityY < 0f && !_isDoubleJumping);

        //Jumping
        _animator.SetBool("IsJumping", _rb.linearVelocityY > 0f && !_isDoubleJumping);

        //Flip sprite
        if (_currentVelocity.x > 0)
            _spriteRenderer.flipX = false;
        else if (_currentVelocity.x < 0)
            _spriteRenderer.flipX = true;

    }

    void OnEnable()
    {
        MovementInputAction.action.Enable();
        JumpInputAction.action.Enable();

        //callbacks
        MovementInputAction.action.performed += Move;
        MovementInputAction.action.canceled += Move;
        MovementInputAction.action.started += Move;

        JumpInputAction.action.started += Jump;
        JumpInputAction.action.canceled += JumpCancelled;

        //event subscriptions
        Lilypad.OnLilypadCollected += HandleLilypadCollected;
    }

    void OnDisable()
    {
        MovementInputAction.action.Disable();
        JumpInputAction.action.Disable();

        //callbacks
        MovementInputAction.action.performed -= Move;
        MovementInputAction.action.canceled -= Move;
        MovementInputAction.action.started -= Move;

        JumpInputAction.action.started -= Jump;
        JumpInputAction.action.canceled -= JumpCancelled;

        //event unsubscriptions
        Lilypad.OnLilypadCollected -= HandleLilypadCollected;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _onPlatform = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _onPlatform = false;

            if (_currentCoyoteTimeCoroutine != null)
            {
                StopCoroutine(_currentCoyoteTimeCoroutine);
            }

            _currentCoyoteTimeCoroutine = StartCoroutine(CoyoteTimeCoroutine());
        }
    }


    //------- Private Methods -------//

    /// <summary>
    /// Handles character movement based on player input.
    /// </summary>
    private void Move(InputAction.CallbackContext context)
    {
        _rawMovementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles character jump based on player input.
    /// </summary>
    private void Jump(InputAction.CallbackContext context)
    {
        _jumpRequested = true;
    }

    /// <summary>
    /// Handles jump cut for variable jump height.
    /// </summary>
    private void JumpCancelled(InputAction.CallbackContext context)
    {
        if (_rb.linearVelocityY > 0f)
        {
            var currentVelocityY = _rb.linearVelocityY;
            _rb.linearVelocityY = currentVelocityY * JumpCutMultiplier;
        }
    }

    /// <summary>
    /// Checks if the character is grounded.
    /// </summary>
    private bool IsGrounded()
    {
        return _onPlatform;
    }

    /// <summary>
    /// Coyote Time Coroutine
    /// </summary>
    private IEnumerator CoyoteTimeCoroutine()
    {
        float elapsedTime = 0f;
        _canUseCoyoteTime = true;

        while (elapsedTime < CoyoteTime)
        {
            // If the player lands, exit the coroutine
            if (_onPlatform)
            {
                _canUseCoyoteTime = false;
                yield break;
            }

            // Allow coyote time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Coyote time expired
        _canUseCoyoteTime = false;
    }

    private void HandleLilypadCollected(Lilypad lilypad)
    {
        _jumpsRemaining = MaximumJumps;
    }
}
