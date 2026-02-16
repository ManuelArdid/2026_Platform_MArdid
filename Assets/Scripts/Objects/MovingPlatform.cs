using UnityEngine;

public class MovingPlatform : HorizontalMovement
{
    //------ UNITY EDITOR ---------//   
    [Header("Platform options")]
    [SerializeField] private bool UnparentOnExit = true;

    //------ CLASS VARIABLES ------//
    Player _playerOnPlatform = null;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform = collision.gameObject.GetComponent<Player>();

            Vector2 direction = (_currentTarget - transform.position).normalized;
            Vector2 externalVelocity = direction * Speed;

            _playerOnPlatform.PlayerSetExternalVelocityY(externalVelocity);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Detach the player when leaving the platform
        if (UnparentOnExit && collision.gameObject.CompareTag("Player"))
        {
            _playerOnPlatform.PlayerSetExternalVelocityY(Vector2.zero);
            _playerOnPlatform = null;
        }
    }
}
