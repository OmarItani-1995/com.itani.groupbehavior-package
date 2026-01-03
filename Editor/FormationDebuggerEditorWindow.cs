using GroupBehavior.Runtime;
using UnityEngine;
using UnityEditor;

namespace GroupBehavior.Editors
{
	public abstract class FormationDebuggerEditorWindow<TTarget, TUser> : EditorWindow
		where TTarget : FormationTarget<TTarget, TUser> where TUser : FormationUser<TTarget, TUser>
	{
		private FormationsManager<TTarget, TUser> formationsManager;
		private Vector2 scrollPosition;

		protected virtual void OnGUI()
		{
			GUILayout.Label("Formation Editor", EditorStyles.boldLabel);

			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Enter Play Mode to debug formations.", MessageType.Info);
				return;
			}

			if (formationsManager == null)
			{
				formationsManager = EditorGUILayout.ObjectField("Formations Manager:", formationsManager,
					typeof(FormationsManager<TTarget, TUser>), true) as FormationsManager<TTarget, TUser>;
				return;
			}

			var groups = formationsManager.Groups;
			if (groups.Count == 0)
			{
				EditorGUILayout.HelpBox("No formation groups found.", MessageType.Info);
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

		protected virtual void DrawFormationGroupDetails(UnitGroup<TTarget, TUser> group)
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField($"Target: {group.Target.name}, Users in Group: {group.Users.Count}");
			DrawGroupTarget(group);
			DrawGroupLeader(group);
			DrawUsersInGroup(group);
			DrawVotingHistory(group);
			EditorGUILayout.EndVertical();
		}

		private static void DrawGroupTarget(UnitGroup<TTarget, TUser> group)
		{
			EditorGUILayout.ObjectField("Target:", group.Target, typeof(TTarget), true);
		}

		private void DrawUsersInGroup(UnitGroup<TTarget, TUser> group)
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

		protected virtual void DrawUserInGroup(TUser user)
		{
			EditorGUILayout.ObjectField("User:", user, typeof(TUser), true);
		}

		protected virtual void DrawGroupLeader(UnitGroup<TTarget, TUser> group)
		{
			EditorGUILayout.ObjectField("Leader:", group.Leader, typeof(TUser), true);
		}

		protected virtual void DrawVotingHistory(UnitGroup<TTarget, TUser> group)
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

		private void DrawVotingLog(VotingData<TTarget, TUser> votingData)
		{
			EditorGUILayout.LabelField("Voting Log:");
			var logs = votingData.GetLogs();
			foreach (var log in logs)
			{
				EditorGUILayout.LabelField(log);
			}
		}

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