using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public class GroupManager<TTarget, TUser> : MonoBehaviour where TTarget : GroupTarget<TTarget, TUser>
		where TUser : GroupUser<TTarget, TUser>
	{
		public List<Group<TTarget, TUser>> Groups = new List<Group<TTarget, TUser>>();
		
		protected virtual void Update()
		{
			UpdateGroups();
		}

		private void UpdateGroups()
		{
			foreach (var group in Groups)
			{
				group.Update();
			}
		}

		/// <summary>
		/// Removes a group from management.
		/// </summary>
		/// <param name="group"></param>
		public void RemoveUnitGroup(Group<TTarget, TUser> group)
		{
			if (Groups.Contains(group))
			{
				Groups.Remove(group);
			}
		}
		
		/// <summary>
		///  Creates a formation group with specified target and users.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="users"></param>
		/// <returns></returns>
		public virtual Group<TTarget, TUser> CreateFormationGroup(TTarget target, List<TUser> users)
		{
			var group = CreateDefaultFormationGroup(target);
			group.Init();
			Groups.Add(group);	
			group.AddUsers(users);
			return group;
		}

		/// <summary>
		/// Creates the default formation group instance.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Group<TTarget, TUser> CreateDefaultFormationGroup(TTarget target)
		{
			return new GroupManualInitialization<TTarget, TUser>()
			{
				Target = target,
			};
		}
	}
}

