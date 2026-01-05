using System.Collections.Generic;
using GroupBehavior.Runtime;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public class GroupCreatorOnTargetTrigger<TTarget, TUser> : MonoBehaviour
		where TTarget : GroupTarget<TTarget, TUser> where TUser : GroupUser<TTarget, TUser>
	{
		public GroupBuilder<TTarget, TUser> GroupBuilder;
		public bool UseTag;
		public string TargetTag;

		public bool UseLayer;
		public string TargetLayer;

		public bool DestroyOnDone = false;

		private void OnTriggerEnter(Collider other)
		{
			if (UseTag && !other.CompareTag(TargetTag))
			{
				return;
			}

			if (UseLayer && other.gameObject.layer != LayerMask.NameToLayer(TargetLayer))
			{
				return;
			}

			TTarget target = other.GetComponent<TTarget>();
			if (target != null)
			{
				OnTargetEntered(target);
			}
		}

		private void OnTargetEntered(TTarget target)
		{
			GroupBuilder.BuildGroup(target);

			if (DestroyOnDone)
			{
				Destroy(this.gameObject);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
	}
}