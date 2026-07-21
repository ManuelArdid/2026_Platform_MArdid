using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class FallingPlatform : MonoBehaviour
{
    //------ UNITY EDITOR ------//
    [SerializeField] private float FallDelay = 0.5f; // Time before the platform starts falling after being stepped on
    [SerializeField] private float DeactivationDelay = 1.5f; // Time before the platform is deactivated after falling
    [SerializeField] private float GravityScale = 5f; // Gravity scale for the falling platform
    //------ CLASS VARIABLES ------//
    private Rigidbody2D _rb;
    private Vector3 _originalPosition;
    private SpriteRenderer[] _spriteRenderers;
    private Collider2D[] _colliders;
    private Coroutine _currentFallAfterDelayCoroutine;
    private Coroutine _currentHideSpriteAfterDelayCoroutine;

    //------ UNITY METHODS ------//
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalPosition = transform.position;
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        _colliders = GetComponentsInChildren<Collider2D>(true);
    }

    void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
    }

    void OnDisable()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.CompareTag("Player"))
        {

            //Fall after a delay
            if (_currentFallAfterDelayCoroutine != null)
                StopCoroutine(_currentFallAfterDelayCoroutine);
            _currentFallAfterDelayCoroutine = StartCoroutine(FallAfterDelay(FallDelay));

            //Hide the sprite and stop falling after a delay
            if (_currentHideSpriteAfterDelayCoroutine != null) StopCoroutine(_currentHideSpriteAfterDelayCoroutine);
            _currentHideSpriteAfterDelayCoroutine = StartCoroutine(HideSpriteAfterDelay(FallDelay + DeactivationDelay));
        }
    }

    //------ PRIVATE METHODS ------//
    private void Fall()
    {
        if (_rb != null)
        {
            //Make it fall
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = GravityScale;

            //Deactivate the collider to prevent further interactions
            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }
        }
    }

    //------ COROUTINES ------//
    private System.Collections.IEnumerator FallAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Fall();
    }

    private System.Collections.IEnumerator HideSpriteAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        foreach (var sprite in _spriteRenderers)
        {
            sprite.enabled = false;
        }

        StopFalling();
    }

    private void StopFalling()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
    }

    //------ EVENT HANDLERS ------//
    private void HandlePlayerReset()
    {
        StopAllCoroutines();
        StopFalling();

        foreach (var collider in _colliders)
        {
            collider.enabled = true;
        }

        foreach (var sprite in _spriteRenderers)
        {
            sprite.enabled = true;
        }

        transform.position = _originalPosition;
    }
}
