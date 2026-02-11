using UnityEngine;

public class Lilypad : MonoBehaviour
{

    //------ Events ------//

    public static event System.Action OnLilypadCollected;

    //------- Unity Methods -------//
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnLilypadCollected?.Invoke();

            //Disable lilypad when player lands on it
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        Player.OnPlayerReset += ResetLilypad;
        OnLilypadCollected += ResetLilypad;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
    }


    void OnDestroy()
    {
        Player.OnPlayerReset -= ResetLilypad;
        OnLilypadCollected -= ResetLilypad;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
    }


    //------- Private Methods -------//

    /// <summary>
    /// Resets the lilypad to be active again.
    /// </summary>
    private void ResetLilypad()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Handles checkpoint activated event by resetting the lilypad.
    /// </summary>
    /// <param name="checkpoint">The activated checkpoint.</param>
    private void HandleCheckpointActivated()
    {
        ResetLilypad();
    }
}
