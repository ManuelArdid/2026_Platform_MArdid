using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Saw : MonoBehaviour
{
    [SerializeField] private Transform TurnPoint;
    [SerializeField] private float Speed = 5f;

    SpriteRenderer _spriteRenderer;
    Vector3 _startPosition;
    Vector3 _currentTarget;
    Vector3 _lastPosition;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
        _currentTarget = TurnPoint.position;
        _lastPosition = transform.position;
        FlipBasedOnDirection();
    }

    void Update()
    {
        _lastPosition = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, _currentTarget, Speed * Time.deltaTime);

        Vector3 toTarget = _currentTarget - _lastPosition;
        Vector3 moved = transform.position - _lastPosition;

        if (Vector3.Dot(toTarget, moved) <= 0f)
        {
            _currentTarget = _currentTarget == TurnPoint.position ? _startPosition : TurnPoint.position;
            FlipBasedOnDirection();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                player.SendPlayerToSpawnPoint();
            }
        }
    }

    private void FlipBasedOnDirection()
    {
        _spriteRenderer.flipX = _currentTarget.x > transform.position.x;
    }
}
