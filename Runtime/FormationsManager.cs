using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public class FormationsManager<TTarget, TUser> : MonoBehaviour where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public List<UnitGroup<TTarget, TUser>> Groups = new List<UnitGroup<TTarget, TUser>>();
		
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

		public void RemoveUnitGroup(UnitGroup<TTarget, TUser> unitGroup)
		{
			if (Groups.Contains(unitGroup))
			{
				Groups.Remove(unitGroup);
			}
		}
		
		public virtual UnitGroup<TTarget, TUser> CreateFormationGroup(TTarget target, List<TUser> users)
		{
			var group = CreateDefaultFormationGroup(target);
			group.Init();
			Groups.Add(group);	
			group.AddUsers(users);
			return group;
		}

		protected virtual UnitGroup<TTarget, TUser> CreateDefaultFormationGroup(TTarget target)
		{
			return new UnitGroupSingle<TTarget, TUser>()
			{
				Target = target,
			};
		}
	}
}

