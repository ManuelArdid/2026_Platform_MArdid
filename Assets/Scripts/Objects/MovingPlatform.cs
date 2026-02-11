using UnityEngine;

public class MovingPlatform : HorizontalMovement
{
    [Header("Platform options")]
    [SerializeField] private bool UnparentOnExit = true;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Attach the player to the platform so it moves together
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Detach the player when leaving the platform
        if (UnparentOnExit && collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.parent == transform)
                collision.transform.SetParent(null);
        }
    }
}
