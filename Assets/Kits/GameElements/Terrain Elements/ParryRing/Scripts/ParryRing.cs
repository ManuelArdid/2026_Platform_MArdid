using System;
using UnityEngine;

[UnityEngine.RequireComponent(typeof(Collider2D))]
[UnityEngine.RequireComponent(typeof(SpriteRenderer))]
public class ParryRing : Parryable
{

    //------ Events ------//

    public static event Action OnParryRingCollected;

    //------- Class Variables -------//
    Collider2D _collider2D;
    SpriteRenderer _spriteRenderer;

    //------- Unity Methods -------//
    void Start()
    {
        _collider2D = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
        OnSuccessfulParry += HandleSuccessfulParry;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
        OnSuccessfulParry -= HandleSuccessfulParry;
    }

    private void HandlePlayerReset()
    {
        _collider2D.enabled = true;
        _spriteRenderer.enabled = true;
    }

    private void HandleSuccessfulParry(Parryable parryable)
    {
        if (parryable == this)
        {
            _collider2D.enabled = false;
            _spriteRenderer.enabled = false;
            OnParryRingCollected?.Invoke();
        }
    }
}