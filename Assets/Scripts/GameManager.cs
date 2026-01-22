using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject greenCharacter;
    [SerializeField] private GameObject redCharacter;
    [SerializeField] private GameObject greenCamera;
    [SerializeField] private GameObject redCamera;

    private GameObject _activeCharacter;

    private void Start()
    {
        string frogColor = PlayerPrefs.GetString("FrogColor");

        if (frogColor == "Green")
        {
            greenCharacter.SetActive(true);
            redCharacter.SetActive(false);
            greenCamera.SetActive(true);
            redCamera.SetActive(false);

            _activeCharacter = greenCharacter;
        }
        else if (frogColor == "Red")
        {
            greenCharacter.SetActive(false);
            redCharacter.SetActive(true);
            greenCamera.SetActive(false);
            redCamera.SetActive(true);

            _activeCharacter = redCharacter;
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