using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime.Formations
{
	[CreateAssetMenu(fileName = "FormationCircleAroundTarget", menuName = "Formations/FormationCircleAroundTarget")]
	public class FormationCircleAroundTargetSO : FormationSO
	{
		public float angle = 360f;
		public float radius = 5f;

		public override Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(Group<TTarget, TUser> group)
		{
			return new FormationCircleAroundTarget<TTarget, TUser>(group, angle, radius);
		}
	}

	public class FormationCircleAroundTarget<TTarget, TUser> : Formation<TTarget, TUser>
		where TTarget : GroupTarget<TTarget, TUser> where TUser : GroupUser<TTarget, TUser>
	{
		private float radius;
		private float angle;
		private readonly Dictionary<TUser, float> _anchorOffset = new();

		public FormationCircleAroundTarget(Group<TTarget, TUser> group, float angle, float radius) : base(group)
		{
			this.radius = radius;
			this.angle = angle;
		}

		public override void AssignIndexes()
		{
			int count = Group.Users.Count;
			if (count == 0) return;

			// 1. Precompute ideal slot positions for each index
			var slotPositions = new Vector3[count];
			for (int i = 0; i < count; i++)
			{
				slotPositions[i] = GetFormationPositionByIndex(i);
			}

			// 2. Leader always in the "center" slot (rawIndex == 0 in your mapping)
			var leader = Group.Leader;
			leader.SetFormationIndex(0);

			var takenIndices = new HashSet<int> { 0 };

			// 3. Assign every other user to the closest free slot
			var followers = new List<TUser>(Group.Users);
			followers.Remove(leader);

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

			const int maxIterations = 4;
			for (int iter = 0; iter < maxIterations; iter++)
			{
				bool improvedThisIteration = false;

				for (int i = 0; i < followers.Count; i++)
				{
					for (int j = i + 1; j < followers.Count; j++)
					{
						var a = followers[i];
						var b = followers[j];

						// Just in case, never move leader off index 0
						if (a == leader || b == leader)
							continue;

						int idxA = a.FormationIndex;
						int idxB = b.FormationIndex;

						// Skip if accidentally the same index
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
							// Swap the indices
							a.SetFormationIndex(idxB);
							b.SetFormationIndex(idxA);
							improvedThisIteration = true;
						}
					}
				}

				// If no improvement in this iteration, we are done
				if (!improvedThisIteration)
					break;
			}
		}

		protected override Vector3 GetFormationPosition(TUser user)
		{
			return GetFormationPositionByIndex(user.FormationIndex);
		}

		private Vector3 GetFormationPositionByIndex(int rawIndex)
		{
			int count = Group.Users.Count;
			// Leader direction (recomputed so it stays correct if target/leader move)
			Vector3 toLeader = Group.Leader.transform.position - Group.Target.transform.position;
			toLeader.y = 0f;
			float leaderAngleRad = 0f;
			if (toLeader.sqrMagnitude > 0.0001f)
			{
				toLeader.Normalize();
				leaderAngleRad = Mathf.Atan2(toLeader.z, toLeader.x);
			}

			// Step in radians (prefer deriving from total arc to avoid drift)
			float stepRad = (angle * Mathf.Deg2Rad) / count;
			int mid = (count - 1) / 2;
			// the "center" slot in index-space
			int index = (rawIndex + mid) % count; // remap so raw 0 ends up in the center
			// Shift base so the leader (index == mid) lands exactly on leaderAngleRad
			float baseAngleRad = leaderAngleRad - (mid * stepRad);
			float slotAngleRad = baseAngleRad + (index * stepRad);
			Vector3 offset = new Vector3(Mathf.Cos(slotAngleRad), 0f, Mathf.Sin(slotAngleRad)) * radius;
			return Group.Target.transform.position + offset;
		}
	}
}