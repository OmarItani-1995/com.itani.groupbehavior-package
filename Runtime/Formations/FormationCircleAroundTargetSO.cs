using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime.Formations
{
    [CreateAssetMenu(fileName = "FormationCircleAroundTarget", menuName = "Formations/FormationCircleAroundTarget")]
    public class FormationCircleAroundTargetSO : FormationSO
    {
        [Min(0f)] public float angle = 360f;
        [Min(0f)] public float radius = 5f;

        public override Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(Group<TTarget, TUser> group)
            => new FormationCircleAroundTarget<TTarget, TUser>(group, angle, radius);
    }

    public class FormationCircleAroundTarget<TTarget, TUser> : Formation<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser : GroupUser<TTarget, TUser>
    {
        private const int SwapImproveMaxIterations = 4;
        private const float MinDirSqrEpsilon = 0.0001f;

        private readonly float _radius;
        private readonly float _angle;

        private readonly Dictionary<TUser, float> _anchorOffset = new();

        public FormationCircleAroundTarget(Group<TTarget, TUser> group, float angle, float radius)
            : base(group)
        {
            _radius = radius;
            _angle = angle;
        }

        public override void OnUserRemoved(TUser user)
        {
            if (user != Group.Leader)
            {
                AssignIndexes();
            }
        }

        public override void AssignIndexes()
        {
            int count = Group.Users.Count;
            if (count == 0) return;

            var slotPositions = BuildSlotPositions(count);

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

        private void AssignByClosestFreeSlot(
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

        private void ImproveByPairSwaps(List<TUser> followers, Vector3[] slotPositions)
        {
            for (int iter = 0; iter < SwapImproveMaxIterations; iter++)
            {
                bool improved = false;

                for (int i = 0; i < followers.Count; i++)
                {
                    var a = followers[i];
                    int idxA = a.FormationIndex;

                    for (int j = i + 1; j < followers.Count; j++)
                    {
                        var b = followers[j];
                        int idxB = b.FormationIndex;

                        if (idxA == idxB)
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
            int count = Group.Users.Count;

            Vector3 toLeader = Group.Leader.transform.position - Group.Target.transform.position;
            toLeader.y = 0f;

            float leaderAngleRad = 0f;
            if (toLeader.sqrMagnitude > MinDirSqrEpsilon)
            {
                toLeader.Normalize();
                leaderAngleRad = Mathf.Atan2(toLeader.z, toLeader.x);
            }

            float stepRad = (_angle * Mathf.Deg2Rad) / count;

            int mid = (count - 1) / 2;
            int index = (rawIndex + mid) % count;

            float baseAngleRad = leaderAngleRad - (mid * stepRad);
            float slotAngleRad = baseAngleRad + (index * stepRad);

            Vector3 offset =
                new Vector3(Mathf.Cos(slotAngleRad), 0f, Mathf.Sin(slotAngleRad)) * _radius;

            return Group.Target.transform.position + offset;
        }
    }
}
