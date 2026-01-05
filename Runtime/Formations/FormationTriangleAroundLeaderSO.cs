using System;
using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime.Formations
{
	[CreateAssetMenu(fileName = "FormationTriangleAroundLeaderSO",
		menuName = "Formations/FormationTriangleAroundLeaderSO")]
	public class FormationTriangleAroundLeaderSO : FormationSO
	{
		public float LeaderDistance = 15f;
		public float Spacing = 2f;

		public override Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(Group<TTarget, TUser> group)
		{
			return new FormationTriangleAroundLeader<TTarget, TUser>(group, LeaderDistance, Spacing);
		}
	}

	public class FormationTriangleAroundLeader<TTarget, TUser> : Formation<TTarget, TUser>
		where TTarget : GroupTarget<TTarget, TUser> where TUser : GroupUser<TTarget, TUser>
	{
		private float leaderDistance;
		private float spacing;

		public FormationTriangleAroundLeader(Group<TTarget, TUser> group, float leaderDistance, float spacing)
			: base(group)
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

			// 2. Leader always in the "center of last row" slot (mapped to rawIndex == 0)
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

			// 4. Local improvement by pairwise swaps
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

						// Never move leader off index 0
						if (a == leader || b == leader)
							continue;

						int idxA = a.FormationIndex;
						int idxB = b.FormationIndex;

						if (idxA == idxB)
							continue;

						Vector3 posA = a.transform.position;
						Vector3 posB = b.transform.position;

						if (idxB >= slotPositions.Length || idxA >= slotPositions.Length)
							throw new ArgumentOutOfRangeException($"{idxB}, {slotPositions.Length}");
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
							improvedThisIteration = true;
						}
					}
				}

				if (!improvedThisIteration)
					break;
			}
		}

		protected override Vector3 GetFormationPosition(TUser user)
		{
			int index = user.FormationIndex;
			return GetFormationPositionByIndex(index);
		}

		/// <summary>
		/// Computes the leader's anchor position at a fixed distance from the target.
		/// </summary>
		private Vector3 GetLeaderAnchorPosition()
		{
			var leader = Group.Leader;
			var target = Group.Target;

			Vector3 distance = leader.transform.position - target.transform.position;
			float currentDistance = distance.magnitude;

			if (currentDistance < 0.001f)
			{
				// Fallback if leader == target position
				return leader.transform.position;
			}

			if (Mathf.Abs(currentDistance - leaderDistance) < 0.1f)
				return leader.transform.position;

			return target.transform.position + distance.normalized * leaderDistance;
		}

		/// <summary>
		/// Maps rawIndex to a triangular slot, with index 0 being the
		/// center of the last (back) row.
		/// </summary>
		protected Vector3 GetFormationPositionByIndex(int rawIndex)
		{
			int count = Group.Users.Count;
			if (count == 0)
				return Group.Leader.transform.position;

			// Leader anchor (back row center reference)
			Vector3 leaderAnchor = GetLeaderAnchorPosition();

			// Direction from leader toward target (this is "forward" for the formation)
			Vector3 toTarget = Group.Target.transform.position - leaderAnchor;
			Vector3 forwardDir;

			if (toTarget.sqrMagnitude < 0.0001f)
				forwardDir = Group.Leader.transform.forward;
			else
				forwardDir = toTarget.normalized;

			Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

			// Leader slot is always index 0, last row center
			if (rawIndex == 0)
				return leaderAnchor;

			// Build a triangular grid of slots (front row has 1, back row has 'rows')
			int rows = 0;
			int totalSlots = 0;
			while (totalSlots < count)
			{
				rows++;
				totalSlots += rows;
			}

			int leaderRow = rows - 1;
			int leaderRowSize = leaderRow + 1;
			int leaderCol = leaderRowSize / 2; // middle of last row

			// Slot order: index 0 -> leader slot, others front-to-back
			var slotOrder = new List<Vector2Int>(count);
			slotOrder.Add(new Vector2Int(leaderRow, leaderCol));

			for (int r = 0; r < rows && slotOrder.Count < count; r++)
			{
				int size = r + 1;
				for (int c = 0; c < size && slotOrder.Count < count; c++)
				{
					if (r == leaderRow && c == leaderCol)
						continue; // already used by leader

					slotOrder.Add(new Vector2Int(r, c));
				}
			}

			rawIndex = Mathf.Clamp(rawIndex, 0, slotOrder.Count - 1);
			Vector2Int slot = slotOrder[rawIndex];
			int row = slot.x;
			int col = slot.y;

			// Row spacing:
			// - leaderRow is at the back (farthest from target)
			// - smaller rows (0,1,2,...) are in front, closer to the target
			float rowSize = row + 1;
			float xOffset = (col - 0.5f * (rowSize - 1)) * spacing;

			float zOffset = (leaderRow - row) * spacing; // 0 for leader row, >0 for units in front

			Vector3 offset = forwardDir * zOffset + rightDir * xOffset;
			return leaderAnchor + offset;
		}
	}
}