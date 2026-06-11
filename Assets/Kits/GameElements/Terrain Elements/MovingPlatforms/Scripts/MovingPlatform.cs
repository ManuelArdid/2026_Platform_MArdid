using UnityEngine;

public class MovingPlatform : HorizontalMovement
{
    //------ UNITY EDITOR ---------//   
    [Header("Platform options")]
    [SerializeField] private bool UnparentOnExit = true;

    //------ CLASS VARIABLES ------//
    Player _playerOnPlatform = null;
    Vector2 _externalVelocity;

    //------ UNITY METHODS ------//
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform = collision.gameObject.GetComponent<Player>();

            UpdatePlayerVelocity();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && _playerOnPlatform != null)
        {
            UpdatePlayerVelocity();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Detach the player when leaving the platform
        if (UnparentOnExit && collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform.PlayerSetExternalVelocityY(Vector2.zero);
            _playerOnPlatform = null;
        }
    }

    //------ PRIVATE METHODS ------//
    private void UpdatePlayerVelocity()
    {
        if (_playerOnPlatform != null)
        {
            Vector2 direction = (_currentTarget - transform.position).normalized;
            _externalVelocity = direction * Speed;

            _playerOnPlatform.PlayerSetExternalVelocityY(_externalVelocity);
        }
    }
}
