using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Movement : MonoBehaviour
{
    //------- Unity Editor Variables -------//
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float acceleration = 10f;
    [SerializeField] protected float deceleration = 60f;

    [Header("Jump Settings")]
    [SerializeField] protected float jumpForce = 10f;
    [Tooltip("Multiplier to apply to upward velocity when jump is released early for variable jump height.")]
    [SerializeField] protected float jumpCutMultiplier = 0.5f;

    [Header("Input Actions")]
    [SerializeField] protected InputActionReference MovementInputAction;
    [SerializeField] protected InputActionReference JumpInputAction;

    [Header("Ground Check Settings")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundCheckDistance = 0.1f;
    [SerializeField] protected Transform groundCheckPoint;


    //------- Private Variables -------//
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _rawMovementInput;
    private bool _jumpRequested = false;
    private Vector2 _currentVelocity = Vector2.zero;
    private bool _onPlatform = false;
    private bool _isJumping = false;
    private bool _isdoubleJumping = false;

    //------- Unity Methods -------//
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        //MOVEMENT
        Vector2 targetVelocity = _rawMovementInput * moveSpeed;

        float currentAcceleration = _rawMovementInput == Vector2.zero
            ? deceleration
            : acceleration;

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
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _isJumping = true;


            //Double jump
            if (!IsGrounded())
            {
                //animation trigger
                _animator.SetTrigger("PerformDoubleJump");
                _isdoubleJumping = true;
            }

            _jumpRequested = false;
        }

        //Ground check reset
        if (IsGrounded())
        {
            _isJumping = false;
            _isdoubleJumping = false;
        }

        //ANIMATIONS

        //Running
        _animator.SetBool("IsRunning", _currentVelocity.x != 0 && IsGrounded());

        //Falling
        _animator.SetBool("IsFalling", !IsGrounded() && _rb.linearVelocityY < 0f && !_isJumping && !_isdoubleJumping);

        //Jumping
        _animator.SetBool("IsJumping", !IsGrounded() && _rb.linearVelocityY > 0f && _isJumping   && !_isdoubleJumping);

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
        }
    }


    //------- Private Methods -------//

    /// <summary>
    /// Handles character movement based on player input.
    /// </summary>
    void Move(InputAction.CallbackContext context)
    {
        _rawMovementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles character jump based on player input.
    /// </summary>
    void Jump(InputAction.CallbackContext context)
    {
        _jumpRequested = true;
    }

    /// <summary>
    /// Handles jump cut for variable jump height.
    /// </summary>
    void JumpCancelled(InputAction.CallbackContext context)
    {
        if (_rb.linearVelocityY > 0f)
        {
            var currentVelocityY = _rb.linearVelocityY;
            _rb.linearVelocityY = currentVelocityY * jumpCutMultiplier;
        }

        _isJumping = false;
    }

    /// <summary>
    /// Checks if the character is grounded.
    /// </summary>
    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null || _onPlatform;
    }
}
