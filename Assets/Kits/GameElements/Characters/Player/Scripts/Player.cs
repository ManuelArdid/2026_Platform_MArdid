using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Player : MonoBehaviour
{
    //------- UNITY EDITOR -------//

    [Header("Movement Settings")]
    [SerializeField] protected float MoveSpeed = 5f;
    [SerializeField]
    [Tooltip("This will be multiplied by the move speed (and jump force) and that will be the maximum speed for the character.")]
    protected float MaxSpeedMultiplier = 1.5f;
    [SerializeField] protected float Acceleration = 10f;
    [SerializeField] protected float Deceleration = 60f;

    [Header("Jump Settings")]
    [SerializeField] protected float JumpForce = 10f;
    [Tooltip("Multiplier to apply to upward velocity when jump is released early for variable jump height.")]
    [SerializeField] protected float JumpCutMultiplier = 0.5f;
    [SerializeField] protected float CoyoteTime = 0.15f;
    [SerializeField] protected float KiwiTime = 0.2f;
    [SerializeField] protected int MaximumJumps = 5;
    [SerializeField] protected float AirJumpReduction = 0.5f;
    [SerializeField] protected float JumpCooldown = 0.2f;
    [SerializeField] protected float SkipCoyoteAfterGroundJumpTime = 0.12f;

    [Header("Parry Settings")]
    [SerializeField] protected float ParryWindow = 0.05f;
    [SerializeField] protected float ParryCooldown = 0.1f;
    [SerializeField] protected float ParryJumpMultiplier = 1.5f;

    [Header("Ground Detection")]
    [SerializeField] protected LayerMask GroundLayer;
    [SerializeField] protected Transform GroundCollisionPoint;
    [SerializeField] protected float GroundCheckSize;


    [Header("Input Actions")]
    [SerializeField] protected InputActionReference MovementInputAction;
    [SerializeField] protected InputActionReference JumpInputAction;
    [SerializeField] protected InputActionReference RestartInputAction;

    [Header("Spawn Settings")]
    [SerializeField] protected Transform SpawnPoint;

    [Header("Camera Settings")]
    [SerializeField] protected GameObject CameraFollowGameObject;

    ///------- PUBLIC PROPERTIES -------//
    public bool PlayerIsParrying { get; private set; }

    //------- PROTECTED VARIABLES -------//

    protected Rigidbody2D _rb;
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;

    protected Vector2 _rawMovementInput;
    protected Vector2 _currentVelocity = Vector2.zero;

    protected bool _jumpRequested = false;
    protected bool _isAirJumping = false;
    protected bool _isJumpingAvailable = true;
    protected bool _isParryOnCooldown = false;
    protected bool _canUseCoyoteTime = false;
    protected bool _canReset = false;
    protected bool _moveControlEnabled = true;
    protected bool _isFacingRight = true;

    protected int _jumpsRemaining;
    protected int _airJumpCounter = 0;


    protected float _originalGravityScale;
    private float _ignoreJumpCutTimer = 0f;

    //------- CLASS VARIABLES -------//

    private Coroutine _currentCoyoteTimeCoroutine = null;
    private float _lastGroundedJumpTime = -10f;
    private Vector2 _externalVelocityY = Vector2.zero;
    private float _lastFallSpeed = 0f;
    private Vector2 _lastDirection = Vector2.zero;
    private Vector2 _lastPositionBeforeJump = Vector2.zero;

    //------- EVENTS -------//
    public static event Action OnPlayerReset;
    public static event Action OnPlayerJump;

    //------- UNITY METHODS -----------------------------------------------------------------------------------------------------------------------//

    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalGravityScale = _rb.gravityScale;

        //Initialize jumps
        _jumpsRemaining = MaximumJumps;

    }

    void OnEnable()
    {
        MovementInputAction.action.Enable();
        JumpInputAction.action.Enable();
        RestartInputAction.action.Enable();

        //callbacks
        MovementInputAction.action.performed += Move;
        MovementInputAction.action.canceled += Move;
        MovementInputAction.action.started += Move;

        JumpInputAction.action.started += Jump;
        JumpInputAction.action.canceled += JumpCancelled;

        RestartInputAction.action.performed += Restart;

        //event subscriptions
        Kiwi.OnKiwiCollected += HandleKiwiCollected;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
        Parryable.OnSuccessfulParry += HandleSuccessfulParry;
        ParryRing.OnParryRingCollected += HandleParryRingCollected;
    }

    void OnDisable()
    {
        //callbacks
        MovementInputAction.action.performed -= Move;
        MovementInputAction.action.canceled -= Move;
        MovementInputAction.action.started -= Move;

        JumpInputAction.action.started -= Jump;
        JumpInputAction.action.canceled -= JumpCancelled;

        RestartInputAction.action.performed -= Restart;

        MovementInputAction.action.Disable();
        JumpInputAction.action.Disable();
        RestartInputAction.action.Disable();

        //event unsubscriptions
        Kiwi.OnKiwiCollected -= HandleKiwiCollected;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
        Parryable.OnSuccessfulParry -= HandleSuccessfulParry;
        ParryRing.OnParryRingCollected -= HandleParryRingCollected;

    }

    protected virtual void Update()
    {
        //Max Speed check in x
        if (Mathf.Abs(_rb.linearVelocity.x) > MoveSpeed * MaxSpeedMultiplier)
        {
            _rb.linearVelocity = new Vector2(
                Mathf.Sign(_rb.linearVelocity.x) * MoveSpeed * MaxSpeedMultiplier,
                _rb.linearVelocity.y
            );
        }

        //Max Speed check in y 
        if (Mathf.Abs(_rb.linearVelocity.y) > JumpForce * MaxSpeedMultiplier)
        {
            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                Mathf.Sign(_rb.linearVelocity.y) * JumpForce * MaxSpeedMultiplier
            );
        }

        // Animations
        _animator.SetBool("IsRunning", _currentVelocity.x != 0 && PlayerIsGrounded());
        _animator.SetBool("IsFalling", _rb.linearVelocityY < 0f && !_isAirJumping && !PlayerIsGrounded());
        _animator.SetBool("IsJumping", _rb.linearVelocityY > 0f && !_isAirJumping && !PlayerIsGrounded());

        // Decrement ignore-jump-cut timer (added)
        if (_ignoreJumpCutTimer > 0f)
            _ignoreJumpCutTimer -= Time.deltaTime;

        // Store last direction
        if (_currentVelocity.x != 0)
            _lastDirection = new Vector2(Mathf.Sign(_currentVelocity.x), 0f);

        // Reset last position before jump if grounded
        if (PlayerIsGrounded())
            _lastPositionBeforeJump = transform.position;
    }

    protected virtual void FixedUpdate()
    {
        // Flip sprite
        Turn();


        //RESET CHECK
        if (_jumpsRemaining < 0)
        {
            StartCoroutine(KiwiTimeCoroutine());

            if (_canReset)
                PlayerSendToSpawnPoint();
        }

        HandleMovement();
        HandleJump();

        // Save last fall speed for potential use in springboard or other mechanics
        _lastFallSpeed = _rb.linearVelocityY;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            _canUseCoyoteTime = false;
            _isJumpingAvailable = true;
            _isAirJumping = false;
            _airJumpCounter = 0;

            // Jumping through platforms can cause OnCollisionEnter2D to be called without the player actually landing,
            // so we check the vertical velocity to confirm a landing before resetting jumps and air jump counter
            // if (_rb.linearVelocityY <= 0f)
            // {

            //}
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {

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

    //------- PUBLIC METHODS -----------------------------------------------------------------------------------------------------------------------//
    /// <summary>
    /// Returns the current horizontal velocity of the player.
    /// </summary>
    /// <returns></returns>
    public float PlayerGetCurrentVelocityX()
    {
        return _currentVelocity.x;
    }

    public void PlayerSetExternalVelocityY(Vector2 newVelocity)
    {
        _externalVelocityY = newVelocity;
    }

    /// <summary>
    /// Enables or disables player control.
    /// </summary>
    public void PlayerSetMoveControl(bool enabled)
    {
        _moveControlEnabled = enabled;
    }

    /// <summary>
    /// Checks if player control is enabled.
    /// </summary>
    public bool PlayerIsControlEnabled()
    {
        return _moveControlEnabled;
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
        PlayerStopAllMovement();
        transform.position = SpawnPoint.position;
        _jumpsRemaining = MaximumJumps;
        GameplayCameraController.Instance.CenterCameraOnPlayer();
        OnPlayerReset?.Invoke();
    }

    /// <summary>
    /// Sets the spawn point position for the player.
    /// </summary>
    public void PlayerSetSpawnPoint(Transform spawnPosition)
    {
        if (SpawnPoint == null)
        {
            GameObject temp = new("RuntimeSpawnPoint");
            SpawnPoint = temp.transform;
        }

        SpawnPoint = spawnPosition;
    }

    /// <summary>
    /// Gets the maximum number of jumps for the player.
    /// </summary>
    public int PlayerGetMaximumJumps()
    {
        return MaximumJumps;
    }

    /// <summary>
    /// Gets the current number of jumps remaining for the player.
    /// </summary>
    public float PlayerGetJumpForce()
    {
        return JumpForce;
    }

    /// <summary>
    /// Gets the last fall speed of the player
    /// </summary>
    public float PlayerGetLastFallSpeed()
    {
        return _lastFallSpeed;
    }

    /// <summary>
    /// Gets the last direction the player was moving in.
    /// </summary>
    public Vector2 PlayerGetLastDirection()
    {
        return _lastDirection;
    }

    /// <summary>
    /// Checks if the player is facing right.
    /// </summary>
    /// <returns>True if facing right, otherwise false.</returns>
    public bool PlayerIsFacingRight()
    {
        return _isFacingRight;
    }

    /// <summary>
    /// Checks if the character is grounded.
    /// </summary>
    /// <returns>True if grounded, otherwise false.</returns>
    public bool PlayerIsGrounded()
    {
        return Physics2D.CircleCast(
            GroundCollisionPoint.position,
            GroundCheckSize,
            Vector2.down,
            0,
            GroundLayer
        );
    }

    /// <summary>
    /// Checks if the character is grounded.
    /// </summary>
    /// <returns>True if grounded, otherwise false.</returns>
    public bool PlayerIsJumping()
    {
        return !PlayerIsGrounded() && (transform.position.y > _lastPositionBeforeJump.y);
    }

    /// <summary>
    /// Gets the current jump height of the player.
    /// </summary>
    /// <returns>The current jump height if in the air, otherwise 0.</returns>
    public float PlayerGetCurrentJumpHeight()
    {
        if (PlayerIsGrounded())
            return 0f;

        return transform.position.y - _lastPositionBeforeJump.y;
    }

    /// <summary>
    /// Stops all player movement by resetting velocity and external forces.
    /// </summary>
    public void PlayerStopAllMovement()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _externalVelocityY = Vector2.zero;
        _currentVelocity = Vector2.zero;
        //_rawMovementInput = Vector2.zero;
    }
    //------- PROTECTED METHODS -----------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Performs the jump action with the specified force.
    /// </summary>
    /// <param name="force"></param>
    protected virtual void PerformJump(float force)
    {
        _lastPositionBeforeJump = transform.position;
        _rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Handles character movement based on player input.
    /// </summary>
    protected virtual void Move(InputAction.CallbackContext context)
    {
        _rawMovementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Flips the character's facing direction based on movement input.
    /// </summary>
    /// <returns>True if the character is facing right, otherwise false.</returns>
    protected bool Turn()
    {
        if (_currentVelocity.x > 0)
        {
            _spriteRenderer.flipX = false;
            _isFacingRight = true;
        }

        else if (_currentVelocity.x < 0)
        {
            _spriteRenderer.flipX = true;
            _isFacingRight = false;
        }

        return _isFacingRight;
    }

    //------- PRIVATE METHODS -----------------------------------------------------------------------------------------------------------------------//

    private void Jump(InputAction.CallbackContext context)
    {
        _jumpRequested = true;
    }
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
    private float CalculateAirJumpDivisor()
    {
        return 1f + AirJumpReduction * (_airJumpCounter * _airJumpCounter);
    }

    private void EnableJumpCutIgnore(float duration = 0.1f)
    {
        _ignoreJumpCutTimer = duration;
    }

    private void Restart(InputAction.CallbackContext context)
    {
        GameplayCameraController.Instance.FixCamera();
        PlayerSendToSpawnPoint();
        GameplayCameraController.Instance.UnfixCamera();
    }

    //------- COROUTINES -----------------------------------------------------------------------------------------------------------------------//

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
    /// Kiwi Time Coroutine
    /// </summary>
    private IEnumerator KiwiTimeCoroutine()
    {
        yield return new WaitForSecondsRealtime(KiwiTime);

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
    /// Air Jump Cooldown Coroutine
    /// </summary>
    private IEnumerator JumpCooldownCoroutine()
    {
        _isJumpingAvailable = false;
        yield return new WaitForSecondsRealtime(JumpCooldown);
        _isJumpingAvailable = true;
    }

    //------- EVENT HANDLERS -----------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Handles character movement based on player input, applying acceleration and deceleration for smooth movement.
    /// </summary>
    protected virtual void HandleMovement()
    {
        if (!_moveControlEnabled) return;

        Vector2 targetVelocity = _rawMovementInput * MoveSpeed;

        float currentAcceleration = _rawMovementInput == Vector2.zero
            ? Deceleration
            : Acceleration;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        _rb.linearVelocity = new Vector2(
            _currentVelocity.x + _externalVelocityY.x,
            _rb.linearVelocity.y + _externalVelocityY.y
        );
    }

    /// <summary>
    /// Handles character jump logic.
    /// </summary>
    protected virtual void HandleJump()
    {
        if (!_jumpRequested || !_isJumpingAvailable) return;

        OnPlayerJump?.Invoke();

        _rb.linearVelocityY = 0f;
        _jumpsRemaining--;

        //Ground Jump
        if (PlayerIsGrounded() || _canUseCoyoteTime)
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
        //Aerial Jump
        else
        {
            if (!_isParryOnCooldown)
                // Start parry window
                StartCoroutine(ParryWindowCoroutine());

            _airJumpCounter++;
            PerformJump(JumpForce / CalculateAirJumpDivisor());
            _animator.SetTrigger("PerformAirJump");
            _isAirJumping = true;

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
    /// and air jumpcounter so the player can jump
    /// as high as a normal jump.
    /// </summary>
    private void HandleSuccessfulParry(Parryable parryable)
    {

        //Cancel vertical velocity
        _rb.linearVelocityY = 0f;

        //Apply normal jump force
        PerformJump(JumpForce * ParryJumpMultiplier);

        //Reset air jumps
        _airJumpCounter = 0;

        // If this parry happened during a air jump, enable ignore-cut briefly
        if (_isAirJumping)
            EnableJumpCutIgnore();

        //player is no longer parrying
        PlayerIsParrying = false;
    }

    /// <summary>
    /// Handles kiwi collected event by resetting jumps or adding a jump depending on the type of kiwi collected.
    /// </summary>
    /// <param name="type"></param>
    private void HandleKiwiCollected(Kiwi.KiwiType type)
    {
        _canReset = true;

        if (type == Kiwi.KiwiType.Complete)
            _jumpsRemaining = MaximumJumps;

        else if (type == Kiwi.KiwiType.Single)
        {
            _jumpsRemaining++;
            _jumpsRemaining = Mathf.Clamp(_jumpsRemaining, 0, MaximumJumps);

        }
    }

    /// <summary>
    /// Handles checkpoint activated event by resetting jumps.
    /// </summary>
    private void HandleCheckpointActivated()
    {
        HandleKiwiCollected(Kiwi.KiwiType.Complete);
    }


    /// <summary> Handles parry ring collected event by
    /// adding a jump
    /// </summary>
    private void HandleParryRingCollected()
    {
        _canReset = true;
        _jumpsRemaining++;
        _jumpsRemaining = Mathf.Clamp(_jumpsRemaining, 0, MaximumJumps);
    }

    //------- DEBUG -----------------------------------------------------------------------------------------------------------------------//
    private void OnDrawGizmos()
    {
        if (GroundCollisionPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GroundCollisionPoint.position, GroundCheckSize);
    }

}
