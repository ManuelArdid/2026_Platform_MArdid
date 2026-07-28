using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] protected bool PointingDown = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<Player>(out var player))
            {
                float playerYSpeed = collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity.y;


                if (!PointingDown && playerYSpeed < 0.01f)
                {
                    player.PlayerSendToSpawnPoint();
                }
                else if (PointingDown && playerYSpeed > 0.01f)
                {
                    player.PlayerSendToSpawnPoint();

                }
            }
        }
    }
}