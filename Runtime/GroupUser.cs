using System;
using System.Threading.Tasks;
using GroupBehavior.Runtime.Utility;
using UnityEngine;

namespace GroupBehavior.Runtime
{
    public abstract class GroupUser : MonoBehaviour { }

    public abstract class GroupUser<TTarget, TUser> : GroupUser
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        public const int UnassignedFormationIndex = -1;

        public FormationSO FormationSO;

        public int FormationIndex { get; private set; } = UnassignedFormationIndex;

        protected Group<TTarget, TUser> Group { get; private set; }

        public bool IsInGroup => Group != null;

        public virtual void SetGroup(Group<TTarget, TUser> group)
        {
            Group = group ?? throw new ArgumentNullException(nameof(group));
        }

        /// <summary>
        /// Request removal from the current group.
        /// </summary>
        public virtual void Unsubscribe()
        {
            if (Group == null) return;
            Group.RemoveUser((TUser)this);
        }

        /// <summary>
        /// Called before the leadership appointment process begins.
        /// </summary>
        /// <returns></returns>
        public virtual Task OnBeforeAppointingLeaderAsync() => Task.CompletedTask;

        /// <summary>
        /// Called after being added to a group builder, before the group is finalized.
        /// </summary>
        /// <returns></returns>
        public virtual Task OnAddedToGroupBuilderAsync() => Task.CompletedTask;

        /// <summary>
        /// Called to vote for a leader among candidates.
        /// </summary>
        /// <param name="votingData"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task VoteForLeaderAsync(VotingData<TTarget, TUser> votingData)
        {
            if (votingData == null) throw new ArgumentNullException(nameof(votingData));

            var votedFor = votingData.VoteForRandom();

#if UNITY_EDITOR
            if (votedFor != null)
                FormationUtility.DrawVoteLines(transform, votedFor.transform, 1f);
#endif

            // Default behavior: simulate thinking time.
            await Awaitable.WaitForSecondsAsync(1f);
        }

        /// <summary>
        /// Called when promoted to leader of the group.
        /// </summary>
        /// <returns></returns>
        public virtual Task PromotedToLeaderAsync()
        {
            Debug.Log($"Promoted to Leader: {name}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a new leader has been appointed.
        /// New Leaders dont receive this call.
        /// </summary>
        /// <param name="newLeader"></param>
        /// <returns></returns>
        public virtual Task NewLeaderAppointedAsync(TUser newLeader) => Task.CompletedTask;

        /// <summary>
        /// Called to determine if this user will accept leadership.
        /// </summary>
        /// <returns></returns>
        public virtual bool WillAcceptLeadership() => true;

        /// <summary>
        /// Called when this user has declined leadership.
        /// </summary>
        /// <returns></returns>
        public virtual Task DeclinedLeadershipAsync()
        {
            Debug.Log($"Declined Leadership: {name}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the formation index for this user.
        /// </summary>
        /// <param name="index"></param>
        public virtual void SetFormationIndex(int index)
        {
            FormationIndex = index;
        }

        /// <summary>
        /// Sets the formation position for this user.
        /// </summary>
        /// <param name="position"></param>
        public virtual void SetFormationPosition(Vector3 position)
        {
            
        }

        /// <summary>
        /// Creates a formation instance based on the assigned FormationSO.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Formation<TTarget, TUser> GetFormation()
        {
            if (Group == null)
                throw new InvalidOperationException($"{name}: Cannot create formation without a Group reference.");

            if (FormationSO == null)
                throw new InvalidOperationException($"{name}: FormationSO is not assigned.");

            return FormationSO.CreateFormation(Group);
        }
    }
}
