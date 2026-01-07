using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GroupBehavior.Runtime
{
    /// <summary>
    /// Base leader voting process.
    /// Responsibilities:
    /// - Create VotingData
    /// - Collect votes from users
    /// - Resolve ties / conflicts
    /// - Assign leader (with optional rejection)
    ///
    /// Concrete implementations decide *when* voting starts (immediate, delayed, etc.).
    /// </summary>
    [Serializable]
    public abstract class GroupLeaderVotingProcess<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        public Group<TTarget, TUser> Group { get; }

        private readonly List<Task> _voteTasks = new(32);
        private CancellationTokenSource _cancellationTokenSource;
        protected GroupLeaderVotingProcess(Group<TTarget, TUser> group)
        {
            Group = group ?? throw new ArgumentNullException(nameof(group));
        }
        
        /// <summary>
        /// Initializes and starts the voting process.
        /// </summary>
        /// <returns></returns>
        protected abstract Task InitializeVotingProcessAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Initializes and starts the voting process
        /// </summary>
        public async Task InitializeAndStartAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                Debug.Log("Previous voting process cancelled.");
            }
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            try
            {
                await InitializeVotingProcessAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Starts the voting process.
        /// </summary>
        /// <returns></returns>
        protected Task StartVotingProcessAsync(CancellationToken cancellationToken)
        {
            var votingData = CreateVotingData();

#if UNITY_EDITOR
            Group.AddVotingDataToHistory(votingData);
#endif

            return VoteForLeaderAsync(votingData, cancellationToken);
        }

        /// <summary>
        /// Conducts the voting process among group users.
        /// </summary>
        /// <param name="votingData"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual async Task VoteForLeaderAsync(VotingData<TTarget, TUser> votingData, CancellationToken cancellationToken)
        {
            if (votingData == null) throw new ArgumentNullException(nameof(votingData));
            _voteTasks.Clear();

            var users = Group.Users;
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                if (user == null) continue;

                _voteTasks.Add(user.VoteForLeaderAsync(votingData, cancellationToken));
            }

            await Task.WhenAll(_voteTasks);
            
            var mostVoted = votingData.GetMostVoted();
            await ResolveMostVotedAsync(votingData, mostVoted, cancellationToken);
        }

        /// <summary>
        /// Resolves the most voted users, handling conflicts if necessary.
        /// </summary>
        /// <param name="votingData"></param>
        /// <param name="mostVoted"></param>
        protected virtual Task ResolveMostVotedAsync(VotingData<TTarget, TUser> votingData, List<TUser> mostVoted, CancellationToken cancellationToken)
        {
            if (mostVoted == null || mostVoted.Count == 0)
            {
#if UNITY_EDITOR
                votingData?.Log("No votes returned. Voting process aborted.");
#endif
                return Task.CompletedTask;
            }

            if (mostVoted.Count == 1)
            {
#if UNITY_EDITOR
                votingData.Log($"No conflict, leader elected: {SafeName(mostVoted[0])}");
#endif
                return TrySetLeaderAsync(votingData, mostVoted[0], cancellationToken);
            }

#if UNITY_EDITOR
            votingData.Log($"Conflict detected among: {string.Join(", ", mostVoted.Select(SafeName))}");
#endif
            return ResolveConflictAsync(votingData, mostVoted, cancellationToken);
        }

        /// <summary>
        /// Resolves conflicts among tied users. Default implementation selects randomly.
        /// </summary>
        /// <param name="votingData"></param>
        /// <param name="tiedUsers"></param>
        protected virtual async Task ResolveConflictAsync(VotingData<TTarget, TUser> votingData, List<TUser> tiedUsers, CancellationToken cancellationToken)
        {
            if (tiedUsers == null || tiedUsers.Count == 0) return;
            cancellationToken.ThrowIfCancellationRequested();
            
            int index = UnityEngine.Random.Range(0, tiedUsers.Count);
            var winner = tiedUsers[index];

#if UNITY_EDITOR
            votingData.Log($"Resolving conflict by random selection: {SafeName(winner)}");
#endif

            await TrySetLeaderAsync(votingData, winner, cancellationToken);
        }

        /// <summary>
        /// Attempts to set a leader. If leadership can be rejected and user rejects, continues voting.
        /// </summary>
        protected virtual Task TrySetLeaderAsync(VotingData<TTarget, TUser> votingData, TUser candidate, CancellationToken cancellationToken)
        {
            if (candidate == null) return Task.CompletedTask;
            
            // Fast path: rejection disabled
            if (!votingData.CanRejectLeadership)
            {
#if UNITY_EDITOR
                votingData.Log($"User automatically accepted leadership (no rejection allowed): {SafeName(candidate)}");
#endif
                return Group.SetLeaderAsync(candidate, cancellationToken);
            }

            // Candidate decision
            if (candidate.WillAcceptLeadership())
            {
#if UNITY_EDITOR
                votingData.Log($"User accepted leadership: {SafeName(candidate)}");
#endif
                return Group.SetLeaderAsync(candidate, cancellationToken);
            }

#if UNITY_EDITOR
            votingData.Log($"User declined leadership: {SafeName(candidate)}");
#endif
            return HandleLeaderDeclinedAsync(votingData, candidate, cancellationToken);
        }

        /// <summary>
        /// Handles the scenario where a candidate declines leadership.
        /// </summary>
        /// <param name="votingData"></param>
        /// <param name="candidate"></param>
        protected virtual async Task HandleLeaderDeclinedAsync(VotingData<TTarget, TUser> votingData, TUser candidate, CancellationToken cancellationToken)
        {
            await candidate.DeclinedLeadershipAsync(cancellationToken);

            votingData.RemoveFromLeadershipVote(candidate);

            // Continue voting with updated data (recursive flow preserved)
            await VoteForLeaderAsync(votingData, cancellationToken);
        }

        /// <summary>
        /// Creates a new VotingData instance.
        /// </summary>
        /// <returns></returns>
        protected virtual VotingData<TTarget, TUser> CreateVotingData()
            => new VotingData<TTarget, TUser>(Group.Target, Group.Users);

        private static string SafeName(TUser user)
            => user != null ? user.gameObject.name : "<null>";
    }

    [Serializable]
    public sealed class GroupLeaderVotingProcessDelayed<TTarget, TUser> : GroupLeaderVotingProcess<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        public float DelayBeforeVote;

        public GroupLeaderVotingProcessDelayed(Group<TTarget, TUser> group, float delayBeforeVote)
            : base(group)
        {
            DelayBeforeVote = delayBeforeVote;
        }

        protected override async Task InitializeVotingProcessAsync(CancellationToken cancellationToken)
        {
            await Awaitable.WaitForSecondsAsync(DelayBeforeVote, cancellationToken);
            await StartVotingProcessAsync(cancellationToken);
        }
    }

    [Serializable]
    public sealed class GroupLeaderVotingProcessImmediate<TTarget, TUser> : GroupLeaderVotingProcess<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        public GroupLeaderVotingProcessImmediate(Group<TTarget, TUser> group) : base(group) { }

        protected override Task InitializeVotingProcessAsync(CancellationToken cancellationToken)
            => StartVotingProcessAsync(cancellationToken);
    }
}
