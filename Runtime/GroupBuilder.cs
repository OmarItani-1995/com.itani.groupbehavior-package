using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public class GroupBuilder<TTarget, TUser> : MonoBehaviour where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public FormationsManager<TTarget, TUser> FormationsManager;
		public List<TUser> users;
		private UnitGroup<TTarget, TUser> unitGroup;
		public virtual void BuildGroup(TTarget target) 
		{
			unitGroup = FormationsManager.CreateFormationGroup(target, users);
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
		
		public void AddUser(TUser user)
		{
			if (unitGroup != null)
			{
				unitGroup.AddUser(user);
			}
			else
			{
				users.Add(user);
				user.OnAddedToGroupBuilderAsync();
			}
		}

		public virtual void StartVotingProcess() 
		{
			if (unitGroup != null)
			{
				unitGroup.StartInitialVotingProcess();
			}	
		}
	}
}
