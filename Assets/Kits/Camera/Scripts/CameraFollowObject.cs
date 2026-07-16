using System.Collections;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _lookAheadDistance = 0.75f;
    [SerializeField] private float _lookAheadSpeed = 8f;

    private Player _player;

    private void Awake()
    {
        _player = _playerTransform.gameObject.GetComponent<Player>();
    }

    private void LateUpdate()
    {
        float offset = _player.PlayerIsFacingRight()
            ? _lookAheadDistance
            : -_lookAheadDistance;

        Vector3 targetPosition = _playerTransform.position;
        targetPosition.x += offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            _lookAheadSpeed * Time.deltaTime);
    }   
}