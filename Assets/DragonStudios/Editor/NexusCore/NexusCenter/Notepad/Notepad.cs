using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DraconisNexus
{
    public class NotepadWindow : EditorWindow
    {
        private string _notes = "";
        private Vector2 _scrollPosition;
        private string _savePath;
        private const string DefaultFileName = "draconis_notes.txt";
        private string _notepadFolder;

        public static void ShowWindow()
        {
            var window = GetWindow<NotepadWindow>("Notepad");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // Get the directory where this script is located
            string[] guids = AssetDatabase.FindAssets("Notepad t:Script");
            if (guids.Length > 0)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                _notepadFolder = Path.GetDirectoryName(scriptPath);
                
                // Create the directory if it doesn't exist
                if (!Directory.Exists(_notepadFolder))
                {
                    Directory.CreateDirectory(_notepadFolder);
                }
                
                _savePath = Path.Combine(_notepadFolder, DefaultFileName);
                LoadNotes();
            }
            else
            {
                Debug.LogError("Could not find Notepad script path. Using default location.");
                _notepadFolder = Application.persistentDataPath;
                _savePath = Path.Combine(_notepadFolder, DefaultFileName);
            }
        }

        private void OnDisable()
        {
            SaveNotes();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Notepad", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Save/Load buttons
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Width(100)))
                {
                    SaveNotes();
                }

                if (GUILayout.Button("Load", GUILayout.Width(100)))
                {
                    LoadNotes();
                }

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Clear Notepad", "Are you sure you want to clear all text?", "Yes", "No"))
                    {
                        _notes = string.Empty;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Text area with scroll view
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                var style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    fontSize = 14,
                    padding = new RectOffset(10, 10, 10, 10)
                };

                _notes = EditorGUILayout.TextArea(_notes, style, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();

            // File path info (show relative path for better readability)
            string relativePath = "Assets" + _savePath.Replace(Application.dataPath, "");
            EditorGUILayout.HelpBox($"Notes are saved to: {relativePath}", MessageType.Info);
        }

        private void SaveNotes()
        {
            try
            {
                File.WriteAllText(_savePath, _notes);
                Debug.Log($"Notes saved to: {_savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save notes: {e.Message}");
            }
        }

        private void LoadNotes()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    _notes = File.ReadAllText(_savePath);
                    Debug.Log($"Notes loaded from: {_savePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load notes: {e.Message}");
            }
        }
    }
}