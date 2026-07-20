using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayCameraController : MonoBehaviour
{
    //-- UNITY EDITOR -----------------------------------------------------------------------//

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera CameraNormal;
    [SerializeField] private CinemachineCamera CameraManual;
    [SerializeField] private Player TargetObject;

    [Header("Input")]
    [SerializeField] private InputActionReference _cameraInput;

    [Header("Settings")]
    [SerializeField] private float _horizontalLimit = 2f;
    [SerializeField] private float _verticalLimit = 2f;
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _returnSpeed = 16f;

    [Header("Auto Camera Offset")]
    [SerializeField] private float _lookAheadOffset = 0.55f;
    [SerializeField] private float _lookAheadSpeed = 6f;

    //-- CLASS VARIABLES -------------------------------------------------------------------//

    private Vector2 _input;
    private bool _manualControl;
    private bool _manualCameraActive;
    private Vector3 _cameraOffset;
    private float _currentLookAhead;
    private CinemachinePositionComposer _normalComposer;
    private bool _lastFacingRight;

    //-- UNITY METHODS ----------------------------------------------------------------------//

    private void Awake()
    {
        CameraNormal.Priority.Value = 10;
        CameraManual.Priority.Value = 0;

        _normalComposer = CameraNormal.GetComponent<CinemachinePositionComposer>();
        _lastFacingRight = TargetObject.PlayerIsFacingRight();
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = TargetObject.transform.position + _cameraOffset;

        //LOOKAHEAD BY CHANGING CINEMACHINE OFFSET
        if (!_manualCameraActive)
        {
            bool facingRight = TargetObject.PlayerIsFacingRight();

            if (facingRight != _lastFacingRight)
            {
                _currentLookAhead = 0f;
                _lastFacingRight = facingRight;
            }

            float targetLookAhead = facingRight
                ? _lookAheadOffset
                : -_lookAheadOffset;

            _currentLookAhead = Mathf.Lerp(
                _currentLookAhead,
                targetLookAhead,
                _lookAheadSpeed * Time.deltaTime);

            Vector3 offset = _normalComposer.TargetOffset;
            offset.x = _currentLookAhead;
            _normalComposer.TargetOffset = offset;
        }

        //MANUAL CAMERA CONTROL
        if (_manualControl)
        {
            if (!_manualCameraActive)
                ActivateManualCamera();

            Vector3 desiredPosition = targetPosition + new Vector3(
                _input.x * _horizontalLimit,
                _input.y * _verticalLimit,
                0f);

            CameraManual.transform.position = Vector3.Lerp(
                CameraManual.transform.position,
                desiredPosition,
                _moveSpeed * Time.deltaTime);
        }

        // RETURN TO NORMAL CAMERA POSITION
        else if (_manualCameraActive)
        {
            CameraManual.transform.position = Vector3.MoveTowards(
                CameraManual.transform.position,
                targetPosition,
                _returnSpeed * Time.deltaTime);

            if (Vector3.Distance(CameraManual.transform.position, targetPosition) < 0.01f)
            {
                CameraManual.transform.position = targetPosition;
                DeactivateManualCamera();
            }
        }
    }

    private void ActivateManualCamera()
    {
        _manualCameraActive = true;

        // Copia exactamente la cámara que está renderizando
        Transform current = Camera.main.transform;

        CameraManual.transform.SetPositionAndRotation(
            current.position,
            current.rotation);

        // Guarda el offset entre la cámara y el objetivo
        _cameraOffset = current.position - TargetObject.transform.position;

        CameraManual.Priority.Value = 20;
    }

    private void DeactivateManualCamera()
    {
        _manualCameraActive = false;

        // Update the priorities to switch back to the normal camera.
        CameraManual.Priority.Value = 0;
    }

    private void OnEnable()
    {
        _cameraInput.action.Enable();

        _cameraInput.action.started += OnCameraStarted;
        _cameraInput.action.performed += OnCameraPerformed;
        _cameraInput.action.canceled += OnCameraCanceled;
    }

    private void OnDisable()
    {
        _cameraInput.action.started -= OnCameraStarted;
        _cameraInput.action.performed -= OnCameraPerformed;
        _cameraInput.action.canceled -= OnCameraCanceled;

        _cameraInput.action.Disable();
    }

    //-- CAMERA INPUT HANDLERS --------------------------------------------------------------//

    private void OnCameraStarted(InputAction.CallbackContext context)
    {
        _manualControl = true;
        _input = context.ReadValue<Vector2>();
    }

    private void OnCameraPerformed(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    private void OnCameraCanceled(InputAction.CallbackContext context)
    {
        _manualControl = false;
        _input = Vector2.zero;
    }
}