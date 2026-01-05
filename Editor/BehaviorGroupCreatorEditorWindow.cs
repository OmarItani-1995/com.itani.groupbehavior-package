using System.IO;
using UnityEditor;
using UnityEngine;

namespace GroupBehavior.Editors
{
	public class BehaviorGroupCreatorEditorWindow : EditorWindow
	{
		[MenuItem("Window/Group Behavior/Behavior Group Creator")]
		public static void ShowWindow()
		{
			var window = GetWindow<BehaviorGroupCreatorEditorWindow>("Behavior Group Creator");
			window.Show();
		}

		private string nameSpace;
		private string targetName;
		public string userName;
		public string managerName;
		public string debuggerWindow => $"{managerName}DebuggerWindow";

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Create Behavior Group Classes", EditorStyles.boldLabel);
			nameSpace = EditorGUILayout.TextField("Namespace (optional):", nameSpace);
			targetName = EditorGUILayout.TextField("Target Class Name:", targetName);
			userName = EditorGUILayout.TextField("User Class Name:", userName);
			managerName = EditorGUILayout.TextField("Manager Class Name:", managerName);

			if (GUILayout.Button("Create Classes"))
			{
				if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(userName) ||
				    string.IsNullOrEmpty(managerName))
				{
					EditorUtility.DisplayDialog("Error", "Please fill in all class names.", "OK");
					return;
				}

				CreateBehaviorGroupClasses();
				EditorUtility.DisplayDialog("Success", "Behavior group classes created successfully.", "OK");
			}
		}

		private void CreateBehaviorGroupClasses()
		{
			string path = EditorUtility.OpenFolderPanel("Select Folder to Save Classes", "Assets", "");
			if (string.IsNullOrEmpty(path))
			{
				Debug.Log("Class creation canceled: No folder selected.");
				return;
			}

			File.WriteAllText(Path.Combine(path, $"{targetName}.cs"), GetTargetClassString());
			File.WriteAllText(Path.Combine(path, $"{userName}.cs"), GetUserClassString());
			File.WriteAllText(Path.Combine(path, $"{managerName}.cs"), GetManagerClassString());
			File.WriteAllText(Path.Combine(path, $"{managerName}DebuggerWindow.cs"),
				GetDebuggerWindowClassString());
			AssetDatabase.Refresh();
		}

		private string GetTargetClassString()
		{
			return new ClassBuilder()
				.AddUsing("GroupBehavior.Runtime")
				.SetNamespace(nameSpace)
				.SetClassName(targetName)
				.SetBaseClass("GroupTarget", targetName, userName)
				.Build();
		}

		private string GetUserClassString()
		{
			return new ClassBuilder()
				.AddUsing("GroupBehavior.Runtime")
				.SetNamespace(nameSpace)
				.SetClassName(userName)
				.SetBaseClass("GroupUser", targetName, userName)
				.Build();
		}

		private string GetManagerClassString()
		{
			return new ClassBuilder()
				.AddUsing("GroupBehavior.Runtime")
				.SetNamespace(nameSpace)
				.SetClassName(managerName)
				.SetBaseClass("GroupManager", targetName, userName)
				.Build();
		}

		public string GetDebuggerWindowClassString()
		{
			return new ClassBuilder()
				.AddUsing("GroupBehavior.Editors")
				.AddUsing("UnityEditor")
				.AddUsing("UnityEngine")
				.SetNamespace(nameSpace)
				.SetClassName(debuggerWindow)
				.SetBaseClass("GroupsDebuggerEditorWindow", targetName, userName)
				.AddFunction(
					builder => builder
						.SetName("ShowWindow")
						.SetModifier("public static")
						.AddAttribute("MenuItem(\"Group Behavior/" + managerName + "-Debugger\")")
						.AddBodyLine($"var window = GetWindow<{debuggerWindow}>();")
						.AddBodyLine($"window.titleContent = new GUIContent(\"{managerName} Debugger\");")
						.AddBodyLine("window.Show();")
				)
				.Build();
		}
	}
}