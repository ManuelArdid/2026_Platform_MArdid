using System;
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
    [SerializeField] protected float CoyoteTime = 0.15f;
    [SerializeField] protected float LilypadTime = 0.2f;
    [SerializeField] protected int MaximumJumps = 5;
    [SerializeField] protected float DoubleJumpReduction = 0.5f;
    [SerializeField] protected float JumpCooldown = 0.2f;
    [SerializeField] protected float SkipCoyoteAfterGroundJumpTime = 0.12f;

    [Header("Parry Settings")]
    [SerializeField] protected float ParryWindow = 0.05f;
    [SerializeField] protected float ParryCooldown = 0.1f;
    [SerializeField] protected float ParryJumpMultiplier = 1.5f;

    [Header("Ground Detection")]
    [SerializeField] protected LayerMask GroundLayer;
    [SerializeField] protected Transform GroundCollisionPoint;

    [Header("Input Actions")]
    [SerializeField] protected InputActionReference MovementInputAction;
    [SerializeField] protected InputActionReference JumpInputAction;

    [Header("Spawn Settings")]
    [SerializeField] protected Transform SpawnPoint;

    //------ Events ------//
    public static event Action OnPlayerReset;
    public static event Action OnPlayerJump;

    //------- Private Variables -------//
    private Coroutine _currentCoyoteTimeCoroutine = null;

    // Added private timestamp to avoid re-enabling coyote immediately after a grounded jump
    private float _lastGroundedJumpTime = -10f;

    //------- Protected Variables -------//
    protected Rigidbody2D _rb;
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;

    protected Vector2 _rawMovementInput;
    protected Vector2 _currentVelocity = Vector2.zero;

    protected bool _jumpRequested = false;
    protected bool _onPlatform = false;
    protected bool _isDoubleJumping = false;
    protected bool _isJumpingAvailable = true;
    protected bool _isParryOnCooldown = false;
    protected bool _canUseCoyoteTime = false;
    protected bool _canReset = false;
    protected bool _controlEnabled = true;

    protected int _jumpsRemaining;
    protected int _doubleJumpCounter = 0;


    protected float _originalGravityScale;

    // Added: timer to ignore jump cut for a short duration after a parry
    private float _ignoreJumpCutTimer = 0f;

    ///------- Public Properties -------//
    public bool PlayerIsParrying { get; private set; }

    //------- Unity Methods -------//
    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalGravityScale = _rb.gravityScale;

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
        _animator.SetBool("IsFalling", _rb.linearVelocityY < 0f && !_isDoubleJumping && !IsGrounded());
        _animator.SetBool("IsJumping", _rb.linearVelocityY > 0f && !_isDoubleJumping && !IsGrounded());


        // Flip sprite
        if (_currentVelocity.x > 0)
            _spriteRenderer.flipX = false;
        else if (_currentVelocity.x < 0)
            _spriteRenderer.flipX = true;

        // Decrement ignore-jump-cut timer (added)
        if (_ignoreJumpCutTimer > 0f)
            _ignoreJumpCutTimer -= Time.deltaTime;
    }

    protected virtual void FixedUpdate()
    {
        //RESET CHECK
        if (_jumpsRemaining < 0)
        {
            StartCoroutine(LilypadTimeCoroutine());

            if (_canReset)
                PlayerSendToSpawnPoint();
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
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
        Parryable.OnSuccessfulParry += HandleSuccessfulParry;
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
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
        Parryable.OnSuccessfulParry -= HandleSuccessfulParry;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _onPlatform = true;
            _isDoubleJumping = false;
            _canUseCoyoteTime = false;
            _doubleJumpCounter = 0;
            _isJumpingAvailable = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _onPlatform = false;

            // Avoid re-enabling coyote time immediately after a grounded jump
            if (Time.time - _lastGroundedJumpTime < SkipCoyoteAfterGroundJumpTime)
                return;

            // Start Coyote Time Coroutine if component is enabled
            if (enabled)
            {
                if (_currentCoyoteTimeCoroutine != null)
                    StopCoroutine(_currentCoyoteTimeCoroutine);

                _currentCoyoteTimeCoroutine = StartCoroutine(CoyoteTimeCoroutine());
            }
        }
    }

    //------- Public Methods -------//

    /// <summary>
    /// Enables or disables player control.
    /// </summary>
    public void PlayerSetControl(bool enabled)
    {
        _controlEnabled = enabled;
    }

    /// <summary>
    /// Checks if player control is enabled.
    /// </summary>
    public bool PlayerIsControlEnabled()
    {
        return _controlEnabled;
    }

    /// <summary>
    /// Sets the gravity scale of the player's Rigidbody2D.
    /// </summary>
    public void PlayerSetGravityScale(float scale)
    {
        _rb.gravityScale = scale;
    }

    /// <summary>
    /// Sets the gravity scale of the player's Rigidbody2D.
    /// </summary>
    public void PlayerResetGravityScale()
    {
        _rb.gravityScale = _originalGravityScale;
    }

    /// <summary>
    /// Sends the player back to the spawn point and resets jumps.
    /// </summary>
    public void PlayerSendToSpawnPoint()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = SpawnPoint.position;
        _jumpsRemaining = MaximumJumps;
        OnPlayerReset?.Invoke();
    }

    /// <summary>
    /// Sets the spawn point position for the player.
    /// </summary>
    public void PlayerSetSpawnPoint(Vector3 spawnPosition)
    {
        if (SpawnPoint == null)
        {
            GameObject temp = new GameObject("RuntimeSpawnPoint");
            SpawnPoint = temp.transform;
        }

        SpawnPoint.position = spawnPosition;
    }

    /// <summary>
    /// Gets the maximum number of jumps for the player.
    /// </summary>
    public int PlayerGetMaximumJumps()
    {
        return MaximumJumps;
    }

    //------- Protected Methods -------//
    /// <summary>
    /// Handles character movement based on player input.
    /// </summary>
    protected virtual void Move(InputAction.CallbackContext context)
    {
        _rawMovementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Performs the jump action with the specified force.
    /// </summary>
    /// <param name="force"></param>
    protected virtual void PerformJump(float force)
    {
        _rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Checks if the character is grounded.
    /// </summary>
    /// <returns>True if grounded, otherwise false.</returns>
    protected bool IsGrounded()
    {
        return Physics2D.CircleCast(
            GroundCollisionPoint.position,
            0.15f,
            Vector2.down,
            0,
            GroundLayer
        );
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
            // If we recently enabled ignore-cut (e.g. parry occurred), consume it and skip the cut
            if (_ignoreJumpCutTimer > 0f)
            {
                _ignoreJumpCutTimer = 0f;
                return;
            }

            _rb.linearVelocityY *= JumpCutMultiplier;
        }
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
    private void HandleLilypadCollected()
    {
        _canReset = true;
        _jumpsRemaining = MaximumJumps;
    }

    /// <summary>
    /// Handles checkpoint activated event.
    /// Resets jumps remaining.
    /// </summary>
    private void HandleCheckpointActivated()
    {
        HandleLilypadCollected();
    }

    /// <summary>
    /// Enables ignore-jump-cut for a short duration.
    /// </summary>
    /// <param name="duration">Duration to ignore jump cut.</param>
    private void EnableJumpCutIgnore(float duration = 0.1f)
    {
        _ignoreJumpCutTimer = duration;
    }

    //------- COROUTINES -------//

    /// <summary>
    /// Coyote Time Coroutine
    /// </summary>
    private IEnumerator CoyoteTimeCoroutine()
    {
        _canUseCoyoteTime = true;

        yield return new WaitForSecondsRealtime(CoyoteTime);

        _canUseCoyoteTime = false;
    }

    /// <summary>
    /// Lilypad Time Coroutine
    /// </summary>
    private IEnumerator LilypadTimeCoroutine()
    {
        yield return new WaitForSecondsRealtime(LilypadTime);

        if (_jumpsRemaining < 0)
            _canReset = true;
    }

    /// <summary>
    /// Parry Window Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator ParryWindowCoroutine()
    {
        PlayerIsParrying = true;
        yield return new WaitForSecondsRealtime(ParryWindow);
        PlayerIsParrying = false;
    }

    /// <summary>
    /// Parry Cooldown Coroutine
    /// </summary>
    private IEnumerator ParryCooldownCoroutine()
    {
        _isParryOnCooldown = true;
        yield return new WaitForSecondsRealtime(ParryCooldown);
        _isParryOnCooldown = false;
    }

    /// <summary>
    /// Double Jump Cooldown Coroutine
    /// </summary>
    private IEnumerator JumpCooldownCoroutine()
    {
        _isJumpingAvailable = false;
        yield return new WaitForSecondsRealtime(JumpCooldown);
        _isJumpingAvailable = true;
    }

    //------- EVENT HANDLERS -------//

    protected virtual void HandleMovement() { }

    /// <summary>
    /// Handles character jump logic.
    /// </summary>
    protected virtual void HandleJump()
    {
        if (!_jumpRequested || !_isJumpingAvailable) return;

        OnPlayerJump?.Invoke();

        _rb.linearVelocityY = 0f;
        _jumpsRemaining--;

        if (IsGrounded() || _canUseCoyoteTime)
        {
            _canUseCoyoteTime = false;

            // Evita que OnCollisionExit2D vuelva a abrir coyote inmediatamente
            _lastGroundedJumpTime = Time.time;
            if (_currentCoyoteTimeCoroutine != null)
            {
                StopCoroutine(_currentCoyoteTimeCoroutine);
                _currentCoyoteTimeCoroutine = null;
            }

            PerformJump(JumpForce);
        }
        else
        {
            if (!_isParryOnCooldown)
                // Start parry window
                StartCoroutine(ParryWindowCoroutine());

            _doubleJumpCounter++;
            PerformJump(JumpForce / CalculateDoubleJumpDivisor());
            _animator.SetTrigger("PerformDoubleJump");
            _isDoubleJumping = true;

            // Start jump cooldown
            StartCoroutine(JumpCooldownCoroutine());

            if (!_isParryOnCooldown)
                // Start parry cooldown
                StartCoroutine(ParryCooldownCoroutine());
        }

        _jumpRequested = false;
    }

    /// <summary>
    /// Handles successful parry event by resetting jumps
    /// and double jumpcounter so the player can jump
    /// as high as a normal jump.
    /// </summary>
    private void HandleSuccessfulParry(Parryable parryable)
    {

        //Cancel vertical velocity
        _rb.linearVelocityY = 0f;

        //Apply normal jump force
        PerformJump(JumpForce * ParryJumpMultiplier);

        //Reset air jumps
        _doubleJumpCounter = 0;

        // If this parry happened during a double jump, enable ignore-cut briefly
        if (_isDoubleJumping)
            EnableJumpCutIgnore();

        //player is no longer parrying
        PlayerIsParrying = false;
    }
}
