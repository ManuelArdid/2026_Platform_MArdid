using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameGreen : MonoBehaviour
{
    // Called by Green button in UI
    public void OnGreenButtonPressed()
    {
        GameManager.Instance.ResetGameData();
        GameManager.Instance.LoadGameScene();        
    }
}