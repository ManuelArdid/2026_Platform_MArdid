using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //------- UNITY EDITOR --------------------//

    [Header("Green Charactter config")]
    [SerializeField] private GameObject Character;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject GameBar;

    //---- CLASS VARIABLES -------------------//

    private GameObject _activeCharacter;

    //------- UNITY METHODS ------------------//

    private void Awake()
    {
        string frogColor = PlayerPrefs.GetString("FrogColor");

        //DEBUG: force green
        frogColor = "Green";
        PlayerPrefs.DeleteAll();

        if (frogColor == "Green")
        {
            //Green activation
            Character.SetActive(true);
            Camera.SetActive(true);
            GameBar.SetActive(true);

            //Red deactivation
            // RedCharacter.SetActive(false);
            // RedCamera.SetActive(false);
            // RedGameBar.SetActive(false);

            _activeCharacter = Character;
        }
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

            _activeCharacter.GetComponent<Player>().PlayerSetSpawnPoint(newTransform);

            _activeCharacter.GetComponent<Player>().PlayerSendToSpawnPoint();
        }
    }

    //------- PUBLIC METHODS --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//


    //-- GAMEPLAY------------------///

    /// <summary>
    /// Called by Checkpoint script when player activates a checkpoint. Sets new spawn point in Player script and saves it to PlayerPrefs.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="checkpointPosition"></param>
    public void CheckpointActivated(Player player, Vector3 checkpointPosition)
    {
        // Set new spawn point in Player script
        player.PlayerSetSpawnPoint(transform);

        // Save spawn point to PlayerPrefs
        Vector3 pos = transform.position;

        PlayerPrefs.SetFloat("SpawnX", checkpointPosition.x);
        PlayerPrefs.SetFloat("SpawnY", checkpointPosition.y);
        PlayerPrefs.SetFloat("SpawnZ", checkpointPosition.z);
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