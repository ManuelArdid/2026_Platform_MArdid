using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
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
    [SerializeField] protected float DoubleJumpReduction = 0.5f;

    [Header("Input Actions")]
    [SerializeField] protected InputActionReference MovementInputAction;
    [SerializeField] protected InputActionReference JumpInputAction;

    [Header("Spawn Settings")]
    [SerializeField] protected Transform SpawnPoint;

    //------ Events ------//
    public static event System.Action OnPlayerReset;


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

    private int _doubleJumpCounter = 0;

    //------- Unity Methods -------//
    void Start()
    {

        //Restart player prefs for deebug only
        PlayerPrefs.DeleteAll();

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        //Initialize jumps
        _jumpsRemaining = MaximumJumps;

        //Load spawn point from PlayerPrefs
        if (PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY") && PlayerPrefs.HasKey("SpawnZ"))
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            float z = PlayerPrefs.GetFloat("SpawnZ");

            SpawnPoint.position = new Vector3(x, y, z);
        }

        //Set player to spawn point
        transform.position = SpawnPoint.position;
    }

    void FixedUpdate()
    {
        //RESET CHECK
        if (_jumpsRemaining < 0)
        {
            transform.position = SpawnPoint.position;
            _jumpsRemaining = MaximumJumps;
            OnPlayerReset?.Invoke();
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
            _jumpsRemaining--;

            //From ground or coyote time
            if (IsGrounded() || _canUseCoyoteTime)
            {
                _rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

            }

            //Double jump
            else if (!IsGrounded() && !_canUseCoyoteTime)
            {
                _doubleJumpCounter++;
                _rb.AddForce(Vector2.up * (JumpForce / (CalculateDoubleJumpDivisor())), ForceMode2D.Impulse);

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
            _doubleJumpCounter = 0;
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

    //------- Public Methods -------//
    public void SetSpawnPoint(Vector3 spawnPosition)
    {
        if (SpawnPoint == null)
        {
            GameObject temp = new GameObject("RuntimeSpawnPoint");
            SpawnPoint = temp.transform;
        }

        SpawnPoint.position = spawnPosition;
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

    /// <summary>
    /// Handles lilypad collected event.
    /// Resets jumps remaining.
    /// </summary>
    private void HandleLilypadCollected(Lilypad lilypad)
    {
        _jumpsRemaining = MaximumJumps;
    }

    /// <summary>
    /// Calculates the divisor for double jump based on the number of double jumps already performed.
    /// </summary>
    private float CalculateDoubleJumpDivisor()
    {
        return 1f + DoubleJumpReduction * (_doubleJumpCounter * _doubleJumpCounter);
    }
}
