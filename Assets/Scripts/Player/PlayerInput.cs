using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Action<Vector3> OnMove;
    public Action<Vector2> OnRotate;

    public Action OnAttack;

    private void Update()
    {
        HandleRotationInput();
        HandleAttackInput();
    }

    private void FixedUpdate()
    {
        HandleMovementInput();
    }

    public void Enable() => enabled = true;
    public void Disable() => enabled = false;

    private void HandleMovementInput()
    {
        Vector3 move = new(
            Input.GetAxisRaw("Horizontal"),
            Input.GetKey(KeyCode.Space) ? 1f : Input.GetKey(KeyCode.LeftShift) ? -1f : 0f,
            Input.GetAxisRaw("Vertical")
        );

        if (move != Vector3.zero)
            OnMove?.Invoke(move);
    }

    private void HandleRotationInput()
    {
        Vector2 rotation = new(
            -Input.GetAxisRaw("Mouse Y"),
            Input.GetAxisRaw("Mouse X")
        );

        if (rotation != Vector2.zero)
            OnRotate?.Invoke(rotation);
    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            OnAttack?.Invoke();
    }
}