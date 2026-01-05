using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GroupBehavior.Runtime
{
    /// <summary>
    /// Main group controller, owns subsystems: roster, leadership, formation.
    /// </summary>
    public abstract partial class Group<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        public TTarget Target { get; set; }
        public List<TUser> Users => Roster.Users;
        public TUser Leader => Leadership.Leader;

        public bool HasLeader => Leadership.HasLeader;

        protected GroupRoster<TTarget, TUser> Roster { get; private set; }
        protected GroupLeadership<TTarget, TUser> Leadership { get; private set; }
        protected GroupFormation<TTarget, TUser> Formation { get; private set; }
        protected GroupScheduler GroupScheduler { get; private set; }

        /// <summary>
        /// Initializes the group and its subsystems.
        /// Hooks up necessary event listeners.
        /// </summary>
        public virtual void Init()
        {
            GroupScheduler = GetUpdateScheduler();

            Roster = GetGroupRoster();

            Leadership = GetGroupLeadership();

            Formation = GetFormationController();

            Roster.UserAdded += Formation.OnUserAdded;
            Roster.UserRemoved += Formation.OnUserRemoved;
            Roster.UsersAdded += Formation.OnUsersAdded;

            Roster.UserRemoved += Leadership.OnUserRemoved;
            Leadership.LeaderChanged += Formation.OnLeaderChanged;
            Leadership.LeaderCleared += Formation.OnLeaderCleared;

#if UNITY_EDITOR
            Leadership.VotingDataProduced += AddVotingDataToHistory;
#endif
        }
        
        public void Update()
        {
            GroupScheduler.Tick();
            OnUpdate();
        }

        protected virtual void OnUpdate() { }

        /// <summary>
        /// Starts the initial voting process to elect a leader.
        /// </summary>
        public void StartInitialVotingProcess() => Leadership.StartInitialVoting();

        /// <summary>
        /// Starts a custom voting process to elect a leader.
        /// </summary>
        /// <param name="process"></param>
        public void StartVotingProcess(GroupLeaderVotingProcess<TTarget, TUser> process) => Leadership.StartVoting(process);

        /// <summary>
        /// Adds a user to the group.
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(TUser user) => Roster.AddUser(user);

        /// <summary>
        /// Adds multiple users to the group.
        /// </summary>
        /// <param name="users"></param>
        public void AddUsers(List<TUser> users) => Roster.AddUsers(users);

        /// <summary>
        /// Removes a user from the group.
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(TUser user) => Roster.RemoveUser(user);

        /// <summary>
        /// Sets the leader directly, bypassing voting.
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        public Task SetLeaderAsync(TUser leader) => Leadership.SetLeaderAsync(leader);

        /// <summary>
        /// Adds an update action to be called every update tick.
        /// </summary>
        /// <param name="update"></param>
        public void AddToUpdate(Action update) => GroupScheduler.Add(update);

        /// <summary>
        /// Removes an update action from being called every update tick.
        /// </summary>
        /// <param name="update"></param>
        public void RemoveFromUpdate(Action update) => GroupScheduler.Remove(update);

        /// <summary>
        /// Selects Update Scheduler Implementation to Create.
        /// </summary>
        /// <returns></returns>
        protected virtual GroupScheduler GetUpdateScheduler()
            => new GroupScheduler();
        
        /// <summary>
        /// Selects Group Roster Implementation to Create.
        /// </summary>
        /// <returns></returns>
        protected virtual GroupRoster<TTarget, TUser> GetGroupRoster()
            => new GroupRoster<TTarget, TUser>(this);
        
        /// <summary>
        /// Selects Group Leadership Implementation to Create.
        /// </summary>
        /// <returns></returns>
        protected virtual GroupLeadership<TTarget, TUser> GetGroupLeadership()
            => new GroupLeadership<TTarget, TUser>(this);
        
        /// <summary>
        /// Selects Group Formation Implementation to Create.
        /// </summary>
        /// <returns></returns>
        protected virtual GroupFormation<TTarget, TUser> GetFormationController()
            => new GroupFormation<TTarget, TUser>();
    }

    public class GroupManualInitialization<TTarget, TUser> : Group<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    { }
}

#if UNITY_EDITOR
namespace GroupBehavior.Runtime
{
    public abstract partial class Group<TTarget, TUser>
        where TTarget : GroupTarget<TTarget, TUser>
        where TUser   : GroupUser<TTarget, TUser>
    {
        public bool Foldout;
        public bool VotingFoldout;

        public List<VotingData<TTarget, TUser>> VotingDataHistory { get; } = new();

        public void AddVotingDataToHistory(VotingData<TTarget, TUser> votingData)
        {
            if (votingData == null) return;
            VotingDataHistory.Add(votingData);
        }
    }
}
#endif