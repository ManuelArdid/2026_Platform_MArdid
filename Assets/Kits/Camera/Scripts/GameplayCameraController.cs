using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayCameraController : MonoBehaviour
{
    //-- UNITY EDITOR -----------------------------------------------------------------------//

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera CameraNormal;
    [SerializeField] private CinemachineCamera CameraManual;
    [SerializeField] private Player TargetPlayer;

    [Header("Input")]
    [SerializeField] private InputActionReference CameraInput;

    [Header("Settings")]
    [SerializeField] private float HorizontalLimit = 2f;
    [SerializeField] private float VerticalLimit = 2f;
    [SerializeField] private float MoveSpeed = 8f;
    [SerializeField] private float ReturnSpeed = 16f;

    [Header("Auto Camera Offset")]
    [SerializeField] private float LookAheadOffset = 0.55f;
    [SerializeField] private float LookAheadSpeed = 6f;

    [Header("Auto Zoomout")]
    [SerializeField] private bool CameraAutoZoomDisabled = false;
    [SerializeField] private float CameraAutoZoomOutHeightTreshold = 5f;
    [SerializeField] private float CameraAutoZoomOutSpeed = 5f;
    [SerializeField] private float CameraAutoZoomInSpeed = 10f;

    //-- CLASS VARIABLES -------------------------------------------------------------------//

    private Vector2 _input;

    private Vector3 _cameraOffset;


    private bool _manualControl;
    private bool _manualCameraActive;
    private bool _lastFacingRight;
    private bool _originalAutoZoomOption;

    private float _currentLookAhead;
    private float _originalOrtogrpaphicSize;
    private float _temporalAutoZoomDeactivationTime = 1f;

    private int _mediumPriority = 10;
    private int _highPriority = 20;

    private CinemachinePositionComposer _normalComposer;

    private Coroutine _delayCoroutine;

    //-- UNITY METHODS ----------------------------------------------------------------------//

    private void Awake()
    {
        // Set the initial priorities of the cameras
        CameraNormal.Priority.Value = _mediumPriority;
        CameraManual.Priority.Value = 0;

        // Get the CinemachinePositionComposer component from the normal camera
        _normalComposer = CameraNormal.GetComponent<CinemachinePositionComposer>();
        _lastFacingRight = TargetPlayer.PlayerIsFacingRight();

        // Store the original orthographic sizes of the cameras
        _originalOrtogrpaphicSize = CameraNormal.GetComponent<CinemachineCamera>().Lens.OrthographicSize;

        // Store the original selection state of the auto zoom feature
        _originalAutoZoomOption = CameraAutoZoomDisabled;

        if (_originalOrtogrpaphicSize != CameraManual.GetComponent<CinemachineCamera>().Lens.OrthographicSize)
        {
            Debug.LogWarning("The original orthographic sizes of the normal and manual cameras are not equal. This may cause issues with camera zooming.");
        }
    }

    private void LateUpdate()
    {
        //LOOKAHEAD BY CHANGING CINEMACHINE OFFSET
        HandleCameraLookAhead();

        //HANDLE CAMERA SELECTION BETWEEN MANUAL AND AUTOMATIC
        HandleCameraSelection();

        //INCREASE THE CAMERA SIZE PROPORTIONALLY TO THE PLAYER'S JUMP HEIGHT
        if (!CameraAutoZoomDisabled)
            HandleCameraZoom();

    }

    private void OnEnable()
    {
        CameraInput.action.Enable();

        CameraInput.action.started += OnCameraStarted;
        CameraInput.action.performed += OnCameraPerformed;
        CameraInput.action.canceled += OnCameraCanceled;

        Player.OnPlayerReset += HandleOnPlayerReset;
    }

    private void OnDisable()
    {
        CameraInput.action.started -= OnCameraStarted;
        CameraInput.action.performed -= OnCameraPerformed;
        CameraInput.action.canceled -= OnCameraCanceled;

        Player.OnPlayerReset -= HandleOnPlayerReset;

        CameraInput.action.Disable();
    }


    //-- PUBLIC METHODS ---------------------------------------------------------------------//
    public CinemachineCamera GetActiveCamera()
    {
        return _manualCameraActive ? CameraManual : CameraNormal;
    }

    //-- CAMERA CONTROL PRIVATE METHODS ------------------------------------------------------------//
    private void ActivateManualCamera()
    {
        _manualCameraActive = true;

        // Copia exactamente la cámara que está renderizando
        Transform current = Camera.main.transform;

        CameraManual.transform.SetPositionAndRotation(
            current.position,
            current.rotation);

        // Guarda el offset entre la cámara y el objetivo
        _cameraOffset = current.position - TargetPlayer.transform.position;

        CameraManual.Priority.Value = _highPriority;
    }

    private void DeactivateManualCamera()
    {
        _manualCameraActive = false;

        // Update the priorities to switch back to the normal camera.
        CameraManual.Priority.Value = 0;
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

    private void HandleCameraZoom()
    {
        if (!TargetPlayer.PlayerIsGrounded())
        {
            float jumpHeight = TargetPlayer.PlayerGetCurrentJumpHeight();
            jumpHeight = Mathf.Abs(jumpHeight); // Ensure jumpHeight is positive

            if (jumpHeight > CameraAutoZoomOutHeightTreshold)
            {

                float newSize = _originalOrtogrpaphicSize + jumpHeight * 0.5f;

                GetActiveCamera().GetComponent<CinemachineCamera>().Lens.OrthographicSize = Mathf.Lerp(
                    GetActiveCamera().GetComponent<CinemachineCamera>().Lens.OrthographicSize,
                    newSize,
                    Time.deltaTime * CameraAutoZoomOutSpeed);
            }
        }
        else
        {
            GetActiveCamera().GetComponent<CinemachineCamera>().Lens.OrthographicSize = Mathf.Lerp(
                GetActiveCamera().GetComponent<CinemachineCamera>().Lens.OrthographicSize,
                _originalOrtogrpaphicSize,
                Time.deltaTime * CameraAutoZoomInSpeed);
        }
    }

    private void HandleCameraSelection()
    {
        Vector3 targetPosition = TargetPlayer.transform.position + _cameraOffset;


        //MANUAL CAMERA CONTROL
        if (_manualControl)
        {
            if (!_manualCameraActive)
                ActivateManualCamera();

            Vector3 desiredPosition = targetPosition + new Vector3(
                _input.x * HorizontalLimit,
                _input.y * VerticalLimit,
                0f);

            CameraManual.transform.position = Vector3.Lerp(
                CameraManual.transform.position,
                desiredPosition,
                MoveSpeed * Time.deltaTime);
        }

        // RETURN TO NORMAL CAMERA POSITION
        else if (_manualCameraActive)
        {
            CameraManual.transform.position = Vector3.MoveTowards(
                CameraManual.transform.position,
                targetPosition,
                ReturnSpeed * Time.deltaTime);

            if (Vector3.Distance(CameraManual.transform.position, targetPosition) < 0.01f)
            {
                CameraManual.transform.position = targetPosition;
                DeactivateManualCamera();
            }
        }
    }

    private void HandleCameraLookAhead()
    {
        if (!_manualCameraActive)
        {
            bool facingRight = TargetPlayer.PlayerIsFacingRight();

            if (facingRight != _lastFacingRight)
            {
                _currentLookAhead = 0f;
                _lastFacingRight = facingRight;
            }

            float targetLookAhead = facingRight
                ? LookAheadOffset
                : -LookAheadOffset;

            _currentLookAhead = Mathf.Lerp(
                _currentLookAhead,
                targetLookAhead,
                LookAheadSpeed * Time.deltaTime);

            Vector3 offset = _normalComposer.TargetOffset;
            offset.x = _currentLookAhead;
            _normalComposer.TargetOffset = offset;
        }
    }

    //-- EVENT HANDLERS ---------------------------------------------------------------------//

    private void HandleOnPlayerReset()
    {
        if (_delayCoroutine != null)
            StopCoroutine(_delayCoroutine);

        _delayCoroutine = StartCoroutine(TemporaryDeactivateAutoZoom(_temporalAutoZoomDeactivationTime));
    }

    //-- COROUTINES --------------------------------------------------------------------------//

    public IEnumerator TemporaryDeactivateAutoZoom(float delay)
    {
        CameraAutoZoomDisabled = true;
        yield return new WaitForSecondsRealtime(delay);
        CameraAutoZoomDisabled = _originalAutoZoomOption;
    }
}