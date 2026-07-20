using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowObject : MonoBehaviour
{
    //------- UNITY EDITOR --------------------------------------------------------//

    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Manual Camera Offset")]
    [SerializeField] private float _manualHorizontalLimit = 2f;
    [SerializeField] private float _manualVerticalLimit = 2f;
    [SerializeField] private float _manualMoveSpeed = 8f;
    [SerializeField] private float _manualReturnSpeed = 12f;
    [SerializeField] private InputActionReference _cameraInput;
    

    //------- CLASS VARIABLES ---------------------------------------------------//

    private Vector2 _cameraInputValue;

    private bool _isCameraControlled;

    private Vector3 _savedPosition;

    //------- UNITY METHODS ------------------------------------------------------//

    private void Update()
    {
        if (!_isCameraControlled)
        {
            _savedPosition = _playerTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (_isCameraControlled)
        {
            Vector3 targetPosition =
                _savedPosition +
                new Vector3(
                    _cameraInputValue.x * _manualHorizontalLimit,
                    _cameraInputValue.y * _manualVerticalLimit,
                    0f);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                _manualMoveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                _savedPosition,
                _manualReturnSpeed * Time.deltaTime);
        }
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

    //------- INPUT CALLBACKS ----------------------------------------------------//

    private void OnCameraStarted(InputAction.CallbackContext context)
    {        
        _isCameraControlled = true;

        _cameraInputValue = context.ReadValue<Vector2>();
    }

    private void OnCameraPerformed(InputAction.CallbackContext context)
    {
        _cameraInputValue = context.ReadValue<Vector2>();
    }

    private void OnCameraCanceled(InputAction.CallbackContext context)
    {
        _cameraInputValue = Vector2.zero;
        _isCameraControlled = false;
    }

}