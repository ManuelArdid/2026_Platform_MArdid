using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingElement : MonoBehaviour, IResetable
{

    //------ UNITY EDITOR ------//

    [Header("Movement Source")]
    [SerializeField] protected bool UseDirectionAndDistance = false;
    [SerializeField] protected Transform[] TurningPoints = null;
    [SerializeField] protected Vector2 InitialDirection = Vector2.right;
    [SerializeField] protected float TravelDistance = 5f;

    [Header("Movement")]
    [SerializeField] protected float Speed = 5f;
    [SerializeField] protected bool TurnBack = true;

    //------ PROTECTED VARIABLES ------//

    protected Rigidbody2D _rb;

    protected int _currentTurningPointIndex;
    protected Vector3 _startPosition;
    protected Vector3 _currentTarget;
    protected Vector3 _calculatedTarget;
    protected Vector3 _lastPosition;

    //------ UNITY METHODS --------------------------------------------------------------------------------------------------------------------//

    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;

        _startPosition = transform.position;
        _lastPosition = _startPosition;

        InitializeTarget();
    }

    protected virtual void FixedUpdate()
    {
        _lastPosition = transform.position;

        Move();

        CheckIfReachedTarget();
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandleOnPlayerReset;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandleOnPlayerReset;
    }

    //------ PROTECTED METHODS -------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Moves the object towards the current target.
    /// </summary>
    protected virtual void Move()
    {
        Vector3 newPosition = Vector3.MoveTowards(
            _rb.position,
            _currentTarget,
            Speed * Time.fixedDeltaTime);

        _rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Calculates the initial movement target.
    /// </summary>
    protected virtual void InitializeTarget()
    {
        if (UseDirectionAndDistance)
        {
            Vector2 direction = InitialDirection.sqrMagnitude > 0f
                ? InitialDirection.normalized
                : Vector2.right;

            _calculatedTarget = _startPosition + (Vector3)(direction * TravelDistance);
            _currentTarget = _calculatedTarget;
            return;
        }

        if (TurningPoints != null && TurningPoints.Length > 0)
        {
            _currentTarget = TurningPoints[0].position;
            return;
        }

        _currentTarget = _startPosition;
        Debug.LogWarning($"{name}: No TurningPoints assigned and Direction+Distance is disabled.");
    }

    /// <summary>
    /// Checks if the object reached the current target.
    /// </summary>
    protected virtual void CheckIfReachedTarget()
    {
        const float arriveThreshold = 0.01f;

        if (Vector2.Distance(_rb.position, _currentTarget) > arriveThreshold)
            return;

        if (!UseDirectionAndDistance &&
            TurningPoints != null &&
            _currentTurningPointIndex < TurningPoints.Length - 1)
        {
            _currentTurningPointIndex++;
            _currentTarget = TurningPoints[_currentTurningPointIndex].position;
            return;
        }

        if (TurnBack)
            TurnBackMethod();
    }

    /// <summary>
    /// Changes the movement target to the opposite end.
    /// </summary>
    protected virtual void TurnBackMethod()
    {
        if (UseDirectionAndDistance)
        {
            _currentTarget = _currentTarget == _calculatedTarget
                ? _startPosition
                : _calculatedTarget;

            return;
        }

        if (TurningPoints == null || TurningPoints.Length == 0)
            return;

        _currentTarget = _currentTarget == TurningPoints[0].position
            ? _startPosition
            : TurningPoints[0].position;
    }

    protected virtual void ResetTurningPointsIndex()
    {
        if (UseDirectionAndDistance)
        {
            _currentTarget = _calculatedTarget;
            return;
        }

        if (TurningPoints == null || TurningPoints.Length == 0)
        {
            _currentTarget = _startPosition;
            return;
        }

        _currentTurningPointIndex = 0;
        _currentTarget = TurningPoints[0].position;
    }

    //------ HANDLE METHODS ------//
    public void HandleOnPlayerReset()
    {
        Reset();
    }

    //------ INTERFACE IMPLEMENTATION ------//
    public void Reset()
    {
        transform.position = _startPosition;
        _lastPosition = _startPosition;
        ResetTurningPointsIndex();
    }
}