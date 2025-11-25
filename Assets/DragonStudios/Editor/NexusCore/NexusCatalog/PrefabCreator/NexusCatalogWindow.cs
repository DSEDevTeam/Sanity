using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;
using DraconisNexus;

namespace DraconisNexus
{
    public class NexusCatalogWindow : EditorWindow
    {
        // Prefab Catalog fields
        private List<string> allPrefabPaths = new List<string>();
        private List<string> filteredPrefabPaths = new List<string>();
        private string searchQuery = "";
        private Vector2 scrollPosition;
        private string selectedAssetPath = "";
        private Editor previewEditor;
        private Vector2 previewScroll;
        private bool showPreview = true;
        private float previewSize = 128f;
        private GUIStyle previewStyle;
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private bool needsRefresh = false;
        private double lastRefreshTime = 0;
        private const double REFRESH_COOLDOWN = 0.5; // Half second cooldown between refreshes
        
        public static void ShowWindow()
        {
            var window = GetWindow<NexusCatalogWindow>("Prefab Search");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        private void OnEnable()
        {
            try
            {
                // Initialize styles only if they're available
                if (EditorStyles.helpBox != null)
                {
                    previewStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(5, 5, 5, 5)
                    };
                }
                
                if (EditorStyles.largeLabel != null)
                {
                    headerStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(0, 0, 5, 5)
                    };
                }
                
                if (EditorStyles.miniButton != null)
                {
                    buttonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        fixedWidth = 80,
                        margin = new RectOffset(2, 2, 2, 2)
                    };
                }
                
