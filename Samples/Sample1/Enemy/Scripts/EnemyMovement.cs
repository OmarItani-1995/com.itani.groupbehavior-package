using UnityEngine;
using UnityEngine.AI;

namespace GroupBehavior.Samples.Runtime
{
    public class EnemyMovement : MonoBehaviour
    {
        public NavMeshAgent agent;

        private Transform _target;

        void Start()
        {
            agent.updateRotation = false;
        }

        void Update()
        {
            LookAtTarget();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void MoveToPosition(Vector3 position)
        {
            agent.SetDestination(position);
        }

        private void LookAtTarget()
        {
            if (_target == null) return;
            Vector3 direction = _target.position - transform.position;
            direction.y = 0; // Keep only horizontal direction
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }
}