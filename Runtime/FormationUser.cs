using System.Threading.Tasks;
using GroupBehavior.Runtime.Utility;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	public abstract class FormationUser : MonoBehaviour
	{

	}

	public abstract class FormationUser<TTarget, TUser> : FormationUser where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public int FormationIndex { get; private set; } = -1;
		public FormationSO FormationSO;

		protected UnitGroup<TTarget, TUser> UnitGroup;
		
		public void Subscribe(TTarget target, TUser user)
		{
			FormationsManager<TTarget, TUser>.Instance.AddUserToFormationGroup(target, user);
		}
		
		public void SetGroup(UnitGroup<TTarget, TUser> group)
		{
			UnitGroup = group;
		}

		public void Unsubscribe(TUser user)
		{
			UnitGroup.RemoveUser(user);
		}

		public virtual async Task VoteForLeaderAsync(VotingData<TTarget, TUser> votingData)
		{
			var votedFor = votingData.VoteForRandom();
#if UNITY_EDITOR
			FormationUtility.DrawVoteLines(transform, votedFor.transform, 1f);
#endif
			await Awaitable.WaitForSecondsAsync(1f);
		}

		public virtual Task PromotedToLeaderAsync()
		{
			Debug.Log("Promoted to Leader: " + gameObject.name);
			return Task.CompletedTask;
		}

		public virtual Task NewLeaderAppointedAsync(TUser newLeader)
		{
			return Task.CompletedTask;
		}

		public virtual void SetFormationIndex(int index)
		{
			FormationIndex = index;
		}

		public virtual void SetFormationPosition(Vector3 position)
		{
		}

		public virtual bool WillAcceptLeadership()
		{
			return true;
		}

		public virtual Task DeclinedLeadershipAsync()
		{
			Debug.Log("Declined Leadership: " + gameObject.name);
			return Task.CompletedTask;
		}

		public virtual Formation<TTarget, TUser> GetFormation(UnitGroup<TTarget, TUser> group)
		{
			return FormationSO.CreateFormation(group);
		}
	}
}