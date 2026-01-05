using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GroupBehavior.Runtime.Formations
{
	[CreateAssetMenu(fileName = "FormationWallInfrontOfLeaderSO",
		menuName = "Formations/FormationWallInfrontOfLeaderSO")]
	public class FormationWallInfrontOfLeaderSO : FormationSO
	{
		public float LeaderDistance = 15f;
		public float Spacing = 2f;

		public override Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(Group<TTarget, TUser> group)
		{
			return new FormationWallInfrontOfLeader<TTarget, TUser>(group, LeaderDistance, Spacing);
		}
	}

	public class FormationWallInfrontOfLeader<TTarget, TUser> : Formation<TTarget, TUser>
		where TTarget : GroupTarget<TTarget, TUser> where TUser : GroupUser<TTarget, TUser>
	{
		private float leaderDistance;
		private float spacing;

		public FormationWallInfrontOfLeader(Group<TTarget, TUser> group, float leaderDistance, float spacing) :
			base(group)
		{
			this.leaderDistance = leaderDistance;
			this.spacing = spacing;
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
			int index = user.FormationIndex;
			return GetFormationPositionByIndex(index);
		}

		protected Vector3 GetFormationPositionByIndex(int rawIndex)
		{
			if (rawIndex == 0)
			{
				// Leader position
				Vector3 distance = Group.Leader.transform.position - Group.Target.transform.position;
				float currentDistance = distance.magnitude;

				// If already at the correct distance, do nothing
				if (Mathf.Abs(currentDistance - leaderDistance) < 0.1f)
					return Group.Leader.transform.position;

				return Group.Target.transform.position + distance.normalized * leaderDistance;
			}
			else
			{
				var leader = Group.Leader;
				Vector3 leaderPosition = leader.transform.position;
				Vector3 leaderForward = leader.transform.forward;
				Vector3 leaderRight = leader.transform.right;

				// Followers only (exclude leader)
				int followerCount = Group.Users.Count - 1;
				if (followerCount <= 0)
					return leaderPosition;

				int index = rawIndex - 1; // 0..followerCount-1

				// Choose a near-square grid
				int columns = Mathf.CeilToInt(followerCount / 2f);
				int rows = Mathf.CeilToInt((float)followerCount / columns);

				int row = index / columns;
				int col = index % columns;

				// How many units are in THIS row? (last row may be partial)
				int firstIndexInRow = row * columns;
				int remaining = followerCount - firstIndexInRow;
				int colsThisRow = Mathf.Min(columns, remaining);

				// Center this row horizontally
				float rowCenterOffset = (colsThisRow - 1) * 0.5f;
				float x = (col - rowCenterOffset) * spacing;

				// Place rows behind (or in front) of leader; adjust sign if needed
				float z = (row + 1) * spacing;

				Vector3 offset = leaderForward * z + leaderRight * x;
				return leaderPosition + offset;
			}
		}
	}
}