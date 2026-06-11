using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBar : MonoBehaviour
{

    //------- UNITY EDITOR -------//

    [SerializeField] private GameObject CurrentPlayer;
    [SerializeField] private GameObject RestartButton;
    [SerializeField] private List<GameObject> KiwisHudSections;

    [Header("Kiwi Images")]
    [SerializeField] private Sprite KiwiSprite;
    [SerializeField] private Sprite SelectedKiwiSprite;
    [SerializeField] private Sprite UsedKiwiSprite;

    [Header("Restart Button Images")]
    [SerializeField] private Sprite RestartButtonSprite;
    [SerializeField] private Sprite SelectedRestartButtonSprite;

    //------- PRIVATE VARIABLES -------//

    private int _liLypadsCount = 0;
    private int _currentKiwiIndex = 0;

    //------- UNITY METHODS -------//

    void Start()
    {
        _liLypadsCount = KiwisHudSections.Count;
        ResetAllKiwis();
    }

    void OnEnable()
    {
        //Kiwi, checkpoint and fly Collection Event
        Kiwi.OnKiwiCollected += HandleOnKiwiCollected;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
        ParryRing.OnParryRingCollected += HandleParryRingCollected;

        //Player Reset Event
        Player.OnPlayerReset += HandlePlayerReset;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;

        //Player Jump Event
        Player.OnPlayerJump += HandlePlayerJump;
    }

    void OnDisable()
    {
        //Kiwi, checkpoint and fly Collection Event
        Kiwi.OnKiwiCollected -= HandleOnKiwiCollected;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
        ParryRing.OnParryRingCollected -= HandleParryRingCollected;

        //Player Reset Event
        Player.OnPlayerReset -= HandlePlayerReset;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;

        //Player Jump Event
        Player.OnPlayerJump -= HandlePlayerJump;

    }

    //------- PRIVATE METHODS -------//
    private void ResetOneKiwi()
    {
        Image img;

        if (_currentKiwiIndex >= 0)
        {
            //Reset current kiwi to default sprite
            img = KiwisHudSections[_currentKiwiIndex].GetComponent<Image>();
            img.sprite = KiwiSprite;
        }

        _currentKiwiIndex++;
        _currentKiwiIndex = Mathf.Clamp(_currentKiwiIndex, 0, _liLypadsCount - 1);

        //New current kiwi to selected sprite
        img = KiwisHudSections[_currentKiwiIndex].GetComponent<Image>();
        img.sprite = SelectedKiwiSprite;

        // Reset restart button sprite (UI Image)
        ResetRestartButton();
    }

    private void ResetAllKiwis()
    {
        for (int i = 0; i < _liLypadsCount; i++)
        {
            Image img = KiwisHudSections[i].GetComponent<Image>();
            img.sprite = KiwiSprite;

            //Set last kiwi to selected sprite
            if (i == _liLypadsCount - 1)
            {
                img.sprite = SelectedKiwiSprite;
            }
        }

        _currentKiwiIndex = _liLypadsCount - 1;

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
    /// Handles kiwi collected events,
    /// </summary>
    private void HandleOnKiwiCollected(Kiwi.KiwiType type)
    {

        //COMPLETE LILYPAD: Reset all kiwis to default sprite
        if (type == Kiwi.KiwiType.Complete)
        {
            ResetAllKiwis();
        }
        //SINGLE LILYPAD: Reset only the current kiwi sprite
        else if (type == Kiwi.KiwiType.Single)
        {
            ResetOneKiwi();
        }
    }

    /// <summary>
    /// Handles checkpoint activated event.
    /// </summary>
    private void HandleCheckpointActivated()
    {
        ResetAllKiwis();
    }

    /// <summary>
    /// Handles fly collected events.
    /// </summary>
    private void HandleParryRingCollected()
    {
        ResetOneKiwi();
    }

    /// <summary>
    /// Handles kiwi reset events.
    /// </summary>
    private void HandlePlayerReset()
    {
        HandleOnKiwiCollected(Kiwi.KiwiType.Complete);
    }


    private void HandlePlayerJump()
    {
        if (_currentKiwiIndex < 0) return;

        //Change current kiwi to used sprite
        Image usedImg = KiwisHudSections[_currentKiwiIndex].GetComponent<Image>();

        usedImg.sprite = UsedKiwiSprite;

        //Move to next kiwi
        _currentKiwiIndex--;
        if (_currentKiwiIndex >= 0)
        {
            //Change next kiwi to selected sprite
            Image nextImg = KiwisHudSections[_currentKiwiIndex].GetComponent<Image>();

            nextImg.sprite = SelectedKiwiSprite;
        }

        //If no kiwis left, change restart button to selected sprite
        if (_currentKiwiIndex < 0)
        {
            Image restartImg = RestartButton.GetComponent<Image>();

            restartImg.sprite = SelectedRestartButtonSprite;
        }
    }
}