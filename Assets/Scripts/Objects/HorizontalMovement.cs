using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HorizontalMovement : MonoBehaviour
{
    [Header("Movement source (choose one)")]
    [SerializeField] protected Transform TurningPoint;              // Optional: used if Direction+Distance is disabled
    [SerializeField] protected bool UseDirectionAndDistance = false;
    [SerializeField] protected Vector2 InitialDirection = Vector2.right;
    [SerializeField] protected float TravelDistance = 5f;

    [Header("Movement")]
    [SerializeField] protected float Speed = 5f;
    [SerializeField] protected bool TurnBack = true;

    //------ PROTECTED VARIABLES ------//
    protected Vector3 _startPosition;
    protected Vector3 _currentTarget;
    protected Vector3 _lastPosition;
    protected Vector3 _calculatedTarget;

    protected Rigidbody2D _rb;

    //------ UNITY METHODS ------//
    protected virtual void Start()
    {
        _startPosition = transform.position;
        _rb = GetComponent<Rigidbody2D>();

        // Decide how the target is calculated
        if (UseDirectionAndDistance)
        {
            Vector2 dir = InitialDirection.sqrMagnitude == 0f ? Vector2.right : InitialDirection.normalized;
            _calculatedTarget = _startPosition + (Vector3)dir * TravelDistance;
            _currentTarget = _calculatedTarget;
        }
        else if (TurningPoint != null)
        {
            _currentTarget = TurningPoint.position;
        }
        else
        {
            _currentTarget = _startPosition;
            Debug.LogWarning($"{name}: No TurningPoint assigned and Direction+Distance is disabled.");
        }

        _lastPosition = transform.position;
    }

    protected virtual void FixedUpdate()
    {
        _lastPosition = transform.position;

        // Move towards the current target
        Vector2 newPosition = Vector2.MoveTowards(
            _rb.position,
            _currentTarget,
            Speed * Time.fixedDeltaTime
        );

        _rb.MovePosition(newPosition);


        CheckIfReachedTargetAndTurn();
    }

    //------------- PUBLIC METHODS ----------------//
    /// <summary>
    /// Reverses movement direction between start and target.
    /// </summary>
    public virtual void TurnBackMethod()
    {
        if (UseDirectionAndDistance)
        {
            _currentTarget = _currentTarget == _calculatedTarget
                ? _startPosition
                : _calculatedTarget;
        }
        else if (TurningPoint != null)
        {
            _currentTarget = _currentTarget == TurningPoint.position
                ? _startPosition
                : TurningPoint.position;
        }
    }

    //------------- PROTECTED METHODS ----------------//
    /// <summary>
    /// Checks if the object reached the target and turns back if needed.
    /// </summary>
    protected virtual void CheckIfReachedTargetAndTurn()
    {
        const float arriveThreshold = 0.01f;

        // Distance-based check for better stability
        if (TurnBack && Vector2.Distance(_rb.position, _currentTarget) <= arriveThreshold)
        {
            TurnBackMethod();
        }
    }
}
