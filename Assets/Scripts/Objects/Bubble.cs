using UnityEngine;
public class Bubble : HorizontalMovement
{

    [SerializeField] protected bool Pops = true;

    //------------- CLASS VARIABLES ----------------//
    private bool _playerInside = false;
    private Rigidbody2D _playerRigidbody;
    private float _originalGravityScale;

    //------------- UNITY METHODS ----------------//

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (_playerInside)
        {
            base.Update();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerInside = true;
            collision.gameObject.transform.SetParent(transform);

            //Deactivate player control while inside the bubble
            _playerRigidbody = collision.GetComponent<Rigidbody2D>();
            _originalGravityScale = _playerRigidbody.gravityScale;
            _playerRigidbody.GetComponent<Player>().PlayerSetControl(false);
            _playerRigidbody.GetComponent<Player>().PlayerSetGravityScale(0f);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerInside = false;
            collision.gameObject.transform.SetParent(null);

            //Reactivate player control when exiting the bubble
            if (_playerRigidbody != null)
            {
                _playerRigidbody.GetComponent<Player>().PlayerSetControl(true);
                _playerRigidbody.GetComponent<Player>().PlayerSetGravityScale(_originalGravityScale);
                _playerRigidbody = null;
            }
        }
    }

    //------------- PUBLIC METHODS ----------------//

    /// <summary>
    /// Overrides the TurnBackMethod to destroy the bubble if Pops is true; otherwise, it calls the base method to reverse direction.
    /// </summary>
    override public void TurnBackMethod()
    {
        if (Pops)
        {
            if (_playerRigidbody != null)
            {
                _playerRigidbody.GetComponent<Player>().PlayerSetControl(true);
                _playerRigidbody.GetComponent<Player>().PlayerSetGravityScale(_originalGravityScale);
            }

            //Move bubble back to original position
            transform.position = _startPosition;
            _playerInside = false;
            _playerRigidbody = null;
        }
        else
        {
            base.TurnBackMethod();
        }
    }

}