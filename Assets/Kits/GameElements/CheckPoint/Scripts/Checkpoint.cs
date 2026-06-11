using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class Checkpoint : MonoBehaviour
{

    //------- Private Variables -------//
    Animator _animator;
    Vector3 _originalPosition;

    //------- Events -------//
    public static event System.Action OnCheckpointActivated;

    //------- Unity Methods -------//
    void Awake()
    {
        _originalPosition = transform.position;
        _animator = GetComponent<Animator>();

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnCheckpointActivated?.Invoke();
            // Deactivate checkpoints collider to prevent multiple triggers
            GetComponent<Collider2D>().enabled = false;

            if (collision.TryGetComponent<Player>(out var player))
            {
                GameManager.Instance.CheckpointActivated(player, transform);
            }

            //Animation
            _animator.SetTrigger("PerformActivate");
        }
    }
}
