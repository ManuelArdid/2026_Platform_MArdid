using UnityEngine;
public class Saw : HorizontalMovement
{
    //------ CLASS VARIABLES ------//
    private SpriteRenderer _spriteRenderer;

    //------ UNITY METHODS ------//
    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        FlipBasedOnDirection();
    }

    protected override void Update()
    {
        base.Update();
        FlipBasedOnDirection();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                player.PlayerSendToSpawnPoint();
            }
        }
    }

    private void FlipBasedOnDirection()
    {
        _spriteRenderer.flipX = _currentTarget.x > transform.position.x;
    }

}
