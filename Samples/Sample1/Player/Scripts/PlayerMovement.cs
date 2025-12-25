using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PlayerInputListener inputListener;
    public float moveSpeed = 5f;

    private Transform _camera;
    public Transform camera => _camera ? _camera : _camera = Camera.main.transform;
    void Update()
    {
        Move();
        Rotate();
    }

    private void Rotate()
    {
        Vector3 lookDirection = inputListener.LookDirection;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void Move()
    {
        Vector3 moveDirection = inputListener.MovementInput.normalized;
        Vector3 movement = moveDirection.x * camera.transform.right + moveDirection.z * camera.transform.forward;
        movement.y = 0;
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }
}
