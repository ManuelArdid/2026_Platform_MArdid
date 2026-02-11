using System;
using UnityEngine;

public class BigFly : MonoBehaviour
{
    //------- CLASS VARIABLES -------//
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;

    private bool _bigFlyCollected = false;

    //------- EVENTS -------//
    public static event Action OnBigFlyCollected;

    //------- UNITY METHODS -------//

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _bigFlyCollected = true;
            _spriteRenderer.enabled = false;
            _collider2D.enabled = false;
        }
    }

    void OnEnable()
    {
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
        Player.OnPlayerReset += HandlePlayerReset;
    }

    void OnDisable()
    {
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
        Player.OnPlayerReset -= HandlePlayerReset;
    }

    //------- PRIVATE METHODS -------//

    /// <summary>
    /// Handles checkpoint activated event by checking if the
    /// associated checkpoint is activated and if the Big Fly has been collected.
    /// </summary>
    private void HandleCheckpointActivated()
    {
        if (_bigFlyCollected)
        {
            OnBigFlyCollected?.Invoke();
            StartCoroutine(GeneralUtils.Instance.DeactivateAfterDelay(gameObject, 0.1f));
        }
    }

    /// <summary>
    /// Handles player reset event by resetting the Big Fly state.
    /// </summary>
    private void HandlePlayerReset()
    {
        _bigFlyCollected = false;
        _spriteRenderer.enabled = true;
        _collider2D.enabled = true;
    }

}