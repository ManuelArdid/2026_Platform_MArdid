using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void OnQuitButtonPressed()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}