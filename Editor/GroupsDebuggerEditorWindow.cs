using GroupBehavior.Runtime;
using UnityEngine;
using UnityEditor;

namespace GroupBehavior.Editors
{
	/// <summary>
	/// Editor window for debugging groups.
	/// </summary>
	/// <typeparam name="TTarget"></typeparam>
	/// <typeparam name="TUser"></typeparam>
	public abstract class GroupsDebuggerEditorWindow<TTarget, TUser> : EditorWindow
		where TTarget : GroupTarget<TTarget, TUser> where TUser : GroupUser<TTarget, TUser>
	{
		private GroupManager<TTarget, TUser> _groupManager;
		private Vector2 scrollPosition;

		protected virtual void OnGUI()
		{
			GUILayout.Label("Groups Editor", EditorStyles.boldLabel);

			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Enter Play Mode to debug groups.", MessageType.Info);
				return;
			}

			if (_groupManager == null)
			{
				_groupManager = EditorGUILayout.ObjectField("Groups Manager:", _groupManager,
					typeof(GroupManager<TTarget, TUser>), true) as GroupManager<TTarget, TUser>;
				return;
			}

			var groups = _groupManager.Groups;
			if (groups.Count == 0)
			{
				EditorGUILayout.HelpBox("No groups found.", MessageType.Info);
				return;
			}

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			for (int i = 0 ; i < groups.Count; i++)
			{
				var group = groups[i];
				if (group.Foldout = EditorGUILayout.Foldout(group.Foldout, $"Group: {i}"))
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.BeginVertical("box");
					DrawFormationGroupDetails(group);
					EditorGUILayout.EndVertical();
					EditorGUI.indentLevel--;
				}
			}

			EditorGUILayout.EndScrollView();
		}

		/// <summary>
		/// Draws the details of a formation group.
		/// </summary>
		/// <param name="group"></param>
		protected virtual void DrawFormationGroupDetails(Group<TTarget, TUser> group)
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField($"Target: {group.Target.name}, Users in Group: {group.Users.Count}");
			DrawGroupTarget(group);
			DrawGroupLeader(group);
			DrawUsersInGroup(group);
			DrawVotingHistory(group);
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Draws the target of the group.
		/// </summary>
		/// <param name="group"></param>
		protected virtual void DrawGroupTarget(Group<TTarget, TUser> group)
		{
			EditorGUILayout.ObjectField("Target:", group.Target, typeof(TTarget), true);
		}

		/// <summary>
		/// Draws the users in the group.
		/// </summary>
		/// <param name="group"></param>
		private void DrawUsersInGroup(Group<TTarget, TUser> group)
		{
			EditorGUILayout.LabelField("Users:");
			EditorGUI.indentLevel++;

			foreach (var user in group.Users)
			{
				EditorGUILayout.BeginHorizontal();
				DrawUserInGroup(user);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUI.indentLevel--;

		}

		/// <summary>
		/// Draws a single user in the group.
		/// </summary>
		/// <param name="user"></param>
		protected virtual void DrawUserInGroup(TUser user)
		{
			EditorGUILayout.ObjectField("User:", user, typeof(TUser), true);
		}

		/// <summary>
		/// Draws the leader of the group.
		/// </summary>
		/// <param name="group"></param>
		protected virtual void DrawGroupLeader(Group<TTarget, TUser> group)
		{
			EditorGUILayout.ObjectField("Leader:", group.Leader, typeof(TUser), true);
		}

		/// <summary>
		/// Draws the voting history of the group.
		/// </summary>
		/// <param name="group"></param>
		protected virtual void DrawVotingHistory(Group<TTarget, TUser> group)
		{
			if (group.VotingFoldout = EditorGUILayout.Foldout(group.VotingFoldout, "Voting History"))
			{
				EditorGUI.indentLevel++;
				for (int i = 0; i < group.VotingDataHistory.Count; i++)
				{
					var votingData = group.VotingDataHistory[i];
					if (votingData.Foldout = EditorGUILayout.Foldout(votingData.Foldout, $"Voting Data {i + 1}"))
					{
						DrawVotingDataDetails(votingData);
						DrawVotingLog(votingData);
					}
				}

				EditorGUI.indentLevel--;
			}
		}

		/// <summary>
		/// Draws the voting log for a given voting data.
		/// </summary>
		/// <param name="votingData"></param>
		private void DrawVotingLog(VotingData<TTarget, TUser> votingData)
		{
			EditorGUILayout.LabelField("Voting Log:");
			var logs = votingData.GetLogs();
			foreach (var log in logs)
			{
				EditorGUILayout.LabelField(log);
			}
		}

		/// <summary>
		///	Draws the details of a voting data.
		/// </summary>
		/// <param name="votingData"></param>
		protected virtual void DrawVotingDataDetails(VotingData<TTarget, TUser> votingData)
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Users and Votes:");
			foreach (var user in votingData.Users)
			{
				int votes = votingData.GetVotesForUser(user);
				EditorGUILayout.LabelField($"User: {user.name}, Votes: {votes}");
			}

			EditorGUILayout.EndVertical();
		}
	}
}