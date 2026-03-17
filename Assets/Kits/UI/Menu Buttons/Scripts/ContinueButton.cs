using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{

    //------ UNITY METHODS ------//
    private void Start()
    {
        //saved game data exists
        if (PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY") && PlayerPrefs.HasKey("SpawnZ"))
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnContinueButtonPressed()
    {
        // Load the main scene
        SceneManager.LoadScene(1);
    }
}