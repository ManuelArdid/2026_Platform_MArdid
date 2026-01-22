using UnityEngine.InputSystem;


public abstract class Player2 : Player
{

    //------- Protected Methods -------//
    /// <summary>
    /// Handles character movement based on player input.
    /// </summary>
    protected override void Move(InputAction.CallbackContext context)
    {
        //Move only if not grounded
        if (!IsGrounded())
        {
            base.Move(context);
        }
    }

}