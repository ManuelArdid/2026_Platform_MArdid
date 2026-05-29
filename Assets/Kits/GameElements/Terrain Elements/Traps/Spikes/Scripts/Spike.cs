using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] protected bool PointingDown = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != null)
                {
                    //Inverted Spikes
                    if (PointingDown)
                    {
                        if (player.GetComponent<Rigidbody2D>().linearVelocity.y < 0)
                            return;

                    }

                    //Regular Spikes
                    else
                    {
                        if (player.GetComponent<Rigidbody2D>().linearVelocity.y > 0)
                            return;

                    }

                    player.PlayerSendToSpawnPoint();
                }
            }
        }
    }
}