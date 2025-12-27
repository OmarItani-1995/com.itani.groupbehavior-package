using UnityEngine;

namespace GroupBehavior.Runtime
{
	public abstract class FormationTarget : MonoBehaviour
	{

	}

	public abstract class FormationTarget<TTarget, TUser> : FormationTarget
		where TTarget : FormationTarget<TTarget, TUser> where TUser : FormationUser<TTarget, TUser>
	{
		protected UnitGroup<TTarget, TUser> CurrentGroup;
		
		public void Subscribe(TTarget target)
		{
			CurrentGroup = FormationsManager<TTarget, TUser>.Instance.GetOrCreateMainFormationGroup(target);
		}
	}
}