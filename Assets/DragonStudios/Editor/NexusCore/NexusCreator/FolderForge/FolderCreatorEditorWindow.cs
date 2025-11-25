using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DraconisNexus
{
    /// <summary>
    /// Editor window for creating and managing folder structures
    /// </summary>
    public class FolderCreatorEditorWindow : EditorWindow
    {
        private enum DropPosition { None, Above, Below, AsChild }
        private const string PresetFolderPath = "Assets/DragonStudios/Editor/FolderPresets";
        private const string WindowTitle = "Folder Creator";
        
        /// <summary>
        /// Shows the Folder Creator window
        /// </summary>
        public static void ShowWindow()
        {
            var window = GetWindow<FolderCreatorEditorWindow>("Folder Creator");
            window.titleContent = new GUIContent(WindowTitle, EditorGUIUtility.IconContent("Folder Icon").image);
            window.minSize = new Vector2(300, 400);
            window.Show();
        }
        
        [System.Serializable]
        private class FolderNode
        {
            public string name = "New Folder";
            public List<FolderNode> children = new();
            public bool isExpanded = true;
        }
        
        private List<FolderNode> _rootNodes = new();
        private Vector2 _scrollPos;

        private FolderNode _draggedNode;
        private FolderNode _hoveredNode;
        private DropPosition _dropPosition;
        private Rect _hoveredRect;

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (var i = 0; i < _rootNodes.Count; i++)
            {
                DrawFolderNode(_rootNodes[i], 0, () => _rootNodes.RemoveAt(i), _rootNodes, i);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Root Folder"))
                _rootNodes.Add(new FolderNode());
            
            if (GUILayout.Button("Save As Preset"))
                SaveAsPreset();
            
            if (GUILayout.Button("Load Preset"))
                LoadPreset();

            if (GUILayout.Button("Create Folder Structure"))
            {
                foreach (var node in _rootNodes)
                    CreateFoldersRecursive(node, "Assets");
                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndHorizontal();

            HandleDragAndDrop();
        }

        private void DrawFolderNode(FolderNode node, int indent, System.Action onRemove, List<FolderNode> siblings, int index)
        {
            var rect = EditorGUILayout.BeginHorizontal();

            GUILayout.Space(indent * 16);
            node.isExpanded = EditorGUILayout.Foldout(node.isExpanded, GUIContent.none, true);
            node.name = EditorGUILayout.TextField(node.name);

            if (GUILayout.Button("+", GUILayout.Width(22)))
                node.children.Add(new FolderNode());

            if (GUILayout.Button("-", GUILayout.Width(22)))
            {
                onRemove?.Invoke();
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.EndHorizontal();
            
            if (rect.Contains(Event.current.mousePosition) && _draggedNode != null && _draggedNode != node)
            {
                _hoveredNode = node;
                _hoveredRect = rect;

                var mouseY = Event.current.mousePosition.y;
                if (mouseY < rect.yMin + 6)
                    _dropPosition = DropPosition.Above;
                else if (mouseY > rect.yMax - 6)
                    _dropPosition = DropPosition.Below;
                else
                    _dropPosition = DropPosition.AsChild;

                Repaint();
            }
            
            if (_hoveredNode == node && _draggedNode != null)
            {
                var color = new Color(0.2f, 0.7f, 1f, 1f);
                switch (_dropPosition)
                {
                    case DropPosition.Above:
                        EditorGUI.DrawRect(new Rect(_hoveredRect.x, _hoveredRect.yMin, _hoveredRect.width, 2), color);
                        break;
                    case DropPosition.Below:
                        EditorGUI.DrawRect(new Rect(_hoveredRect.x, _hoveredRect.yMax, _hoveredRect.width, 2), color);
                        break;
                    case DropPosition.AsChild:
                        EditorGUI.DrawRect(new Rect(_hoveredRect.x + 12, _hoveredRect.center.y, _hoveredRect.width - 12, 2), color);
                        break;
                }
            }
            
            if (node.isExpanded)
            {
                for (var i = 0; i < node.children.Count; i++)
                {
                    var captured = i;
                    DrawFolderNode(node.children[i], indent + 1, () => node.children.RemoveAt(captured), node.children, i);
                }
            }
            
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _draggedNode = node;
                Event.current.Use();
            }
            
            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition) && _draggedNode != null)
            {
                PerformDrop(siblings, index);
                Event.current.Use();
            }
        }
        
        private void HandleDragAndDrop()
        {
            if (Event.current.type != EventType.MouseUp) return;
            _draggedNode = null;
            _hoveredNode = null;
            _dropPosition = DropPosition.None;
        }
        
        private void PerformDrop(List<FolderNode> siblings, int index)
        {
            if (_draggedNode == null || _hoveredNode == null)
                return;

            if (!RemoveNode(_rootNodes, _draggedNode))
                return;

            switch (_dropPosition)
            {
                case DropPosition.Above:
                    siblings.Insert(index, _draggedNode);
                    break;
                case DropPosition.Below:
                    siblings.Insert(index + 1, _draggedNode);
                    break;
                case DropPosition.AsChild:
                    _hoveredNode.children.Add(_draggedNode);
                    break;
            }

            _draggedNode = null;
            _hoveredNode = null;
            _dropPosition = DropPosition.None;
            Repaint();
        }
        
        private static bool RemoveNode(List<FolderNode> nodes, FolderNode target)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == target)
                {
                    nodes.RemoveAt(i);
                    return true;
                }

                if (RemoveNode(nodes[i].children, target))
                    return true;
            }
            return false;
        }

        private static void CreateFoldersRecursive(FolderNode node, string parentPath)
        {
            var newPath = Path.Combine(parentPath, node.name);
            if (!AssetDatabase.IsValidFolder(newPath))
            {
                AssetDatabase.CreateFolder(parentPath, node.name);
            }

            foreach (var child in node.children)
            {
                CreateFoldersRecursive(child, newPath);
            }
        }

        private static void EnsurePresetFolder()
        {
            if (Directory.Exists(PresetFolderPath)) return;
            Directory.CreateDirectory(PresetFolderPath);
            AssetDatabase.Refresh();
        }

        private void SaveAsPreset()
        {
            EnsurePresetFolder();
            
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Folder Structure Preset",
                "NewFolderPreset",
                "json",
                "Enter a name for this preset",
                PresetFolderPath);
            
            if (string.IsNullOrEmpty(path))
                return;
            
            var json = JsonUtility.ToJson(new FolderNodeWrapper { nodes = _rootNodes }, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            //Debug.Log("Preset saved to " + path);
        }

        private void LoadPreset()
        {
            var path = EditorUtility.OpenFilePanel(
                "Load Folder Structure Preset",
                PresetFolderPath,
                "json");
            
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            
            var json = File.ReadAllText(path);
            _rootNodes = JsonUtility.FromJson<FolderNodeWrapper>(json).nodes ??  new List<FolderNode>();
            Repaint();
            //Debug.Log("Preset loaded: " + path);
        }

        [System.Serializable]
        private class FolderNodeWrapper
        {
            public List<FolderNode> nodes = new();
        }
        

    }
}