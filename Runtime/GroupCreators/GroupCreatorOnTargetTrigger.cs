using System.Collections.Generic;
using GroupBehavior.Runtime;
using UnityEngine;

public class GroupCreatorOnTargetTrigger<TTarget, TUser> : MonoBehaviour where TTarget : FormationTarget<TTarget, TUser> where TUser : FormationUser<TTarget, TUser> 
{
	public List<TUser> users = new List<TUser>();
	public bool UseTag;
	public string TargetTag;

	public bool UseLayer;
	public string TargetLayer;

	public bool DestroyOnDone = false;
	public bool StartVotingProcess = false;
	
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
		var group = FormationsManager<TTarget, TUser>.Instance.CreateFormationGroup(target, users);
		if (StartVotingProcess)
		{
			group.StartInitialVotingProcess();
		}

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
