using UnityEngine;

namespace GroupBehavior.Samples.Runtime
{
	public class CameraMovement : MonoBehaviour
	{
		public Transform target;
		public float moveSpeed = 10f;

		private Vector3 _velocity;

		void LateUpdate()
		{
			HandleMovement();
		}

		void HandleMovement()
		{
			if (target == null) return;
			Vector3 targetPosition = target.position;
			targetPosition.y = transform.position.y; // Keep camera height constant
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, 1f / moveSpeed);
		}
	}
}