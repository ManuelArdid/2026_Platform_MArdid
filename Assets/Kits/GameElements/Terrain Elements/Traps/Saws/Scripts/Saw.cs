using UnityEngine;
public class Saw : HorizontalMovement
{

    [SerializeField]
    private float RotationSpeed = 180f;

    //------ CLASS VARIABLES ------//
    private SpriteRenderer _spriteRenderer;

    //------ UNITY METHODS ------//
    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        FlipBasedOnDirection();
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, -RotationSpeed * Time.deltaTime);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
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
