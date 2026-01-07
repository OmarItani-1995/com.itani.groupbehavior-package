using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime.Formations
{
    [CreateAssetMenu(fileName = "FormationWallInfrontOfLeaderSO", menuName = "Formations/FormationWallInfrontOfLeaderSO")]
    public class FormationWallInfrontOfLeaderSO : FormationSO
    {
        public float LeaderDistance = 15f;
        public float Spacing = 2f;

        public override Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(Group<TTarget, TUser> group)
            => new FormationWallInfrontOfLeader<TTarget, TUser>(group, LeaderDistance, Spacing);
    }

    public class FormationWallInfrontOfLeader<TTarget, TUser> : Formation<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser : GroupUser<TTarget, TUser>
    {
        private const int SwapImproveMaxIterations = 4;

        private readonly float _leaderDistance;
        private readonly float _spacing;

        public FormationWallInfrontOfLeader(Group<TTarget, TUser> group, float leaderDistance, float spacing)
            : base(group)
        {
            _leaderDistance = leaderDistance;
            _spacing = spacing;
        }

        public override void AssignIndexes()
        {
            int count = Group.Users.Count;
            if (count == 0) return;

            Vector3[] slotPositions = BuildSlotPositions(count);

            var leader = Group.Leader;
            leader.SetFormationIndex(0);

            var taken = new HashSet<int> { 0 };
            var followers = new List<TUser>(Group.Users);
            followers.Remove(leader);

            AssignByClosestFreeSlot(followers, slotPositions, taken);

            ImproveByPairSwaps(followers, slotPositions);
        }

        protected override Vector3 GetFormationPosition(TUser user)
            => GetSlotPositionByIndex(user.FormationIndex);

        private Vector3[] BuildSlotPositions(int count)
        {
            var slots = new Vector3[count];
            for (int i = 0; i < count; i++)
                slots[i] = GetSlotPositionByIndex(i);

            return slots;
        }

        private static void AssignByClosestFreeSlot(
            List<TUser> followers,
            Vector3[] slotPositions,
            HashSet<int> takenIndices)
        {
            int count = slotPositions.Length;

            foreach (var user in followers)
            {
                float bestDistSqr = float.MaxValue;
                int bestIndex = -1;

                for (int i = 0; i < count; i++)
                {
                    if (takenIndices.Contains(i))
                        continue;

                    float distSqr = (user.transform.position - slotPositions[i]).sqrMagnitude;
                    if (distSqr < bestDistSqr)
                    {
                        bestDistSqr = distSqr;
                        bestIndex = i;
                    }
                }

                user.SetFormationIndex(bestIndex);
                takenIndices.Add(bestIndex);
            }
        }

        private static void ImproveByPairSwaps(List<TUser> followers, Vector3[] slotPositions)
        {
            for (int iter = 0; iter < SwapImproveMaxIterations; iter++)
            {
                bool improved = false;

                for (int i = 0; i < followers.Count; i++)
                {
                    var a = followers[i];
                    int idxA = a.FormationIndex;

                    if ((uint)idxA >= (uint)slotPositions.Length)
                        continue;

                    for (int j = i + 1; j < followers.Count; j++)
                    {
                        var b = followers[j];
                        int idxB = b.FormationIndex;

                        if (idxA == idxB)
                            continue;

                        if ((uint)idxB >= (uint)slotPositions.Length)
                            continue;

                        Vector3 posA = a.transform.position;
                        Vector3 posB = b.transform.position;

                        Vector3 slotA = slotPositions[idxA];
                        Vector3 slotB = slotPositions[idxB];

                        float currentCost =
                            (posA - slotA).sqrMagnitude +
                            (posB - slotB).sqrMagnitude;

                        float swappedCost =
                            (posA - slotB).sqrMagnitude +
                            (posB - slotA).sqrMagnitude;

                        if (swappedCost < currentCost)
                        {
                            a.SetFormationIndex(idxB);
                            b.SetFormationIndex(idxA);

                            idxA = a.FormationIndex;
                            improved = true;
                        }
                    }
                }

                if (!improved)
                    break;
            }
        }

        private Vector3 GetSlotPositionByIndex(int rawIndex)
        {
            if (rawIndex == 0)
                return GetLeaderAnchorPosition();

            var leader = Group.Leader;
            Vector3 leaderPos = leader.transform.position;

            int followerCount = Group.Users.Count - 1;
            if (followerCount <= 0)
                return leaderPos;

            int followerIndex = rawIndex - 1;

            int columns = Mathf.CeilToInt(followerCount / 2f);
            int rows = Mathf.CeilToInt((float)followerCount / columns);

            int row = followerIndex / columns;
            int col = followerIndex % columns;

            int firstIndexInRow = row * columns;
            int remaining = followerCount - firstIndexInRow;
            int colsThisRow = Mathf.Min(columns, remaining);

            float rowCenterOffset = (colsThisRow - 1) * 0.5f;
            float xOffset = (col - rowCenterOffset) * _spacing;

            float zOffset = (row + 1) * _spacing;

            Vector3 offset = (leader.transform.forward * zOffset) + (leader.transform.right * xOffset);
            return leaderPos + offset;
        }

        private Vector3 GetLeaderAnchorPosition()
        {
            Vector3 leaderPos = Group.Leader.transform.position;
            Vector3 targetPos = Group.Target.transform.position;

            Vector3 delta = leaderPos - targetPos;
            float dist = delta.magnitude;

            if (Mathf.Abs(dist - _leaderDistance) < 0.1f)
                return leaderPos;

            if (dist < 0.001f)
                return leaderPos;

            return targetPos + (delta / dist) * _leaderDistance;
        }
    }
}
