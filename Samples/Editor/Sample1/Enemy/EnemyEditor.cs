using GroupBehavior.Samples.Runtime;
using UnityEditor;
using UnityEngine;

namespace GroupBehavior.Samples.Editors
{
    [CustomEditor(typeof(Enemy))]
    public class EnemyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Enemy enemy = (Enemy)target;

            if (GUILayout.Button("Die"))
            {
                enemy.Die();
            }
        }
    }
}