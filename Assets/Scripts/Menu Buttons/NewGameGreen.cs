using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameGreen : MonoBehaviour
{
    // Called by Green button in UI
    public void OnGreenButtonPressed()
    {
        // Reset saved game data
        PlayerPrefs.DeleteAll();

        // Set frog color to Green
        PlayerPrefs.SetString("FrogColor", "Green");
        PlayerPrefs.Save();

        // Load the main gameplay scene (index 1 in Build Settings)
        SceneManager.LoadScene(1);
    }
}