using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	[System.Serializable]
	public abstract class GroupLeaderVotingProcess<TTarget, TUser> where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public UnitGroup<TTarget, TUser> UnitGroup;

		public GroupLeaderVotingProcess(UnitGroup<TTarget, TUser> unitGroup)
		{
			UnitGroup = unitGroup;
			InitializeVotingProcessAsync();
		}

		protected abstract Task InitializeVotingProcessAsync();

		protected Task StartVotingProcessAsync()
		{
			VotingData<TTarget, TUser> votingData = CreateVotingData();
#if UNITY_EDITOR
		UnitGroup.AddVotingDataToHistory(votingData);
#endif
			return VoteForLeaderAsync(votingData);
		}

		protected virtual async Task VoteForLeaderAsync(VotingData<TTarget, TUser> votingData)
		{
			List<Task> voteTasks = new List<Task>();
			foreach (var user in UnitGroup.Users)
			{
				voteTasks.Add(user.VoteForLeaderAsync(votingData));
			}

			await Task.WhenAll(voteTasks);

			var mostVoted = votingData.GetMostVoted();
			await ResolveMostVotedAsync(votingData, mostVoted);
		}

		protected virtual async Task ResolveMostVotedAsync(VotingData<TTarget, TUser> votingData, List<TUser> mostVoted)
		{
			if (mostVoted.Count == 1)
			{
#if UNITY_EDITOR
			votingData.Log("No conflict, leader elected: " + mostVoted[0].gameObject.name);
#endif
				await SetFormationLeaderAsync(votingData, mostVoted[0]);
			}
			else
			{
#if UNITY_EDITOR
			votingData.Log("Conflict detected among: " + string.Join(", ", mostVoted.Select(u => u.gameObject.name)));
#endif
				await ResolveConflictAsync(votingData, mostVoted);
			}
		}

		protected virtual async Task ResolveConflictAsync(VotingData<TTarget, TUser> votingData, List<TUser> tiedUsers)
		{
			int randomIndex = UnityEngine.Random.Range(0, tiedUsers.Count);
#if UNITY_EDITOR
		votingData.Log("Resolving conflict by random selection: " + tiedUsers[randomIndex].gameObject.name);
#endif

			await SetFormationLeaderAsync(votingData, tiedUsers[randomIndex]);
		}

		protected virtual Task SetFormationLeaderAsync(VotingData<TTarget, TUser> votingData, TUser user)
		{
			if (!votingData.CanRejectLeadership)
			{
#if UNITY_EDITOR
			votingData.Log("User automatically accepted leadership (no rejection allowed): " + user.gameObject.name);
#endif
				return UnitGroup.SetLeaderAsync(user);
			}

			if (user.WillAcceptLeadership())
			{
#if UNITY_EDITOR
			votingData.Log("User accepted leadership: " + user.gameObject.name);
#endif
				return UnitGroup.SetLeaderAsync(user);
			}
			else
			{
#if UNITY_EDITOR
			votingData.Log("User declined leadership: " + user.gameObject.name);
#endif
				return LeaderDeclinedAsync(votingData, user);
			}
		}

		protected virtual async Task LeaderDeclinedAsync(VotingData<TTarget, TUser> votingData, TUser user)
		{
			await user.DeclinedLeadershipAsync();
			votingData.RemoveFromLeadershipVote(user);
			await VoteForLeaderAsync(votingData);
		}

		protected virtual VotingData<TTarget, TUser> CreateVotingData()
		{
			return new VotingData<TTarget, TUser>(UnitGroup.Target, UnitGroup.Users);
		}
	}

	[System.Serializable]
	public class GroupLeaderVotingProcessDelayed<TTarget, TUser> : GroupLeaderVotingProcess<TTarget, TUser>
		where TTarget : FormationTarget<TTarget, TUser> where TUser : FormationUser<TTarget, TUser>
	{
		public GroupLeaderVotingProcessDelayed(UnitGroup<TTarget, TUser> unitGroup, float delayBeforeVote) :
			base(unitGroup)
		{
			this.delayBeforeVote = delayBeforeVote;
		}

		public float delayBeforeVote;

		protected override async Task InitializeVotingProcessAsync()
		{
			await Awaitable.WaitForSecondsAsync(delayBeforeVote);
			await StartVotingProcessAsync();
		}
	}


	[System.Serializable]
	public class GroupLeaderVotingProcessImmediate<TTarget, TUser> : GroupLeaderVotingProcess<TTarget, TUser>
		where TTarget : FormationTarget<TTarget, TUser> where TUser : FormationUser<TTarget, TUser>
	{
		public GroupLeaderVotingProcessImmediate(UnitGroup<TTarget, TUser> unitGroup) : base(unitGroup)
		{
		}

		protected override Task InitializeVotingProcessAsync()
		{
			return StartVotingProcessAsync();
		}
	}
}

