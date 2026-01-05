using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GroupBehavior.Runtime
{
	/// <summary>
	/// Builds a group with specified target and users.
	/// </summary>
	/// <typeparam name="TTarget"></typeparam>
	/// <typeparam name="TUser"></typeparam>
	public class GroupBuilder<TTarget, TUser> : MonoBehaviour where TTarget : GroupTarget<TTarget, TUser>
		where TUser : GroupUser<TTarget, TUser>
	{
		public GroupManager<TTarget, TUser> groupManager;
		public List<TUser> users;
		protected Group<TTarget, TUser> _group;
		
		/// <summary>
		///	Builds the group with the specified target.
		/// </summary>
		/// <param name="target"></param>
		public virtual void BuildGroup(TTarget target) 
		{
			_group = groupManager.CreateFormationGroup(target, users);
		}

		protected virtual void Start()
		{
			var initialUsers = users.ToList();
			users.Clear();
			foreach (var user in initialUsers)
			{
				AddUser(user);
			}
		}
		
		/// <summary>
		/// Adds a user to the group being built.
		/// </summary>
		/// <param name="user"></param>
		public void AddUser(TUser user)
		{
			if (_group != null)
			{
				_group.AddUser(user);
			}
			else
			{
				users.Add(user);
				user.OnAddedToGroupBuilderAsync();
			}
		}

		/// <summary>
		/// Starts the voting process for leader selection.
		/// </summary>
		public virtual void StartVotingProcess() 
		{
			if (_group != null)
			{
				_group.StartInitialVotingProcess();
			}	
		}
	}
}
