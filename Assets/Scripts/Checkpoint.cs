using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class Checkpoint : MonoBehaviour
{

    //------- Private Variables -------//
    Animator _animator;

    //------- Unity Methods -------//
    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
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

            // Deactivate checkpoints collider to prevent multiple triggers
            GetComponent<Collider2D>().enabled = false;

            //Animation
            _animator.SetTrigger("PerformActivate");
        }
    }
}
