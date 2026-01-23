using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class GameBar : MonoBehaviour
{
    [SerializeField] private GameObject CurrentPlayer;
    [SerializeField] private GameObject RestartButton;
    [SerializeField] private List<GameObject> Lilypads;

    [Header("Lilypad Images")]
    [SerializeField] private Sprite LilypadSprite;
    [SerializeField] private Sprite SelectedLilypadSprite;
    [SerializeField] private Sprite UsedLilypadSprite;

    [Header("Restart Button Images")]
    [SerializeField] private Sprite RestartButtonSprite;
    [SerializeField] private Sprite SelectedRestartButtonSprite;

    //------- Private Variables -------//

    private int _liLypadsCount = 0;
    private int _currentLilypadIndex = 0;
    //------- Unity Methods -------//


    void Start()
    {
        _liLypadsCount = Lilypads.Count;
        ResetLilypads();
    }

    void OnEnable()
    {
        //Lilypad Collection Event
        Lilypad.OnLilypadCollected += HandleLilypadsReset;

        //Player Reset Event
        Player.OnPlayerReset += HandleLilypadsReset;

        //Player Jump Event
        Player.OnPlayerJump += HandlePlayerJump;
    }

    void OnDisable()
    {
        //Lilypad Collection Event
        Lilypad.OnLilypadCollected -= HandleLilypadsReset;

        //Player Reset Event
        Player.OnPlayerReset -= HandleLilypadsReset;

        //Player Jump Event
        Player.OnPlayerJump -= HandlePlayerJump;

    }

    /// <summary>
    /// Resets lilypad index and sprites to the initial state.
    /// </summary>
    private void ResetLilypads()
    {
        // Reset lilypad index
        _currentLilypadIndex = _liLypadsCount - 1;

        // Reset lilypad sprites (UI Images)
        for (int i = 0; i < _liLypadsCount; i++)
        {
            Image img = Lilypads[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = (i == _currentLilypadIndex) ? SelectedLilypadSprite : LilypadSprite;
            }
            else
            {
                Debug.LogWarning($"[GameBar] Lilypad at index {i} has no Image component.");
            }
        }

        // Reset restart button sprite (UI Image)
        Image restartImg = RestartButton.GetComponent<Image>();
        if (restartImg != null)
        {
            restartImg.sprite = RestartButtonSprite;
        }
        else
        {
            Debug.LogWarning("[GameBar] RestartButton has no Image component.");
        }
    }

    private void HandleLilypadsReset()
    {
        ResetLilypads();
    }

    private void HandlePlayerJump()
    {
        if (_currentLilypadIndex < 0) return;

        //Change current lilypad to used sprite
        Image usedImg = Lilypads[_currentLilypadIndex].GetComponent<Image>();
        if (usedImg != null)
        {
            usedImg.sprite = UsedLilypadSprite;
        }

        //Move to next lilypad
        _currentLilypadIndex--;
        if (_currentLilypadIndex >= 0)
        {
            //Change next lilypad to selected sprite
            Image nextImg = Lilypads[_currentLilypadIndex].GetComponent<Image>();
            if (nextImg != null)
            {
                nextImg.sprite = SelectedLilypadSprite;
            }
        }

        //If no lilypads left, change restart button to selected sprite
        if (_currentLilypadIndex < 0)
        {
            Image restartImg = RestartButton.GetComponent<Image>();
            if (restartImg != null)
            {
                restartImg.sprite = SelectedRestartButtonSprite;
            }
        }
    }
}