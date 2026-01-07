using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	/// <summary>
	/// Base class for formations.
	/// </summary>
	/// <typeparam name="TTarget"></typeparam>
	/// <typeparam name="TUser"></typeparam>
	public abstract class Formation<TTarget, TUser> where TTarget : GroupTarget<TTarget, TUser>
		where TUser : GroupUser<TTarget, TUser>
	{
		public Group<TTarget, TUser> Group { get; private set; }

		public Formation(Group<TTarget, TUser> group)
		{
			Group = group;
		}

		/// <summary>
		/// Assigns formation indexes to group users.
		/// </summary>
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

		/// <summary>
		/// Starts the formation by adding the position update to the group's update loop.
		/// </summary>
		public void StartFormation()
		{
			Group.AddToUpdate(UpdatePositions);
			AssignIndexes();
		}

		/// <summary>
		/// Ends the formation by removing the position update from the group's update loop.
		/// </summary>
		public void EndFormation()
		{
			Group.RemoveFromUpdate(UpdatePositions);
		}

		/// <summary>
		/// Updates the positions of all users in the formation.
		/// </summary>
		public virtual void UpdatePositions()
		{
			foreach (var user in Group.Users)
			{
				user.SetFormationPosition(GetFormationPosition(user));
			}
		}

		/// <summary>
		/// Gets the formation position for a specific user.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		protected abstract Vector3 GetFormationPosition(TUser user);

		/// <summary>
		/// Called when a user is removed from the formation.
		/// </summary>
		/// <param name="user"></param>
		public virtual void OnUserRemoved(TUser user)
		{
			if (user != Group.Leader)
			{
				AssignIndexes();
			}
		}

		/// <summary>
		/// Called when a user is added to the formation.
		/// </summary>
		/// <param name="user"></param>
		public virtual void OnUserAdded(TUser user)
		{
			AssignIndexes();
		}

		/// <summary>
		/// Called when multiple users are added to the formation.
		/// </summary>
		/// <param name="users"></param>
		public virtual void OnUsersAdded(List<TUser> users)
		{
			AssignIndexes();
		}
	}

	public abstract class FormationSO : ScriptableObject
	{
		public abstract Formation<TTarget, TUser> CreateFormation<TTarget, TUser>(Group<TTarget, TUser> group)
			where TTarget : GroupTarget<TTarget, TUser>
			where TUser : GroupUser<TTarget, TUser>;
	}
}