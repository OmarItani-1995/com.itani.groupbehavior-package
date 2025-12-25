using UnityEngine;

namespace GroupBehavior.Runtime
{
	public abstract class Formation<TTarget, TUser> where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public UnitGroup<TTarget, TUser> Group { get; private set; }

		public Formation(UnitGroup<TTarget, TUser> group)
		{
			Group = group;
		}

		public virtual void AssignIndexes()
		{
			var leader = Group.Leader;
			leader.SetFormationIndex(0);
			var users = Group.Users;
			int index = 1;
			for (int i = 0; i < users.Count; i++)
			{
				if (users[i] == leader) continue;
				users[i].SetFormationIndex(index);
				index++;
			}
		}

		public void StartFormation()
		{
			Group.AddToUpdate(UpdatePositions);
			AssignIndexes();
		}

		public void EndFormation()
		{
			Group.RemoveFromUpdate(UpdatePositions);
		}

		public virtual void UpdatePositions()
		{
			foreach (var user in Group.Users)
			{
				user.SetFormationPosition(GetFormationPosition(user));
			}
		}

		protected abstract Vector3 GetFormationPosition(TUser user);

		public virtual void OnUserRemoved(TUser user)
		{
			AssignIndexes();
		}

		public virtual void OnUserAdded(TUser user)
		{
			AssignIndexes();
		}
	}

	public abstract class FormationSO : ScriptableObject
	{
		public abstract Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(UnitGroup<TTarget, TUser> group)
			where TTarget : FormationTarget<TTarget, TUser>
			where TUser : FormationUser<TTarget, TUser>;
	}
}