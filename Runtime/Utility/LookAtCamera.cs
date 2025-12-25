using UnityEngine;

namespace GroupBehavior.Runtime.Example
{
    public class LookAtCamera : MonoBehaviour
    {
        private Transform cameraTransform;
        void Start()
        {
            cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            Vector3 direction = cameraTransform.position - transform.position;
            direction.y = 0; // Keep only the horizontal direction
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }
}