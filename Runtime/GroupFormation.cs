using System.Collections.Generic;

namespace GroupBehavior.Runtime
{
    /// <summary>
    /// Owns formation lifecycle and transitions.
    /// </summary>
    public class GroupFormation<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser : GroupUser<TTarget, TUser>
    {
        public Formation<TTarget, TUser> CurrentFormation { get; private set; }

        /// <summary>
        /// Called when the group leader changes.
        /// </summary>
        /// <param name="oldLeader"></param>
        /// <param name="newLeader"></param>
        public virtual void OnLeaderChanged(TUser oldLeader, TUser newLeader)
        {
            UpdateFormationFromLeader(newLeader);
        }

        /// <summary>
        /// Called when the group leader is cleared.
        /// </summary>
        /// <param name="oldLeader"></param>
        public virtual void OnLeaderCleared(TUser oldLeader)
        {
            SetFormation(null);
        }

        /// <summary>
        /// Called when a user is added to the group.
        /// </summary>
        /// <param name="user"></param>
        public void OnUserAdded(TUser user)
        {
            CurrentFormation?.OnUserAdded(user);
        }

        /// <summary>
        /// Called when multiple users are added to the group.
        /// </summary>
        /// <param name="users"></param>
        public void OnUsersAdded(List<TUser> users)
        {
            CurrentFormation?.OnUsersAdded(users);
        }

        /// <summary>
        /// Called when a user is removed from the group.
        /// </summary>
        /// <param name="user"></param>
        public void OnUserRemoved(TUser user)
        {
            CurrentFormation?.OnUserRemoved(user);
        }

        private void UpdateFormationFromLeader(TUser leader)
        {
            if (leader == null)
            {
                SetFormation(null);
                return;
            }

            var formation = leader.GetFormation();
            SetFormation(formation);
        }

        /// <summary>
        /// Sets the current formation, handling lifecycle transitions.
        /// </summary>
        /// <param name="formation"></param>
        protected virtual void SetFormation(Formation<TTarget, TUser> formation)
        {
            if (ReferenceEquals(CurrentFormation, formation))
                return;

            CurrentFormation?.EndFormation();
            CurrentFormation = formation;
            CurrentFormation?.StartFormation();
        }
    }
}