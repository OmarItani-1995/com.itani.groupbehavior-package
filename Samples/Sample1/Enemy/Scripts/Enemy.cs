using UnityEngine;

namespace GroupBehavior.Samples.Runtime
{
    public class Enemy : MonoBehaviour
    {
        public Player player;
        public EnemyMovement movement;
        public EnemyFormationUser formationUser;

        void Start()
        {
            movement.SetTarget(player.transform);
            formationUser.SetTarget(player.GetFormationTarget());
        }

        public void Die()
        {
            formationUser.OnDeath();
        }
    }
}