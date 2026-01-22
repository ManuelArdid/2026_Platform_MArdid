using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Player : MonoBehaviour
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
    private Coroutine _currentCoyoteTimeCoroutine = null;

    //------- Protected Variables -------//
    protected Rigidbody2D _rb;
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;

    protected Vector2 _rawMovementInput;
    protected Vector2 _currentVelocity = Vector2.zero;

    protected bool _jumpRequested = false;
    protected bool _onPlatform = false;
    protected bool _isDoubleJumping = false;
    protected bool _canUseCoyoteTime = false;

    protected int _jumpsRemaining;
    protected int _doubleJumpCounter = 0;

    //------- Unity Methods -------//
    protected virtual void Start()
    {
        //Restart player prefs for debug only
        PlayerPrefs.DeleteAll();

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        //Initialize jumps
        _jumpsRemaining = MaximumJumps;

        //Load spawn point from PlayerPrefs
        if (PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY") && PlayerPrefs.HasKey("SpawnZ"))
        {
            SpawnPoint.position = new Vector3(
                PlayerPrefs.GetFloat("SpawnX"),
                PlayerPrefs.GetFloat("SpawnY"),
                PlayerPrefs.GetFloat("SpawnZ")
            );
        }

        //Set player to spawn point
        transform.position = SpawnPoint.position;
    }

    protected virtual void Update()
    {
        // Animations
        _animator.SetBool("IsRunning", _currentVelocity.x != 0 && IsGrounded());
        _animator.SetBool("IsFalling", _rb.linearVelocityY < 0f && !_isDoubleJumping);
        _animator.SetBool("IsJumping", _rb.linearVelocityY > 0f && !_isDoubleJumping);

        // Flip sprite
        if (_currentVelocity.x > 0)
            _spriteRenderer.flipX = false;
        else if (_currentVelocity.x < 0)
            _spriteRenderer.flipX = true;
    }

    protected virtual void FixedUpdate()
    {
        //RESET CHECK
        if (_jumpsRemaining < 0)
        {
            SendPlayerToSpawnPoint();
        }

        HandleMovement();
        HandleJump();
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
        //callbacks
        MovementInputAction.action.performed -= Move;
        MovementInputAction.action.canceled -= Move;
        MovementInputAction.action.started -= Move;

        JumpInputAction.action.started -= Jump;
        JumpInputAction.action.canceled -= JumpCancelled;

        MovementInputAction.action.Disable();
        JumpInputAction.action.Disable();

        //event unsubscriptions
        Lilypad.OnLilypadCollected -= HandleLilypadCollected;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _onPlatform = true;
            _isDoubleJumping = false;
            _canUseCoyoteTime = false;
            _doubleJumpCounter = 0;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _onPlatform = false;

            // Start Coyote Time Coroutine if component is enabled
            if (isActiveAndEnabled)
            {
                if (_currentCoyoteTimeCoroutine != null)
                    StopCoroutine(_currentCoyoteTimeCoroutine);

                _currentCoyoteTimeCoroutine = StartCoroutine(CoyoteTimeCoroutine());
            }
        }
    }

    //------- Public Methods -------//
    public void SendPlayerToSpawnPoint()
    {
        transform.position = SpawnPoint.position;
        _jumpsRemaining = MaximumJumps;
        OnPlayerReset?.Invoke();
    }

    /// <summary>
    /// Sets the spawn point position for the player.
    /// </summary>
    public void SetSpawnPoint(Vector3 spawnPosition)
    {
        if (SpawnPoint == null)
        {
            GameObject temp = new GameObject("RuntimeSpawnPoint");
            SpawnPoint = temp.transform;
        }

        SpawnPoint.position = spawnPosition;
    }

    //------- Protected Methods -------//
    /// <summary>
    /// Handles character movement based on player input.
    /// </summary>
    protected virtual void Move(InputAction.CallbackContext context)
    {
        _rawMovementInput = context.ReadValue<Vector2>();
    }

    protected virtual void HandleMovement() { }

    protected virtual void HandleJump()
    {
        if (!_jumpRequested) return;

        _rb.linearVelocityY = 0f;
        _jumpsRemaining--;
        Debug.Log("Jumps Remaining: " + _jumpsRemaining);

        if (IsGrounded() || _canUseCoyoteTime)
        {
            PerformJump(JumpForce);
        }
        else
        {
            _doubleJumpCounter++;
            PerformJump(JumpForce / CalculateDoubleJumpDivisor());
            _animator.SetTrigger("PerformDoubleJump");
            _isDoubleJumping = true;
        }

        _jumpRequested = false;
    }

    protected virtual void PerformJump(float force)
    {
        _rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Checks if the character is grounded.
    /// </summary>
    protected bool IsGrounded()
    {
        return _onPlatform;
    }

    //------- Private Methods -------//
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
            _rb.linearVelocityY *= JumpCutMultiplier;
        }
    }

    /// <summary>
    /// Coyote Time Coroutine
    /// </summary>
    private IEnumerator CoyoteTimeCoroutine()
    {
        _canUseCoyoteTime = true;
        float elapsedTime = 0f;

        while (elapsedTime < CoyoteTime)
        {
            if (_onPlatform)
            {
                _canUseCoyoteTime = false;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _canUseCoyoteTime = false;
    }

    /// <summary>
    /// Calculates the divisor for double jump based on the number of double jumps already performed.
    /// </summary>
    private float CalculateDoubleJumpDivisor()
    {
        return 1f + DoubleJumpReduction * (_doubleJumpCounter * _doubleJumpCounter);
    }

    /// <summary>
    /// Handles lilypad collected event.
    ///  Resets jumps remaining.
    /// </summary>
    private void HandleLilypadCollected(Lilypad lilypad)
    {
        _jumpsRemaining = MaximumJumps;
    }
}
