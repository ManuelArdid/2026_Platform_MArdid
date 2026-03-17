using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameRed : MonoBehaviour
{
    // Called by Red button in UI
    public void OnRedButtonPressed()
    {
        // Reset saved game data
        PlayerPrefs.DeleteAll();

        // Set frog color to Red
        PlayerPrefs.SetString("FrogColor", "Red");
        PlayerPrefs.Save();

        // Load the main gameplay scene (index 1 in Build Settings)
        SceneManager.LoadScene(1);
    }
}