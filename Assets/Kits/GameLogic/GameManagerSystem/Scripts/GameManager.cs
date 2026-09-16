using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    //------- UNITY EDITOR --------------------//
    [Header("Input")]
    [SerializeField] private InputActionReference RestartInputAction;

    //------- UNITY METHODS ------------------//

    void Awake()
    {

        //DEBUG
        //PlayerPrefs.DeleteAll();

    }

    //------- PUBLIC METHODS --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Returns the saved spawn point position if one exists.
    /// </summary>
    /// <param name="spawnPosition"></param>
    /// <returns></returns>
    public bool TryGetSavedSpawnPoint(out Vector3 spawnPosition)
    {
        if (PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY") && PlayerPrefs.HasKey("SpawnZ"))
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            float z = PlayerPrefs.GetFloat("SpawnZ");

            spawnPosition = new Vector3(x, y, z);

            return true;
        }

        spawnPosition = Vector3.zero;

        return false;
    }

    //-- GAMEPLAY------------------///

    /// <summary>
    /// Called by Checkpoint script when player activates a checkpoint. Sets new spawn point in Player script and saves it to PlayerPrefs.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="checkpointPosition"></param>
    public void CheckpointActivated(Player player, Transform checkpointTransform)
    {
        // Set new spawn point in Player script
        player.PlayerSetSpawnPoint(checkpointTransform);

        // Save spawn point to PlayerPrefs
        Vector3 pos = checkpointTransform.position;

        PlayerPrefs.SetFloat("SpawnX", pos.x);
        PlayerPrefs.SetFloat("SpawnY", pos.y);
        PlayerPrefs.SetFloat("SpawnZ", pos.z);
        PlayerPrefs.Save();
    }

    //-- SCENES ------------------///

    /// <summary>
    /// Loads the main menu scene (index 0 in Build Settings).
    /// </summary>
    public void LoadMainMenuScene()
    {
        // Load the main menu scene (index 0 in Build Settings)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Loads the main gameplay scene (index 1 in Build Settings).
    /// </summary>
    public void LoadGameScene()
    {
        // Load the main gameplay scene (index 1 in Build Settings)
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    //-- UI ----------------------//

    //-- INPUT -------------------//

    //-- AUDIO -------------------//

    //-- DATA --------------------//
    public void ResetGameData()
    {
        // Reset saved game data
        PlayerPrefs.DeleteAll();

        // Set frog color to Green
        PlayerPrefs.SetString("FrogColor", "Green");
        PlayerPrefs.Save();
    }

}