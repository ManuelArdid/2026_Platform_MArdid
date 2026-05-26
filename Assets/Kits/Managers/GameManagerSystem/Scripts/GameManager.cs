using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Green Charactter config")]
    [SerializeField] private GameObject GreenCharacter;
    [SerializeField] private GameObject GreenCamera;
    [SerializeField] private GameObject GreenGameBar;

   // [Header("Red Charactter config")]
   // [SerializeField] private GameObject RedCharacter;
   // [SerializeField] private GameObject RedCamera;
   // [SerializeField] private GameObject RedGameBar;

    private GameObject _activeCharacter;

    private void Awake()
    {
        string frogColor = PlayerPrefs.GetString("FrogColor");

        //DEBUG: force green
        frogColor = "Green";
        PlayerPrefs.DeleteAll();

        if (frogColor == "Green")
        {
            //Green activation
            GreenCharacter.SetActive(true);
            GreenCamera.SetActive(true);
            GreenGameBar.SetActive(true);

            //Red deactivation
           // RedCharacter.SetActive(false);
           // RedCamera.SetActive(false);
           // RedGameBar.SetActive(false);

            _activeCharacter = GreenCharacter;
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
}