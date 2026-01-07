using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GroupBehavior.Runtime
{
	/// <summary>
    /// Owns leader state + voting process.
    /// </summary>
    public class GroupLeadership<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        private readonly Group<TTarget, TUser> _group;
        private GroupLeaderVotingProcess<TTarget, TUser> _currentVotingProcess;

        private readonly List<Task> _notifyTasks = new(32);

        public TUser Leader { get; private set; }
        public bool HasLeader => Leader != null;

        public event Action<TUser, TUser> LeaderChanged; // (old, new)
        public event Action<TUser> LeaderCleared;

        public event Action<VotingData<TTarget, TUser>> VotingDataProduced;

        public GroupLeadership(Group<TTarget, TUser> group)
        {
            _group = group ?? throw new ArgumentNullException(nameof(group));
        }
        
        /// <summary>
        /// Creates the initial voting process.
        /// </summary>
        /// <returns></returns>
        protected virtual GroupLeaderVotingProcess<TTarget, TUser> CreateInitialVotingProcess()
            => new GroupLeaderVotingProcessImmediate<TTarget, TUser>(_group);

        /// <summary>
        /// Creates the voting process to appoint a new leader after the current one is removed.
        /// </summary>
        /// <returns></returns>
        protected virtual GroupLeaderVotingProcess<TTarget, TUser> CreateLeaderRemovedProcess()
            => new GroupLeaderVotingProcessImmediate<TTarget, TUser>(_group);

        /// <summary>
        /// Starts the initial voting process to elect a leader.
        /// </summary>
        public void StartInitialVoting()
        {
            SetVotingProcess(CreateInitialVotingProcess());
        }

        /// <summary>
        /// Starts a custom voting process.
        /// </summary>
        /// <param name="process"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void StartVoting(GroupLeaderVotingProcess<TTarget, TUser> process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            SetVotingProcess(process);
        }

        /// <summary>
        /// Sets and starts the given voting process.
        /// </summary>
        /// <param name="process"></param>
        protected virtual void SetVotingProcess(GroupLeaderVotingProcess<TTarget, TUser> process)
        {
            _currentVotingProcess = process;
            _currentVotingProcess.InitializeAndStartAsync();
        }
        
        /// <summary>
        /// Restarts the current voting process.
        /// </summary>
        public void RestartCurrentVoting()
        {
            if (_currentVotingProcess == null) return;
            SetVotingProcess(_currentVotingProcess);   
        }

        public void PublishVotingData(VotingData<TTarget, TUser> votingData)
        {
            VotingDataProduced?.Invoke(votingData);
        }

        /// <summary>
        /// Sets the leader directly, bypassing voting.
        /// </summary>
        /// <param name="newLeader"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SetLeaderAsync(TUser newLeader, CancellationToken cancellationToken)
        {
            if (newLeader == null) throw new ArgumentNullException(nameof(newLeader));
            if (!_group.Users.Contains(newLeader))
                throw new InvalidOperationException("Cannot set leader: user is not a member of this group.");

            if (ReferenceEquals(Leader, newLeader))
                return;

            var oldLeader = Leader;
            Leader = newLeader;

            _notifyTasks.Clear();

            var users = _group.Users;
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                if (user == null || ReferenceEquals(user, Leader)) continue;
                _notifyTasks.Add(user.NewLeaderAppointedAsync(Leader));
            }

            _notifyTasks.Add(Leader.PromotedToLeaderAsync());
            await Task.WhenAll(_notifyTasks);

            LeaderChanged?.Invoke(oldLeader, Leader);
        }

        public virtual void OnUserRemoved(TUser removedUser)
        {
            if (removedUser == null) return;
            if (!ReferenceEquals(removedUser, Leader)) return;

            var oldLeader = Leader;
            Leader = null;

            LeaderCleared?.Invoke(oldLeader);

            SetVotingProcess(CreateLeaderRemovedProcess());
        }
    }
}