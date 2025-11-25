using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DraconisNexus;
using DraconisNexus;
using UnityEditor;


// Draconis Nexus - Main Hub Window
namespace DraconisNexus
{
    public class DraconisNexusWindow : EditorWindow
    {
        public enum Tab { Hub, NexusCenter, NexusCatalog, NexusCreator, Changelog }
        public Tab currentTab = Tab.Hub;
        private ToDoTab toDoTab;
        private NotepadWindow notepadWindow;

        public static DraconisNexusWindow ShowWindow()
        {
            var window = CreateInstance<DraconisNexusWindow>();
            window.titleContent = new GUIContent("Draconis Nexus");
            window.minSize = new Vector2(800, 600);
            window.maxSize = new Vector2(800, 600);
            
            // Center the window on screen
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            position.size = window.minSize;
            window.position = position;
            
            // Show as utility window (non-dockable, always on top)
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            // We'll create the ToDoTab when the window is opened
        }

        private void OnDisable()
        {
            // Clean up any open windows
        }

        private void OnGUI()
        {
            // Draw tabs and check for tab change
            var newTab = (Tab)GUILayout.Toolbar((int)currentTab, System.Enum.GetNames(typeof(Tab)));
            
            // Only trigger actions when tab actually changes
            if (newTab != currentTab)
            {
                currentTab = newTab;
                
                // Handle tab-specific actions
                // No auto-opening of windows on tab change
                // Windows will only open when explicitly clicked by the user
            }
            
            // Always draw the current tab's content
            switch (currentTab)
            {
                case Tab.Hub:
                    DrawHub();
                    break;
                case Tab.NexusCenter:
                    DrawNexusCenter();
                    break;
                    
                case Tab.NexusCatalog:
                    DrawNexusCatalog();
                    break;
                case Tab.NexusCreator:
                    EditorGUILayout.BeginVertical(NexusStyles.CenteredBoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    
                    // Title with icon
                    EditorGUILayout.LabelField("Nexus Creator Tools", NexusStyles.HeaderLabelStyle);
                    
                    // Divider
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    
                    // Description
                    EditorGUILayout.HelpBox("Access all creator tools from one central location.", MessageType.Info);
                    
                    // Spacer
                    GUILayout.Space(20);
                    
                    // Tools container
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    
                    // Prefab Creator Button
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("  Prefab Creator", EditorGUIUtility.IconContent("d_PrefabModel Icon").image), 
                        NexusStyles.LargeButtonStyle, GUILayout.Height(40), GUILayout.Width(220)))
                    {
                        PrefabCreatorWindow.ShowWindow();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    
                    // Folder Creator Button
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("  Folder Creator", EditorGUIUtility.IconContent("d_Folder Icon").image), 
                        NexusStyles.LargeButtonStyle, GUILayout.Height(40), GUILayout.Width(220)))
                    {
                        FolderCreatorEditorWindow.ShowWindow();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    
                    // DreamShaper Button
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("  DreamShaper", EditorGUIUtility.IconContent("d_SceneViewRGB").image), 
                        NexusStyles.LargeButtonStyle, GUILayout.Height(40), GUILayout.Width(220)))
                    {
                        DreamShaperWindow.ShowWindow();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    
                    // Zen Mode Button
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("  Zen Mode", EditorGUIUtility.IconContent("d_Animation.EventMarker").image), 
                        NexusStyles.LargeButtonStyle, GUILayout.Height(40), GUILayout.Width(220)))
                    {
                        ZenWindow.ShowWindow();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    
                    // Additional information
                    EditorGUILayout.Space(20);
                    EditorGUILayout.HelpBox("Tip: You can also access these tools from the main menu under 'Draconis Nexus'.", MessageType.None);
                    
                    // Footer
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Draconis Nexus - v1.0.2", EditorStyles.centeredGreyMiniLabel);
                    
                    var copyrightStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    { 
                        fontSize = 10,
                        normal = { textColor = new Color(0.35f, 0.6f, 0.85f) },
                        margin = new RectOffset(0, 0, 5, 2)
                    };
                    EditorGUILayout.LabelField("Copyright 2025 Dragon Studios ENT LLC. All rights reserved.", copyrightStyle);
                    
                    EditorGUILayout.EndVertical();
                    break;
                    
                case Tab.Changelog:
                    EditorGUILayout.BeginVertical(NexusStyles.CenteredBoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    
                    // Title with icon
                    EditorGUILayout.LabelField("Change Log", NexusStyles.HeaderLabelStyle);
                    
                    // Divider
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    
                    // Changelog content
                    _changelogScrollPos = EditorGUILayout.BeginScrollView(_changelogScrollPos);
                    
                    string changelogPath = "Assets/DragonStudios/Editor/NexusCore/NexusCenter/ChangeLog.MD";
                    if (File.Exists(changelogPath))
                    {
                        string changelog = File.ReadAllText(changelogPath);
                        EditorGUILayout.TextArea(changelog, EditorStyles.wordWrappedLabel);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Change log file not found at: " + changelogPath, MessageType.Warning);
                    }
                    
                    EditorGUILayout.EndScrollView();
                    
                    EditorGUILayout.EndVertical();
                    break;
            }
        }

        private enum NexusCenterTab { ToDo, Notepad }
        private NexusCenterTab _currentNexusCenterTab = NexusCenterTab.ToDo;
        private Vector2 _nexusCenterScrollPos;
        private Vector2 _changelogScrollPos;

        private void DrawNexusCenter()
        {
            // Main container with some padding
            _nexusCenterScrollPos = EditorGUILayout.BeginScrollView(_nexusCenterScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(NexusStyles.CenteredBoxStyle, GUILayout.ExpandWidth(true));
            
            // Title with icon
            EditorGUILayout.LabelField("Nexus Center", NexusStyles.HeaderLabelStyle);
            
            // Divider
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Description
            EditorGUILayout.HelpBox("Access your productivity tools from here.", MessageType.Info);
            
            // Spacer
            GUILayout.Space(20);
            
            // Buttons container
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            
            // To-Do List Button with icon
            if (GUILayout.Button(new GUIContent("  To-Do List", EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image), 
                NexusStyles.LargeButtonStyle))
            {
                OpenToDoListWindow();
            }
            
            GUILayout.Space(15);
            
            // Notepad Button with icon
            if (GUILayout.Button(new GUIContent("  Notepad", EditorGUIUtility.IconContent("d_TextAsset Icon").image), 
                NexusStyles.LargeButtonStyle))
            {
                OpenNotepadWindow();
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Footer
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Draconis Nexus - v1.0.2", EditorStyles.centeredGreyMiniLabel);
            
            var copyrightStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            { 
                fontSize = 10,
                normal = { textColor = new Color(0.35f, 0.6f, 0.85f) },
                margin = new RectOffset(0, 0, 5, 2)
            };
            EditorGUILayout.LabelField("Copyright 2025 Dragon Studios ENT LLC. All rights reserved.", copyrightStyle);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawProductivityTools()
        {
            // Description text
            EditorGUILayout.HelpBox("Access your productivity tools from here.", MessageType.Info);
            
            // Spacer
            GUILayout.Space(20);
            
            // Buttons container
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            
            // To-Do List Button with icon
            if (GUILayout.Button(new GUIContent("  To-Do List", EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image), 
                NexusStyles.LargeButtonStyle))
            {
                OpenToDoListWindow();
            }
            
            GUILayout.Space(15);
            
            // Notepad Button with icon
            if (GUILayout.Button(new GUIContent("  Notepad", EditorGUIUtility.IconContent("d_TextAsset Icon").image), 
                NexusStyles.LargeButtonStyle))
            {
                OpenNotepadWindow();
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawNexusCatalog()
        {
            // Main container with some padding
            EditorGUILayout.BeginVertical(NexusStyles.CenteredBoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            // Title with icon
            EditorGUILayout.LabelField("Nexus Catalog", NexusStyles.HeaderLabelStyle);
            
            // Divider
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Description
            EditorGUILayout.HelpBox("Access your asset management tools from here.", MessageType.Info);
            
            // Spacer
            GUILayout.Space(20);
            
            // Buttons container
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            
            // Prefab Search Button with icon
            if (GUILayout.Button(new GUIContent("  Prefab Search", EditorGUIUtility.IconContent("d_Search Icon").image), 
                NexusStyles.LargeButtonStyle))
            {
                NexusCatalogWindow.ShowWindow();
            }
            
            GUILayout.Space(15);
            
            // Asset Importer Button with icon
            if (GUILayout.Button(new GUIContent("  Asset Importer", EditorGUIUtility.IconContent("d_Import").image), 
                NexusStyles.LargeButtonStyle))
            {
                AssetImporterWindow.ShowWindow();
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Footer
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Draconis Nexus - v1.0.2", EditorStyles.centeredGreyMiniLabel);
            
            var copyrightStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            { 
                fontSize = 10,
                normal = { textColor = new Color(0.35f, 0.6f, 0.85f) },
                margin = new RectOffset(0, 0, 5, 2)
            };
            EditorGUILayout.LabelField("Copyright 2025 Dragon Studios ENT LLC. All rights reserved.", copyrightStyle);
            
            EditorGUILayout.EndVertical();
        }
        
        private void OpenToDoListWindow()
        {
            ToDoTab.ShowWindow();
        }
        
        private void OpenNotepadWindow()
        {
            NotepadWindow.ShowWindow();
        }

        private void DrawHub()
        {
            // Main container with consistent styling
            EditorGUILayout.BeginVertical(NexusStyles.CenteredBoxStyle, GUILayout.ExpandWidth(true));
            
            // Welcome header with minimal top spacing
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Welcome to Draconis Nexus", NexusStyles.HeaderLabelStyle);
            
            // Divider with reduced spacing
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // Description with compact spacing
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("A collection of essential Unity editor tools for game development.", MessageType.Info);
            
            // Quick Access section with minimal spacing
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Quick Access", NexusStyles.SectionHeaderStyle);
            
            // Buttons container with consistent styling
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            
            // Nexus Creator Tools button with icon
            if (GUILayout.Button(new GUIContent("  Nexus Creator Tools", EditorGUIUtility.IconContent("d_ToolHandleCenter").image), 
                NexusStyles.LargeButtonStyle, GUILayout.Height(36)))
            {
                currentTab = Tab.NexusCreator;
            }
            
            
            GUILayout.Space(10);
            
            // Documentation button with icon
            if (GUILayout.Button(new GUIContent("  Documentation", EditorGUIUtility.IconContent("d__Help").image), 
                NexusStyles.LargeButtonStyle, GUILayout.Height(36)))
            {
                DocumentationViewer.ShowWindow();
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Footer with copyright - minimal spacing
            GUILayout.FlexibleSpace();
            var copyrightStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            { 
                fontSize = 10,
                normal = { textColor = new Color(0.35f, 0.6f, 0.85f) },
                margin = new RectOffset(0, 0, 5, 2)
            };
            
            EditorGUILayout.LabelField("Copyright 2025 Dragon Studios ENT LLC. All rights reserved.", copyrightStyle);
            
            EditorGUILayout.EndVertical();
        }
    }

    [System.Serializable]
    public class MarkdownViewer : EditorWindow
    {
        protected string content = "";
        protected Vector2 scrollPosition;
        protected static readonly Vector2 WINDOW_SIZE = new Vector2(600, 800);
        protected string windowTitle = "";

        protected virtual void LoadContent(string filePath)
        {
            string fullPath = Path.Combine(Application.dataPath, filePath);
            if (File.Exists(fullPath))
            {
                content = File.ReadAllText(fullPath);
            }
            else
            {
                content = $"File not found at: {fullPath}";
                Debug.LogError($"File not found at: {fullPath}");
            }
        }

        protected virtual void OnGUI()
        {
            GUIStyle style = new GUIStyle(EditorStyles.textArea)
            {
                richText = true,
                wordWrap = true,
                fontSize = 14,
                padding = new RectOffset(10, 10, 10, 10)
            };

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.TextArea(content, style, GUILayout.ExpandWidth(true));
            }
        }
    }

    public class DocumentationViewer : MarkdownViewer
    {
        public static void ShowWindow()
        {
            var window = CreateInstance<DocumentationViewer>();
            window.titleContent = new GUIContent("Documentation");
            window.minSize = WINDOW_SIZE;
            window.maxSize = WINDOW_SIZE;
            window.LoadDocumentation();
            window.ShowUtility();
        }

        private void LoadDocumentation()
        {
            LoadContent("DragonStudios/Editor/NexusCore/Documentation.MD");
        }
    }

    public class ChangeLogViewer : MarkdownViewer
    {
        public static void ShowWindow()
        {
            var window = CreateInstance<ChangeLogViewer>();
            window.titleContent = new GUIContent("Change Log");
            window.minSize = WINDOW_SIZE;
            window.maxSize = WINDOW_SIZE;
            window.LoadChangeLog();
            window.ShowUtility();
        }

        private void LoadChangeLog()
        {
            LoadContent("DragonStudios/Editor/DSE/ChangeLog.MD");
        }
    }
}
