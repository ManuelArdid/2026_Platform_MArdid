using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    //------- Unity Editor Variables -------//
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float jumpForce = 10f;
    [SerializeField] protected InputActionReference MovementInputAction;
    [SerializeField] protected InputActionReference JumpInputAction;


    //------- Private Variables -------//
    private Rigidbody2D _rb;
    private Vector2 _rawMovementInput;
    private bool _jumpRequested = false;

    //------- Unity Methods -------//
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        _rb.linearVelocity = _rawMovementInput * moveSpeed;

        if (_jumpRequested)
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
    }

    void OnDisable()
    {
        MovementInputAction.action.Disable();
        JumpInputAction.action.Disable();

        //callbacks
        MovementInputAction.action.performed -= Move;
        JumpInputAction.action.performed -= Jump;
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
}

