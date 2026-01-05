using System;
using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	/// <summary>
	/// Owns membership: add/remove users, uniqueness rules, and emits events.
	/// </summary>
	public class GroupRoster<TTarget, TUser>
		where TTarget : GroupTarget<TTarget, TUser>
		where TUser : GroupUser<TTarget, TUser>
	{
		protected readonly Group<TTarget, TUser> Group;
		protected readonly List<TUser> _users = new();

		public List<TUser> Users => _users;

		public event Action<TUser> UserAdded;
		public event Action<TUser> UserRemoved;
		public event Action<List<TUser>> UsersAdded;

		public GroupRoster(Group<TTarget, TUser> group)
		{
			Group = group ?? throw new ArgumentNullException(nameof(group));
		}

		/// <summary>
		/// Adds a user to the group if not already present.
		/// </summary>
		/// <param name="user"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual void AddUser(TUser user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (_users.Contains(user)) return;

			_users.Add(user);
			user.SetGroup(Group);

			if (!Group.HasLeader)
				_ = user.OnBeforeAppointingLeaderAsync();

			UserAdded?.Invoke(user);
		}

		/// <summary>
		/// Adds multiple users to the group, ignoring nulls and duplicates.
		/// </summary>
		/// <param name="users"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual void AddUsers(List<TUser> users)
		{
			if (users == null) throw new ArgumentNullException(nameof(users));
			if (users.Count == 0) return;

			bool anyAdded = false;

			for (int i = 0; i < users.Count; i++)
			{
				var user = users[i];
				if (user == null) continue;
				if (_users.Contains(user)) continue;

				anyAdded = true;
				_users.Add(user);
				user.SetGroup(Group);

				if (!Group.HasLeader)
					_ = user.OnBeforeAppointingLeaderAsync();

				UserAdded?.Invoke(user);
			}

			if (anyAdded)
				UsersAdded?.Invoke(users);
		}

		/// <summary>
		/// Removes a user from the group.
		/// </summary>
		/// <param name="user"></param>
		public virtual void RemoveUser(TUser user)
		{
			if (user == null) return;
			if (!_users.Remove(user)) return;

			UserRemoved?.Invoke(user);
		}

		public bool Contains(TUser user) => user != null && _users.Contains(user);
	}
}