using UnityEngine;

public class NewGameButton : MonoBehaviour
{
    public void OnNewGameButtonPressed()
    {
        //DEBUG
        GameManager.Instance.ResetGameData();
        GameManager.Instance.LoadGameScene();        
    }
}