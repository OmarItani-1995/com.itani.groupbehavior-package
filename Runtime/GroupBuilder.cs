using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public class GroupBuilder<TTarget, TUser> : MonoBehaviour where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		private List<TUser> users;
		
		public void BuildGroup(TTarget target) 
		{
			FormationsManager<TTarget, TUser>.Instance.CreateFormationGroup(target, users);
		}
		
		public void AddUser(TUser user)
		{
			users.Add(user);
			user.OnAddedToGroupBuilderAsync();
		}
	}
}
