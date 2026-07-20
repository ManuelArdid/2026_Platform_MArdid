using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManualCamera : MonoBehaviour
{
    //-- UNITY EDITOR -----------------------------------------------------------------------//

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera CameraNormal;
    [SerializeField] private CinemachineCamera CameraManual;

    [Header("Input")]
    [SerializeField] private InputActionReference _cameraInput;

    [Header("Settings")]
    [SerializeField] private float _horizontalLimit = 2f;
    [SerializeField] private float _verticalLimit = 2f;
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _returnSpeed = 16f;

    //-- CLASS VARIABLES -------------------------------------------------------------------//

    private Vector2 _input;
    private bool _manualControl;
    private bool _manualCameraActive;

    private Vector3 _basePosition;

    //-- UNITY METHODS ----------------------------------------------------------------------//

    private void Awake()
    {
        CameraNormal.Priority.Value = 10;
        CameraManual.Priority.Value = 0;
    }

    private void LateUpdate()
    {
        if (_manualControl)
        {
            if (!_manualCameraActive)
                ActivateManualCamera();

            Vector3 targetPosition = _basePosition + new Vector3(
                _input.x * _horizontalLimit,
                _input.y * _verticalLimit,
                0f);

            CameraManual.transform.position = Vector3.Lerp(
                CameraManual.transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime);
        }
        else if (_manualCameraActive)
        {
            CameraManual.transform.position = Vector3.Lerp(
                CameraManual.transform.position,
                _basePosition,
                _returnSpeed * Time.deltaTime);

            if (Vector3.Distance(CameraManual.transform.position, _basePosition) < 0.01f)
            {
                CameraManual.transform.position = _basePosition;
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

        _basePosition = current.position;

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