using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //------- UNITY EDITOR --------------------//

    [Header("Player Character config")]
    [SerializeField] private GameObject Character;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject GameBar;

    //------- UNITY METHODS ------------------//

    private void Awake()
    {
        //DEBUG
        PlayerPrefs.DeleteAll();

    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY") && PlayerPrefs.HasKey("SpawnZ"))
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            float z = PlayerPrefs.GetFloat("SpawnZ");

            Transform newTransform = new GameObject("TempSpawnPoint").transform;
            newTransform.position = new Vector3(x, y, z);

            Character.GetComponent<Player>().PlayerSetSpawnPoint(newTransform);

            Character.GetComponent<Player>().PlayerSendToSpawnPoint();
        }
    }

    //------- PUBLIC METHODS --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    ///  Sets the player character, camera, and game bar references in the GameManager. Called by Player script when player character is initialized. This allows GameManager to manage player-related functionality and UI elements throughout the game.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="camera"></param>
    /// <param name="gameBar"></param>
   // public void SetPlayerCharacter(GameObject character, GameObject camera, GameObject gameBar)
   // {
   //     Character = character;
   //     Camera = camera;
   //     GameBar = gameBar;
   // }

    //-- GAMEPLAY------------------///

    /// <summary>
    /// Activates the player character, camera, and game bar. Called by Player script when player character is initialized. This allows the player to start playing the game after the character has been set up and references have been assigned in the GameManager.
    /// </summary>
    public void ActivatePlayableCharacter()
    {
        Character.SetActive(true);
        Camera.SetActive(true);
        GameBar.SetActive(true);
    }

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

    //-- UI ----------------------///

    //-- INPUT -------------------///

    //-- AUDIO -------------------///

    //-- DATA --------------------///
    public void ResetGameData()
    {
        // Reset saved game data
        PlayerPrefs.DeleteAll();

        // Set frog color to Green
        PlayerPrefs.SetString("FrogColor", "Green");
        PlayerPrefs.Save();
    }

}