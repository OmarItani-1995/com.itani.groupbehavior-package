using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputListener : MonoBehaviour
{
    public Vector3 MovementInput { get; private set; }
    public Vector3 LookDirection { get; private set; } 

    public void OnMove(InputValue value)
    {
        var input = value.Get<Vector2>();
        MovementInput = new Vector3(input.x, 0, input.y);
    }

    private void Update()
    {
        UpdateLookDirection();
    }
    
    private void UpdateLookDirection()
    {
        var mousePosition = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 lookPoint = hitInfo.point;
            Vector3 direction = lookPoint - transform.position;
            direction.y = 0; // Keep only horizontal direction
            LookDirection = direction.normalized;
        }
    }
}
