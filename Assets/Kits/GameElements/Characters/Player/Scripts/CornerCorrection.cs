using UnityEngine;

public class CornerCorrection : MonoBehaviour
{
    //----------- UNITY EDITOR -----------//

    [Header("Player Reference")]
    [SerializeField] GameObject PlayerReference = null;

    [Header("Correction Settings")]
    [SerializeField] private float CorrectionDistance = 0.15f;

    [Header("Raycast Settings")]
    [SerializeField] private float RaycastDistance = 1f;
    [SerializeField] private float LeftRaycastXOffset = -0.1f;
    [SerializeField] private float LeftRaycastYOffset = 0.1f;
    [SerializeField] private float RightRaycastXOffset = 0.1f;
    [SerializeField] private float RightRaycastYOffset = 0.1f;
    [SerializeField] private float CenterBoxWidth = 0.2f;
    [SerializeField] private float CenterBoxHeight = 1f;
    [SerializeField] private float CenterBoxYOffset = 0f;

    //----------- PRIVATE VARIABLES -----------//
    private Transform _playerTransform;
    private RaycastHit2D _hitLeft;
    private RaycastHit2D _hitRight;
    private RaycastHit2D _hitCenter;

    //----------- UNITY METHODS -----------//
    void Start()
    {
        _playerTransform = PlayerReference.transform;
    }

    void Update()
    {
        PrepareRaycasts();
    }

    void OnEnable()
    {
        Player.OnPlayerJump += HandleOnPlayerJump;
    }

    void OnDisable()
    {
        Player.OnPlayerJump -= HandleOnPlayerJump;
    }

    //----------- PRIVATE METHODS -----------//
    private void PrepareRaycasts()
    {
        _hitLeft = Physics2D.Raycast(_playerTransform.position + new Vector3(LeftRaycastXOffset, LeftRaycastYOffset), Vector2.up, RaycastDistance, LayerMask.GetMask("Ground"));
        _hitRight = Physics2D.Raycast(_playerTransform.position + new Vector3(RightRaycastXOffset, RightRaycastYOffset), Vector2.up, RaycastDistance, LayerMask.GetMask("Ground"));
        _hitCenter = Physics2D.BoxCast(_playerTransform.position + new Vector3((LeftRaycastXOffset + RightRaycastXOffset) * 0.5f, (LeftRaycastYOffset + RightRaycastYOffset) * 0.5f + CenterBoxYOffset), new Vector2(CenterBoxWidth, CenterBoxHeight), 0f, Vector2.up, 0f, LayerMask.GetMask("Ground"));
    }

    private void CorrectPlayerPosition()
    {
        if (_hitCenter.collider)
        {
            return;
        }

        if (_hitLeft.collider != null)
        {
            _playerTransform.position += new Vector3(CorrectionDistance, 0f, 0f);
        }
        else if (_hitRight.collider != null)
        {
            _playerTransform.position -= new Vector3(CorrectionDistance, 0f, 0f);
        }
    }

    //----------- EVENT HANDLERS -----------//
    private void HandleOnPlayerJump()
    {
        CorrectPlayerPosition();
    }

    //----------- DEBUG -----------//
    private void OnDrawGizmos()
    {
        if (PlayerReference == null) return;

        Vector3 leftOrigin = PlayerReference.transform.position +
                             new Vector3(LeftRaycastXOffset, LeftRaycastYOffset);

        Vector3 rightOrigin = PlayerReference.transform.position +
                              new Vector3(RightRaycastXOffset, RightRaycastYOffset);

        Vector3 centerOrigin = PlayerReference.transform.position +
                               new Vector3((LeftRaycastXOffset + RightRaycastXOffset) * 0.5f,
                                           (LeftRaycastYOffset + RightRaycastYOffset) * 0.5f + CenterBoxYOffset);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector3.up * RaycastDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector3.up * RaycastDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centerOrigin, new Vector3(CenterBoxWidth, CenterBoxHeight, 0f));

        // Draw small spheres at the raycast origins
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(leftOrigin, 0.03f);
        Gizmos.DrawSphere(rightOrigin, 0.03f);
        Gizmos.DrawSphere(centerOrigin, 0.03f);
    }
}