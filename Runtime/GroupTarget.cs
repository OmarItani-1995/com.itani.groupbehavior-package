using UnityEngine;

namespace GroupBehavior.Runtime
{
	public abstract class GroupTarget : MonoBehaviour
	{

	}

	public abstract class GroupTarget<TTarget, TUser> : GroupTarget
		where TTarget : GroupTarget<TTarget, TUser> where TUser : GroupUser<TTarget, TUser>
	{
		protected Group<TTarget, TUser> CurrentGroup;
		
		public void SetGroup(Group<TTarget, TUser> group)
		{
			CurrentGroup = group;
		}
	}
}