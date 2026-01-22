using UnityEngine;

public class Player2 : Player
{
    protected override void HandleMovement()
    {
        if (IsGrounded())
        {
            _currentVelocity.x = 0f;   
            _rb.linearVelocityX = 0f;
            return;
        }

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


    protected override void Update()
    {
        // Animations
        _animator.SetBool("IsFalling", _rb.linearVelocityY < 0f && !_isDoubleJumping);
        _animator.SetBool("IsJumping", _rb.linearVelocityY > 0f && !_isDoubleJumping);

        // Flip sprite
        if (_currentVelocity.x > 0)
            _spriteRenderer.flipX = false;
        else if (_currentVelocity.x < 0)
            _spriteRenderer.flipX = true;
    }
}
