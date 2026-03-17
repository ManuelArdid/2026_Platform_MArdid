using UnityEngine;

public class Lilypad : MonoBehaviour
{
    //------- UNITY EDITOR -------//
    [SerializeField] protected LilyPadType Type = LilyPadType.Complete;

    //------ Events ------//

    public static event System.Action<LilyPadType> OnLilypadCollected;

    //------- Enums -------//
    public enum LilyPadType
    {
        Single,
        Complete
    }

    //------- Unity Methods -------//
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnLilypadCollected?.Invoke(Type);

            //Disable lilypad when player lands on it
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
        OnLilypadCollected += HandleLilypadCollected;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
    }


    void OnDestroy()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
        OnLilypadCollected -= HandleLilypadCollected;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
    }


    //------- Private Methods -------//

    /// <summary>
    /// Resets the lilypad to be active again.
    /// </summary>
    private void HandlePlayerReset()
    {
        gameObject.SetActive(true);
    }

    private void HandleLilypadCollected(LilyPadType type)
    {
        if (type == LilyPadType.Complete)
            gameObject.SetActive(true);
    }

    /// <summary>
    /// Handles checkpoint activated event by resetting the lilypad.
    /// </summary>
    /// <param name="checkpoint">The activated checkpoint.</param>
    private void HandleCheckpointActivated()
    {
        gameObject.SetActive(true);
    }
}
