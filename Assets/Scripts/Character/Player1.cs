using UnityEngine;

public class Player1 : Player
{
    protected override void HandleMovement()
    {
        Vector2 targetVelocity = _rawMovementInput * MoveSpeed;

        float currentAcceleration = _rawMovementInput == Vector2.zero
            ? Deceleration
            : Acceleration;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        _rb.linearVelocityX = _currentVelocity.x;
    }
}
