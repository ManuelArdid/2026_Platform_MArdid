using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    //------ UNITY EDITOR --------------------------------------------------------------------------------------//
    [SerializeField] private InputActionReference PauseInputAction;

    //------ UNITY METHODS --------------------------------------------------------------------------------------//
    void OnEnable()
    {
        PauseInputAction.action.Enable();
        PauseInputAction.action.performed += HandlePauseInput;
    }
    void OnDisable()
    {
        PauseInputAction.action.Disable();
        PauseInputAction.action.performed -= HandlePauseInput;
    }

    //------ HANDLE METHODS --------------------------------------------------------------------------------------//
    private void HandlePauseInput(InputAction.CallbackContext context)
    {
        GameManager.Instance.LoadMainMenuScene();
    }
}
