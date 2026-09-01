using UnityEngine;

public class NewGameGreen : MonoBehaviour
{
    public void OnNewGameButtonPressed()
    {
        GameManager.Instance.ResetGameData();
        GameManager.Instance.LoadGameScene();        
    }
}