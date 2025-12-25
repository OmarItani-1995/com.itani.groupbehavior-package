using System;
using System.Collections.Generic;
using System.Linq;

namespace GroupBehavior.Runtime
{
	[System.Serializable]
	public partial class VotingData<TTarget, TUser> where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public TTarget Target { get; private set; }
		public List<TUser> Users { get; private set; }
		public bool CanRejectLeadership { get; private set; } = true;
		private Dictionary<TUser, int> Votes;

		public VotingData(TTarget target, List<TUser> users)
		{
			Target = target;
			Users = users.ToList();
			Votes = new Dictionary<TUser, int>();
			UpdateCanRejectLeadership();
		}

		public void Vote(TUser user, int votes)
		{
			Votes.TryAdd(user, 0);
			Votes[user] += votes;
		}

		public void RemoveFromLeadershipVote(TUser user)
		{
			Users.Remove(user);
			Votes.Clear();

			UpdateCanRejectLeadership();
		}

		private void UpdateCanRejectLeadership()
		{
			bool willAllUsersReject = Users.Any(u => u.WillAcceptLeadership());
			if (!willAllUsersReject)
			{
				CanRejectLeadership = false;
			}
		}

		public List<TUser> GetMostVoted()
		{
			int max = Votes.Values.Max();
			List<TUser> mostVoted = new List<TUser>();
			foreach (var pair in Votes)
			{
				if (pair.Value == max)
				{
					mostVoted.Add(pair.Key);
				}
			}

			return mostVoted;
		}

		public List<TUser> Get(Predicate<TUser> condition)
		{
			return Users.FindAll(condition);
		}

		public List<TUser> GetMax(Func<TUser, int> selector)
		{
			int max = Users.Max(selector);
			return Users.Where(u => selector(u) == max).ToList();
		}

		public List<TUser> GetMax(List<TUser> users, Func<TUser, int> selector)
		{
			int max = users.Max(selector);
			return users.Where(u => selector(u) == max).ToList();
		}

		public TUser VoteForRandom()
		{
			var randomUser = Users[UnityEngine.Random.Range(0, Users.Count)];
			Vote(randomUser, 1);
			return randomUser;
		}

		public int GetVotesForUser(TUser user)
		{
			return Votes.TryGetValue(user, out var voteCount) ? voteCount : 0;
		}
	}

#if UNITY_EDITOR
	public partial class VotingData<TTarget, TUser> where TTarget : FormationTarget<TTarget, TUser>
		where TUser : FormationUser<TTarget, TUser>
	{
		public bool Foldout;

		private List<string> _voteLog = new List<string>();

		public void Log(string message)
		{
			_voteLog.Add(message);
		}

		public List<string> GetLogs()
		{
			return _voteLog;
		}
	}
#endif
}