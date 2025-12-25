using UnityEngine;
using UnityEngine.Serialization;

namespace GroupBehavior.Samples.Runtime
{
    public class Player : MonoBehaviour
    {
        public PlayerFormationTarget formationTarget;

        public PlayerFormationTarget GetFormationTarget()
        {
            return formationTarget;
        }
    }
}