                RefreshPrefabs();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing NexusCatalogWindow: {e.Message}");
            }
        }
        
        private void OnDisable()
        {
            try
            {
                // Clean up the preview editor when the window is closed
                if (previewEditor != null && previewEditor.target != null)
                {
                    DestroyImmediate(previewEditor);
                    previewEditor = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error cleaning up NexusCatalogWindow: {e.Message}");
                previewEditor = null; // Ensure we don't leave a broken reference
            }
        }
        
        private void RefreshPrefabs(bool force = false)
        {
            double currentTime = EditorApplication.timeSinceStartup;
            
            // Don't refresh too often
            if (!force && currentTime - lastRefreshTime < REFRESH_COOLDOWN)
            {
                needsRefresh = true;
                return;
            }
            
            // Clear existing data
            allPrefabPaths.Clear();
            
            // Find all prefabs in the project, excluding those in PackageCache and Packages
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            allPrefabPaths = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => !path.Contains("PackageCache", StringComparison.OrdinalIgnoreCase) && 
                              !path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase) &&
                              path.StartsWith("Assets/"))
                .ToList();
            
            // Apply search filter if there's a query
            UpdateFilteredResults();
            
            lastRefreshTime = currentTime;
            needsRefresh = false;
        }
        
        private void UpdateFilteredResults()
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                filteredPrefabPaths = new List<string>(allPrefabPaths);
            }
            else
            {
                var query = searchQuery.ToLower();
                filteredPrefabPaths = allPrefabPaths
                    .Where(path => Path.GetFileNameWithoutExtension(path).ToLower().Contains(query))
                    .ToList();
            }
            
            // Sort alphabetically
            filteredPrefabPaths.Sort((a, b) => 
                string.Compare(Path.GetFileNameWithoutExtension(a), 
                             Path.GetFileNameWithoutExtension(b), 
                             StringComparison.OrdinalIgnoreCase));
        }
        
        private void OnGUI()
        {
            // Handle delayed refresh if needed
            if (needsRefresh && EditorApplication.timeSinceStartup - lastRefreshTime >= REFRESH_COOLDOWN)
            {
                RefreshPrefabs();
            }
            
            // Draw the prefab catalog
            DrawPrefabCatalog();
        }
        
        void DrawPrefabCatalog()
        {
            // Begin the main vertical layout
            EditorGUILayout.BeginVertical();
            
            // Toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Refresh button
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshPrefabs(true);
            }
            
            // Add some space between buttons
            GUILayout.Space(5);
            
            // Import Assets button
            if (GUILayout.Button("Import Assets", EditorStyles.toolbarButton, GUILayout.Width(90)))
            {
                AssetImporterWindow.ShowWindow();
            }
            
            // Search field
            EditorGUI.BeginChangeCheck();
            string newSearchQuery = EditorGUILayout.TextField("", searchQuery, "SearchTextField");
            if (EditorGUI.EndChangeCheck() || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                searchQuery = newSearchQuery;
                UpdateFilteredResults();
            }
            
            // Clear search button
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    searchQuery = "";
                    UpdateFilteredResults();
                    GUI.FocusControl(null);
                }
            }
            
            GUILayout.EndHorizontal();
            
            // Split view for prefab list and preview
            EditorGUILayout.BeginHorizontal();
            
            // Prefab list (left panel)
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f));
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            
            // Display prefab list
            foreach (string path in filteredPrefabPaths)
            {
                string prefabName = Path.GetFileNameWithoutExtension(path);
                bool isSelected = path == selectedAssetPath;
                
                GUIStyle style = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;
                
                if (GUILayout.Button(prefabName, style, GUILayout.Height(20)))
                {
                    selectedAssetPath = path;
                    // Create a preview for the selected prefab
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        if (previewEditor != null) DestroyImmediate(previewEditor);
                        previewEditor = Editor.CreateEditor(prefab);
                    }
                }
                
                // Add a subtle separator
                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            // Preview panel (right panel)
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            if (!string.IsNullOrEmpty(selectedAssetPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(selectedAssetPath);
                if (prefab != null)
                {
                    // Header with prefab name and buttons
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GUILayout.Label(prefab.name, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    
                    // Toggle preview
                    showPreview = GUILayout.Toggle(showPreview, "Preview", EditorStyles.toolbarButton, GUILayout.Width(70));
                    
                    // Open button
                    if (GUILayout.Button("Open", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    {
                        Selection.activeObject = prefab;
                        EditorGUIUtility.PingObject(prefab);
                    }
                    
                    // Instantiate button
                    if (GUILayout.Button("Instantiate", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    {
                        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        Selection.activeGameObject = instance;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                    
                    GUILayout.EndHorizontal();
                    
                    // Preview area
                    if (showPreview && previewEditor != null)
                    {
                        previewScroll = EditorGUILayout.BeginScrollView(previewScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        
                        // Use the preview size from the slider, but ensure it's within reasonable bounds
                        float previewWidth = Mathf.Clamp(previewSize, 64f, position.width * 0.9f);
                        float previewHeight = Mathf.Clamp(previewSize, 64f, position.height * 0.6f);
                        
                        // Calculate the preview rect
                        Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
                        
                        // Center the preview
                        float xPadding = (position.width * 0.5f - previewWidth) * 0.5f;
                        previewRect.x += xPadding;
                        
                        // Draw the preview with the specified size
                        previewEditor.OnInteractivePreviewGUI(previewRect, EditorStyles.helpBox);
                        
                        // Add a slider to adjust preview size
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Preview Size", GUILayout.Width(80));
                        previewSize = GUILayout.HorizontalSlider(previewSize, 64f, 512f);
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUILayout.EndScrollView();
                    }
                    
                    // Prefab info
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Path:", selectedAssetPath, EditorStyles.wordWrappedLabel);
                    
                    // Components list
                    var components = prefab.GetComponents<Component>();
                    if (components.Length > 1)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Components:", EditorStyles.boldLabel);
                        foreach (var component in components)
                        {
                            if (component == null) continue;
                            EditorGUILayout.LabelField(component.GetType().Name);
                        }
                    }
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("Select a prefab to preview", MessageType.Info);
                GUILayout.FlexibleSpace();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            // Status bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"{filteredPrefabPaths.Count} prefabs");
            if (!string.IsNullOrEmpty(searchQuery))
            {
                GUILayout.Label("|");
                GUILayout.Label($"Filtered by: '{searchQuery}'", EditorStyles.miniLabel);
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("Dragon Studios ENT LLC", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            // End of main vertical layout
            EditorGUILayout.EndVertical();
        }
    }
}
