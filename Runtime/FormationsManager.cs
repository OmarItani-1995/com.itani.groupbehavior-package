using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public class FormationsManager<TTarget, TUser> : MonoBehaviour where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		#region Singleton

		public static FormationsManager<TTarget, TUser> Instance;

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		#endregion

		protected Dictionary<TTarget, UnitGroup<TTarget, TUser>> FormationGroups =
			new Dictionary<TTarget, UnitGroup<TTarget, TUser>>();

		public virtual void Update()
		{
			UpdateGroups();
		}

		private void UpdateGroups()
		{
			foreach (var group in FormationGroups)
			{
				group.Value.Update();
			}
		}

		public void RemoveUnitGroup(UnitGroup<TTarget, TUser> unitGroup)
		{
			if (FormationGroups.ContainsKey(unitGroup.Target))
			{
				FormationGroups.Remove(unitGroup.Target);
			}
		}
		
		public virtual void CreateFormationGroup(TTarget target, List<TUser> users)
		{
			var formationGroup = CreateDefaultFormationGroup(target);
			formationGroup.Init();
			formationGroup.AddUsers(users);
			FormationGroups.Add(target, formationGroup);	
		}

		public UnitGroup<TTarget, TUser> GetOrCreateMainFormationGroup(TTarget target)
		{
			if (!FormationGroups.ContainsKey(target))
			{
				var formationGroup = CreateDefaultFormationGroup(target);
				formationGroup.Init();
				FormationGroups.Add(target, formationGroup);
			}

			return FormationGroups[target];
		}

		public UnitGroup<TTarget, TUser> AddUserToFormationGroup(TTarget target, TUser user)
		{
			var formationGroup = FormationGroups.TryGetValue(target, out var group)
				? group
				: GetOrCreateMainFormationGroup(target);
			formationGroup.AddUser(user);
			return formationGroup;
		}

		protected virtual UnitGroup<TTarget, TUser> CreateDefaultFormationGroup(TTarget target)
		{
			return new UnitGroupSingle<TTarget, TUser>()
			{
				Target = target,
			};
		}

		#region Debug

		public static Dictionary<TTarget, UnitGroup<TTarget, TUser>> GetGroups()
		{
			return Instance.FormationGroups;
		}

		#endregion
		
	}
}

