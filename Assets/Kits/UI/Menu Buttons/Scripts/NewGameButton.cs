using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameGreen : MonoBehaviour
{
    public void OnNewGameButtonPressed()
    {
        GameManager.Instance.ResetGameData();
        GameManager.Instance.LoadGameScene();        
    }
}