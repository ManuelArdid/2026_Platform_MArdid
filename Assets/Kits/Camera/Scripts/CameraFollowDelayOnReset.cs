using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowDelayOnReset : MonoBehaviour
{
    //----------- UNITY EDITOR -----------//
    [Header("References")]
    [SerializeField] private CinemachineCamera CinemachineCamera;

    [Header("Settings")]
    [SerializeField] private float FollowDelay = 0.1f;

    //----------- CLASS VARIABLES -----------//
    private Transform _followTarget;
    private Coroutine _delayCoroutine;

    //----------- UNITY METHODS -----------//
    private void Awake()
    {
        _followTarget = CinemachineCamera.Follow;
    }

    private void OnEnable()
    {
        Player.OnPlayerReset += HandlePlayerReset;
    }

    private void OnDisable()
    {
        Player.OnPlayerReset -= HandlePlayerReset;
    }

    //----------- HANDLER METHODS -----------//

    private void HandlePlayerReset()
    {
        if (_delayCoroutine != null)
            StopCoroutine(_delayCoroutine);

        _delayCoroutine = StartCoroutine(DelayedFollow());
    }

    //----------- COROUTINES -----------//

    private IEnumerator DelayedFollow()
    {
        // Freeze camera follow.
        CinemachineCamera.Follow = null;

        yield return new WaitForSeconds(FollowDelay);

        // Restore camera follow.
        CinemachineCamera.Follow = _followTarget;

        _delayCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Vertical
        Gizmos.DrawLine(
            transform.position + Vector3.up * 100f,
            transform.position + Vector3.down * 100f);

        // Horizontal
        Gizmos.DrawLine(
            transform.position + Vector3.left * 100f,
            transform.position + Vector3.right * 100f);
    }
}