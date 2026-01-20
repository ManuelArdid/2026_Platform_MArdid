using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
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
    private Vector2 _rawMovementInput;
    private bool _jumpRequested = false;
    private Vector2 _currentVelocity = Vector2.zero;

    //------- Unity Methods -------//
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
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
            if (IsGrounded())
            {
                _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            _jumpRequested = false;
        }
    }

    void OnEnable()
    {
        MovementInputAction.action.Enable();
        JumpInputAction.action.Enable();

        //callbacks
        MovementInputAction.action.performed += Move;
        MovementInputAction.action.canceled += Move;
        MovementInputAction.action.started += Move;

        JumpInputAction.action.performed += Jump;
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

        JumpInputAction.action.performed -= Jump;
        JumpInputAction.action.started -= Jump;
        JumpInputAction.action.canceled -= JumpCancelled;
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

    void JumpCancelled(InputAction.CallbackContext context)
    {
        if (_rb.linearVelocityY > 0f)
        {
            var currentVelocityY = _rb.linearVelocityY;
            _rb.linearVelocityY = currentVelocityY * jumpCutMultiplier;
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }
}
