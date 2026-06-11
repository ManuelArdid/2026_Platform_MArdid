using UnityEngine;
public class Saw : MonoBehaviour
{

    [SerializeField]
    private float RotationSpeed = 180f;

    //------ UNITY METHODS ------//

    void Update()
    {
        transform.Rotate(0f, 0f, -RotationSpeed * Time.deltaTime);
    }

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
