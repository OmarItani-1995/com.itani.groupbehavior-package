using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GroupBehavior.Runtime
{
    public abstract partial class UnitGroup<TTarget, TUser> where TTarget : FormationTarget<TTarget, TUser>
        where TUser : FormationUser<TTarget, TUser>
    {
        public TTarget Target;
        public List<TUser> Users = new List<TUser>();
        public TUser Leader;

        private List<Action> _updateActions = new List<Action>();
        protected GroupLeaderVotingProcess<TTarget, TUser> _currentVotingProcess;
        protected Formation<TTarget, TUser> _formation;

        public void Init()
        {
            if (Leader == null)
                SetVotingProcess(GetInitialVotingProcess());
        }

        public virtual void AddUser(TUser user)
        {
            Users.Add(user);
            user.SetGroup(this);
            _formation?.OnUserAdded(user);
        }
        
        public void AddUsers(List<TUser> users)
        {
            Users.AddRange(users);
            foreach (var user in users)
            {
                user.SetGroup(this);
            }
            _formation?.OnUsersAdded(users);
        }

        public virtual void RemoveUser(TUser user)
        {
            Users.Remove(user);
            if (user == Leader)
            {
                Leader = null;
                SetFormation(null);
            }

            if (Users.Count == 0)
            {
                FormationsManager<TTarget, TUser>.Instance.RemoveUnitGroup(this);
                return;
            }

            if (Leader == null)
                SetVotingProcess(GetLeaderRemovedProcess());
            else
                _formation?.OnUserRemoved(user);

        }

        public void SetVotingProcess(GroupLeaderVotingProcess<TTarget, TUser> process)
        {
            _currentVotingProcess = process;
        }

        public void Update()
        {
            for (int i = _updateActions.Count - 1; i >= 0; i--)
            {
                _updateActions[i].Invoke();
            }

            OnUpdate();
        }

        protected virtual void OnUpdate()
        {
        }

        public void AddToUpdate(Action update)
        {
            _updateActions.Add(update);
        }

        public void RemoveFromUpdate(Action update)
        {
            if (!_updateActions.Contains(update)) return;
            _updateActions.Remove(update);
        }

        public virtual GroupLeaderVotingProcess<TTarget, TUser> GetInitialVotingProcess()
        {
            return new GroupLeaderVotingProcessDelayed<TTarget, TUser>(this, 1);
        }

        public virtual GroupLeaderVotingProcess<TTarget, TUser> GetLeaderRemovedProcess()
        {
            return new GroupLeaderVotingProcessImmediate<TTarget, TUser>(this);
        }

        public virtual async Task SetLeaderAsync(TUser formationUser)
        {
            Leader = formationUser;
            await Leader.PromotedToLeaderAsync();
            List<Task> tasks = new List<Task>();
            foreach (var user in Users)
            {
                if (user != Leader)
                {
                    tasks.Add(user.NewLeaderAppointedAsync(Leader));
                }
            }

            await Task.WhenAll(tasks);
            UpdateFormation();
        }

        protected virtual void UpdateFormation()
        {
            var formation = Leader.GetFormation(this);
            SetFormation(formation);
        }

        protected virtual void SetFormation(Formation<TTarget, TUser> formation)
        {
            _formation?.EndFormation();
            _formation = formation;
            _formation?.StartFormation();
        }
    }


#if UNITY_EDITOR
public abstract partial class UnitGroup<TTarget, TUser> where TTarget : FormationTarget<TTarget, TUser>
    where TUser : FormationUser<TTarget, TUser>
{
    public bool Foldout;
    public bool VotingFoldout;
    
    public List<VotingData<TTarget, TUser>> VotingDataHistory = new List<VotingData<TTarget, TUser>>();
    public void AddVotingDataToHistory(VotingData<TTarget,TUser> votingData)
    {
        VotingDataHistory.Add(votingData);
    }
}
#endif

    public class UnitGroupSingle<TTarget, TUser> : UnitGroup<TTarget, TUser>
        where TTarget : FormationTarget<TTarget, TUser> where TUser : FormationUser<TTarget, TUser>
    {

    }
}