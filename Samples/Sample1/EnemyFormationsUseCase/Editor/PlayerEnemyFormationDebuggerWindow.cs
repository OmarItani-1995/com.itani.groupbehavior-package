using GroupBehavior.Samples.Runtime;
using UnityEditor;
using UnityEngine;

namespace GroupBehavior.Editor.Example
{
	public class
		PlayerEnemyFormationDebuggerWindow : FormationDebuggerEditorWindow<PlayerFormationTarget, EnemyFormationUser>
	{
		[MenuItem("Window/Player Enemy Formation Debugger")]
		public static void ShowWindow()
		{
			var window = GetWindow<PlayerEnemyFormationDebuggerWindow>();
			window.titleContent = new GUIContent("Player Enemy Formation Debugger");
			window.Show();
		}
	}
}