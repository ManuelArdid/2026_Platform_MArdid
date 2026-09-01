using System;
using UnityEngine;

public class Collectable : MonoBehaviour, IResetable
{
    //------- CLASS VARIABLES -------//
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;

    private bool _collected = false;

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
            _collected = true;
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
    /// associated checkpoint is activated and if the Big Kiwi has been collected.
    /// </summary>
    private void HandleCheckpointActivated()
    {
        if (_collected)
        {
            StartCoroutine(GeneralUtils.Instance.DeactivateAfterDelay(gameObject, 0.1f));
        }
    }

    /// <summary>
    /// Handles player reset event by resetting the Big Kiwi state.
    /// </summary>
    private void HandlePlayerReset()
    {
        Reset();
    }

    //------- INTERFACE IMPLEMENTATION -------//
    public void Reset()
    {
        _collected = false;
        _spriteRenderer.enabled = true;
        _collider2D.enabled = true;
    }
}