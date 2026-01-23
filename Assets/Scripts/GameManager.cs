using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Green Charactter config")]
    [SerializeField] private GameObject GreenCharacter;
    [SerializeField] private GameObject GreenCamera;
    [SerializeField] private GameObject GreenGameBar;

    [Header("Red Charactter config")]
    [SerializeField] private GameObject RedCharacter;
    [SerializeField] private GameObject RedCamera;
    [SerializeField] private GameObject RedGameBar;

    private GameObject _activeCharacter;

    private void Start()
    {
        string frogColor = PlayerPrefs.GetString("FrogColor");

        if (frogColor == "Green")
        {
            //Green activation
            GreenCharacter.SetActive(true);
            GreenCamera.SetActive(true);
            GreenGameBar.SetActive(true);

            //Red deactivation
            RedCharacter.SetActive(false);
            RedCamera.SetActive(false);
            RedGameBar.SetActive(false);

            _activeCharacter = GreenCharacter;
        }
        else if (frogColor == "Red")
        {
            //Red activation
            RedCharacter.SetActive(true);
            RedCamera.SetActive(true);
            RedGameBar.SetActive(true);

            //Green deactivation
            GreenCharacter.SetActive(false);
            GreenCamera.SetActive(false);
            GreenGameBar.SetActive(false);

            _activeCharacter = RedCharacter;
        }

        if (PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY") && PlayerPrefs.HasKey("SpawnZ"))
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            float z = PlayerPrefs.GetFloat("SpawnZ");

            _activeCharacter.transform.position = new Vector3(x, y, z);
        }
    }
}