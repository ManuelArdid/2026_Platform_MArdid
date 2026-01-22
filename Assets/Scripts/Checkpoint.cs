using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            if (collision.TryGetComponent<Player>(out var player))
            {
                // Set new spawn point in Player script
                player.SetSpawnPoint(transform.position);

                // Savwe spawn point to PlayerPrefs
                Vector3 pos = transform.position;

                PlayerPrefs.SetFloat("SpawnX", pos.x);
                PlayerPrefs.SetFloat("SpawnY", pos.y);
                PlayerPrefs.SetFloat("SpawnZ", pos.z);
                PlayerPrefs.Save();
            }

            // Destroy checkpoint after activation
            Destroy(gameObject);
        }
    }
}
