using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace DraconisNexus
{
    public class PrefabCreatorWindow : EditorWindow
    {
        private GameObject selectedObject;
        private string prefabName = "NewPrefab";
        private string savePath = "Assets/Prefabs";
        private bool createAsVariant = false;
        private Vector2 scrollPosition;
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabCreatorWindow>("Prefab Creator");
            window.minSize = new Vector2(350, 200);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Prefab Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Select an object in the hierarchy and click 'Create Prefab' to save it as a prefab.", MessageType.Info);
            
            // Object field for the selected object
            selectedObject = (GameObject)EditorGUILayout.ObjectField("Object to Prefab", selectedObject, typeof(GameObject), true);
            
            // Prefab name field
            prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
            
            // Save path field with folder selection
            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string newPath = EditorUtility.SaveFolderPanel("Select Prefab Save Location", "Assets", "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    // Convert to a path relative to the project
                    savePath = "Assets" + newPath.Replace(Application.dataPath, "");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Toggle for prefab variant
            createAsVariant = EditorGUILayout.Toggle("Create as Variant", createAsVariant);
            
            // Create button
            EditorGUILayout.Space(15);
            using (new EditorGUI.DisabledScope(selectedObject == null || string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(savePath)))
            {
                if (GUILayout.Button("Create Prefab", GUILayout.Height(30)))
                {
                    CreatePrefab();
                }
            }
            
            // Display help box if no object is selected
            if (selectedObject == null)
            {
                EditorGUILayout.HelpBox("Please select an object from the scene hierarchy.", MessageType.Warning);
            }
        }
        
        private void CreatePrefab()
        {
            try
            {
                // Ensure the save directory exists
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                    AssetDatabase.Refresh();
                }
                
                // Ensure the prefab name has the correct extension
                string prefabPath = $"{savePath}/{prefabName}.prefab";
                
                // Check if the prefab already exists
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (existingPrefab != null && !EditorUtility.DisplayDialog("Prefab Exists", 
                    $"A prefab named '{prefabName}' already exists at this location. Overwrite?", 
                    "Yes", "No"))
                {
                    return; // User chose not to overwrite
                }
                
                // Create the prefab
                GameObject prefab;
                if (createAsVariant)
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(selectedObject, prefabPath);
                }
                else
                {
                    // Create a clean copy to avoid modifying the original
                    GameObject tempObject = Instantiate(selectedObject);
                    tempObject.name = selectedObject.name;
                    
                    // Only try to unpack if this is a prefab instance
                    if (PrefabUtility.IsPartOfAnyPrefab(tempObject))
                    {
                        PrefabUtility.UnpackPrefabInstance(tempObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    }
                    
                    // Create the prefab
                    prefab = PrefabUtility.SaveAsPrefabAsset(tempObject, prefabPath);
                    
                    // Clean up the temporary object
                    DestroyImmediate(tempObject);
                }
                
                if (prefab != null)
                {
                    // Select the created prefab in the project view
                    EditorGUIUtility.PingObject(prefab);
                    Selection.activeObject = prefab;
                    
                    Debug.Log($"Prefab created successfully at: {prefabPath}");
                    
                    // Refresh the asset database to show the new prefab
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError("Failed to create prefab. Please check the console for errors.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating prefab: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create prefab: {e.Message}", "OK");
            }
        }
    }
}
