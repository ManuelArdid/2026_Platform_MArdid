using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject greenCharacter;
    [SerializeField] private GameObject redCharacter;
    [SerializeField] private GameObject greenCharacterUtils;
    [SerializeField] private GameObject redCharacterUtils;

    private GameObject _activeCharacter;

    private void Start()
    {
        string frogColor = PlayerPrefs.GetString("FrogColor");

        if (frogColor == "Green")
        {
            greenCharacter.SetActive(true);
            redCharacter.SetActive(false);
            greenCharacterUtils.SetActive(true);
            redCharacterUtils.SetActive(false);

            _activeCharacter = greenCharacter;
        }
        else if (frogColor == "Red")
        {
            greenCharacter.SetActive(false);
            redCharacter.SetActive(true);
            greenCharacterUtils.SetActive(false);
            redCharacterUtils.SetActive(true);

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