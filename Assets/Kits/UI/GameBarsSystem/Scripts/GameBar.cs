using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBar : MonoBehaviour
{

    //------- UNITY EDITOR -------//

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

    //------- PRIVATE VARIABLES -------//

    private int _liLypadsCount = 0;
    private int _currentLilypadIndex = 0;

    //------- UNITY METHODS -------//

    void Start()
    {
        _liLypadsCount = Lilypads.Count;
        ResetAllLilypads();
    }

    void OnEnable()
    {
        //Lilypad, checkpoint and fly Collection Event
        Lilypad.OnLilypadCollected += HandleOnLilypadCollected;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
        ParryFly.OnFlyCollected += HandleFlyCollected;

        //Player Reset Event
        Player.OnPlayerReset += HandlePlayerReset;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;

        //Player Jump Event
        Player.OnPlayerJump += HandlePlayerJump;
    }

    void OnDisable()
    {
        //Lilypad, checkpoint and fly Collection Event
        Lilypad.OnLilypadCollected -= HandleOnLilypadCollected;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
        ParryFly.OnFlyCollected -= HandleFlyCollected;

        //Player Reset Event
        Player.OnPlayerReset -= HandlePlayerReset;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;

        //Player Jump Event
        Player.OnPlayerJump -= HandlePlayerJump;

    }

    //------- PRIVATE METHODS -------//
    private void ResetOneLilypad()
    {
        Image img;

        if (_currentLilypadIndex >= 0)
        {
            //Reset current lilypad to default sprite
            img = Lilypads[_currentLilypadIndex].GetComponent<Image>();
            img.sprite = LilypadSprite;
        }

        _currentLilypadIndex++;
        _currentLilypadIndex = Mathf.Clamp(_currentLilypadIndex, 0, _liLypadsCount - 1);

        //New current lilypad to selected sprite
        img = Lilypads[_currentLilypadIndex].GetComponent<Image>();
        img.sprite = SelectedLilypadSprite;

        // Reset restart button sprite (UI Image)
        ResetRestartButton();
    }

    private void ResetAllLilypads()
    {
        for (int i = 0; i < _liLypadsCount; i++)
        {
            Image img = Lilypads[i].GetComponent<Image>();
            img.sprite = LilypadSprite;

            //Set last lilypad to selected sprite
            if (i == _liLypadsCount - 1)
            {
                img.sprite = SelectedLilypadSprite;
            }
        }

        _currentLilypadIndex = _liLypadsCount - 1;

        // Reset restart button sprite (UI Image)
        ResetRestartButton();
    }

    private void ResetRestartButton()
    {
        Image restartImg = RestartButton.GetComponent<Image>();
        restartImg.sprite = RestartButtonSprite;
    }

    //------- HANDLE METHODS -------//

    /// <summary>
    /// Handles lilypad collected events,
    /// </summary>
    private void HandleOnLilypadCollected(Lilypad.LilyPadType type)
    {

        //COMPLETE LILYPAD: Reset all lilypads to default sprite
        if (type == Lilypad.LilyPadType.Complete)
        {
            ResetAllLilypads();
        }
        //SINGLE LILYPAD: Reset only the current lilypad sprite
        else if (type == Lilypad.LilyPadType.Single)
        {
            ResetOneLilypad();
        }
    }

    /// <summary>
    /// Handles checkpoint activated event.
    /// </summary>
    private void HandleCheckpointActivated()
    {
        ResetAllLilypads();
    }

    /// <summary>
    /// Handles fly collected events.
    /// </summary>
    private void HandleFlyCollected()
    {
        ResetOneLilypad();
    }

    /// <summary>
    /// Handles lilypad reset events.
    /// </summary>
    private void HandlePlayerReset()
    {
        HandleOnLilypadCollected(Lilypad.LilyPadType.Complete);
    }


    private void HandlePlayerJump()
    {
        if (_currentLilypadIndex < 0) return;

        //Change current lilypad to used sprite
        Image usedImg = Lilypads[_currentLilypadIndex].GetComponent<Image>();

        usedImg.sprite = UsedLilypadSprite;

        //Move to next lilypad
        _currentLilypadIndex--;
        if (_currentLilypadIndex >= 0)
        {
            //Change next lilypad to selected sprite
            Image nextImg = Lilypads[_currentLilypadIndex].GetComponent<Image>();

            nextImg.sprite = SelectedLilypadSprite;
        }

        //If no lilypads left, change restart button to selected sprite
        if (_currentLilypadIndex < 0)
        {
            Image restartImg = RestartButton.GetComponent<Image>();

            restartImg.sprite = SelectedRestartButtonSprite;
        }
    }
}