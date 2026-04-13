using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class Lever : MonoBehaviour
{
    //------------ UNITY EDITOR ------------//
    [Header("General Settings")]
    [SerializeField] protected float snapThreshold = 0.5f;
    [SerializeField] protected float rotationSpeed = 180f;

    [Header("Turn Limits")]
    [SerializeField] protected bool ApplyTurnLimits = false;
    [SerializeField] protected int MaxRightTurns;
    [SerializeField] protected int MaxLeftTurns;

    //------------ CLASS FIELDS ------------//
    private Rigidbody2D _rb;
    private float _originalRotation;
    private bool _moving = false;
    private float _targetAngle;
    private float _motorDirection = 1f;
    private int _turnsRight = 0;
    private int _turnsLeft = 0;


    //------------ UNITY METHODS ------------//

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalRotation = _rb.rotation;
    }

    void OnEnable()
    {
        Player.OnPlayerReset += ResetLever;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= ResetLever;
    }

    void FixedUpdate()
    {
        if (!_moving) return;

        // Calculate the new angle towards the target angle, considering rotation speed and time
        float newAngle = Mathf.MoveTowardsAngle(
            _rb.rotation,
            _targetAngle,
            rotationSpeed * Time.fixedDeltaTime
        );

        // Apply the new angle to the Rigidbody2D
        _rb.MoveRotation(newAngle);

        // Check if we've reached the target angle within the snap threshold, and if so, snap to it and stop moving
        if (Mathf.Abs(Mathf.DeltaAngle(_rb.rotation, _targetAngle)) <= snapThreshold)
        {
            _rb.MoveRotation(_targetAngle);
            _moving = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryActivate(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryActivate(collision);
    }

    void TryActivate(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _moving) return;
        Vector2 r = transform.InverseTransformPoint(collision.transform.position);
        _motorDirection = (r.x * r.y >= 0) ? -1f : 1f;

        if (ApplyTurnLimits)
        {
            if (_motorDirection > 0 && _turnsRight >= MaxRightTurns) return;
            if (_motorDirection < 0 && _turnsLeft >= MaxLeftTurns) return;

            if (_motorDirection > 0)
            {
                _turnsRight++;
                _turnsLeft--;
            }
            else
            {
                _turnsLeft++;
                _turnsRight--;
            }
        }

        float currentAngle = _rb.rotation;
        float baseAngle = Mathf.Round(currentAngle / 90f) * 90f;

        _targetAngle = baseAngle + (90f * _motorDirection);
        _moving = true;
    }

    //------------ PUBLIC METHODS ------------//
    public void ResetLever()
    {
        _rb.MoveRotation(_originalRotation);

        _turnsRight = 0;
        _turnsLeft = 0;
    }


    //----------- DEBUG -----------//
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        float angle = Application.isPlaying && _rb != null ? _rb.rotation : transform.eulerAngles.z;

        while (angle > 360)
        {
            angle -= 360;
        }

        while (angle < -360)
        {
            angle += 360;
        }

        Handles.Label(
            transform.position + Vector3.up * 0.5f,
            angle.ToString("F1") + "°"
        );
    }
#endif
}