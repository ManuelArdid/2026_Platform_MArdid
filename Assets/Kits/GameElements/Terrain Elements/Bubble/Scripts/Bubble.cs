using UnityEngine;
using UnityEngine.InputSystem;

public class Bubble : MovingElement
{
    [Header("Bubble Settings")]
    [SerializeField] protected bool Pops = true;
    [SerializeField] protected InputActionReference JumpInputAction;
    [SerializeField] protected InputActionReference ResetInputAction;

    //------------- CLASS VARIABLES ----------------//
    private bool _playerInside = false;
    private GameObject _player;
    private Player _playerController;
    private Rigidbody2D _playerRigidbody;
    private RigidbodyType2D _originalBodyType;

    //------------- UNITY METHODS ----------------//

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {

        if (_playerInside)
        {
            base.FixedUpdate();
        }
    }

    private void OnEnable()
    {
        if (JumpInputAction != null)
        {
            JumpInputAction.action.started += HandleJump;
            ResetInputAction.action.started += HandleReset;
        }
    }

    private void OnDisable()
    {
        if (JumpInputAction != null)
        {
            JumpInputAction.action.started -= HandleJump;
            ResetInputAction.action.started -= HandleReset;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AbductPlayer(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ReleasePlayer();
        }
    }

    //------------- PROTECTED METHODS ----------------//

    /// <summary>
    /// Overrides the TurnBackMethod to destroy the bubble if Pops is true; otherwise, it calls the base method to reverse direction.
    /// </summary>
    protected override void TurnBackMethod()
    {
        if (Pops)
        {
            Pop();
        }
        else
        {
            base.TurnBackMethod();
        }
    }

    //-------------- PRIVATE METHODS ----------------//

    private void Pop()
    {
        if (!_playerInside) return;
        
        ReleasePlayer();
        ResetTurningPointsIndex();
        // Move bubble back to original position
        transform.position = _startPosition;

    }

    private void AbductPlayer(GameObject player)
    {
        _playerRigidbody = player.GetComponent<Rigidbody2D>();
        _playerController = player.GetComponent<Player>();

        if (_playerRigidbody == null || _playerController == null)
        {
            Debug.LogError($"Bubble: Missing required components on {player.name}.");
            return;
        }

        // Stop any existing movement
        _playerController.PlayerStopAllMovement();

        // Save and change body type
        _originalBodyType = _playerRigidbody.bodyType;
        _playerRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // Set the player as a child of the bubble
        _playerInside = true;
        _player = player;
        _player.transform.SetParent(transform);
        _player.transform.localPosition = Vector3.zero;

        // Disable player control
        _playerController.PlayerSetMoveControl(false);

        // Fix camera
        GameplayCameraController.Instance.FixCamera();
    }

    private void ReleasePlayer()
    {
        if (!_playerInside)
            return;

        if (_player != null)
        {
            _player.transform.SetParent(null);
        }

        if (_playerController != null)
        {
            _playerController.PlayerSetMoveControl(true);
        }

        if (_playerRigidbody != null)
        {
            _playerRigidbody.bodyType = _originalBodyType;
        }

        _player = null;
        _playerController = null;
        _playerRigidbody = null;
        _playerInside = false;

        // Unfix camera
        GameplayCameraController.Instance.UnfixCamera();
    }


    //------------- INPUT HANDLERS ----------------//
    private void HandleJump(InputAction.CallbackContext context)
    {
        Pop();
    }

    private void HandleReset(InputAction.CallbackContext context)
    {
        Pop();
    }
}