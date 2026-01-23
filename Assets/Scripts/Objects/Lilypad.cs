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
        Player.OnPlayerReset -= ResetLilypad;
        Player.OnPlayerReset += ResetLilypad;
    }


    void OnDestroy()
    {
        Player.OnPlayerReset -= ResetLilypad;
    }


    //------- Private Methods -------//

    /// <summary>
    /// Resets the lilypad to be active again.
    /// </summary>
    private void ResetLilypad()
    {
        gameObject.SetActive(true);
    }
}
