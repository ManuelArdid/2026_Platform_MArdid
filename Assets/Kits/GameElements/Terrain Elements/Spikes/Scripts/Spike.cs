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
                player.PlayerSendToSpawnPoint();
            }
        }
    }
}