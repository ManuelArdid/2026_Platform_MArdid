using UnityEngine;

public class VictoryScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}