using UnityEngine;

public class Kiwi : MonoBehaviour
{
    //------- UNITY EDITOR -------//
    [SerializeField] protected KiwiType Type = KiwiType.Complete;

    //------ Events ------//

    public static event System.Action<KiwiType> OnKiwiCollected;

    //------- Enums -------//
    public enum KiwiType
    {
        Single,
        Complete
    }

    //------- Unity Methods -------//
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnKiwiCollected?.Invoke(Type);

            //Disable kiwi when player lands on it
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
        OnKiwiCollected += HandleKiwiCollected;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
    }


    void OnDestroy()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
        OnKiwiCollected -= HandleKiwiCollected;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
    }


    //------- Private Methods -------//

    /// <summary>
    /// Resets the kiwi to be active again.
    /// </summary>
    private void HandlePlayerReset()
    {
        gameObject.SetActive(true);
    }

    private void HandleKiwiCollected(KiwiType type)
    {
        if (type == KiwiType.Complete)
            gameObject.SetActive(true);
    }

    /// <summary>
    /// Handles checkpoint activated event by resetting the kiwi.
    /// </summary>
    /// <param name="checkpoint">The activated checkpoint.</param>
    private void HandleCheckpointActivated()
    {
        gameObject.SetActive(true);
    }
}
