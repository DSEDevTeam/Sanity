using UnityEditor;
using UnityEngine;

namespace DraconisNexus
{
    public class ChangeLogWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = GetWindow<ChangeLogWindow>("Changelog");
            window.titleContent = new GUIContent("Changelog");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private Vector2 _scrollPosition;

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Change Log", EditorStyles.boldLabel);
            
            var versionStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };
            var entryStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, wordWrap = true, padding = new RectOffset(15, 5, 0, 0) };
            
            // Version 1.0.1
            EditorGUILayout.Space(5);
            GUILayout.Label("Version 1.0.1 - UI Improvements", versionStyle);
            EditorGUI.indentLevel++;
            GUILayout.Label("• Improved window layout and sizing", entryStyle);
            GUILayout.Label("• Enhanced drag and drop functionality in Folder Forge", entryStyle);
            GUILayout.Label("• Added changelog to Hub tab", entryStyle);
            GUILayout.Label("• Updated the Folder Forge tool, and called it Folder Creator", entryStyle);
            GUILayout.Label("• Made each tool a separate window making the hub a launcher", entryStyle);
            EditorGUI.indentLevel--;
            
            // Version 1.0.0
            EditorGUILayout.Space(10);
            GUILayout.Label("Version 1.0.0 - Initial Release", versionStyle);
            EditorGUI.indentLevel++;
            GUILayout.Label("• Added Folder Forge tool for easy folder structure creation", entryStyle);
            GUILayout.Label("• Added Notepad for quick note-taking within Unity", entryStyle);
            GUILayout.Label("• Added To-Do List for task management", entryStyle);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndScrollView();
        }
    }
}
