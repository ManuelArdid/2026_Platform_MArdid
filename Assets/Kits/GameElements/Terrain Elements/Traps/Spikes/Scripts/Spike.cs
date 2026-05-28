using UnityEngine;

public class Spike : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != null)
                {

                    //only if pl is going down on the spikes, not if he is jumping up through them
                    if (player.GetComponent<Rigidbody2D>().linearVelocity.y > 0)
                        return;

                    player.PlayerSendToSpawnPoint();
                }
            }
        }
    }
}