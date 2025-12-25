using UnityEngine;

namespace GroupBehavior.Runtime.Utility
{
    public static class FormationUtility
    {
        private static readonly Color voterColor = Color.yellow;
        private static readonly Color candidateColor = Color.red;

        public static void DrawVoteLines(Transform voter, Transform candidate, float duration)
        {
            var midPoint = (voter.position + candidate.position) / 2;
            Debug.DrawLine(voter.position, midPoint, voterColor, duration);
            Debug.DrawLine(midPoint, candidate.position, candidateColor, duration);
        }
    }
}