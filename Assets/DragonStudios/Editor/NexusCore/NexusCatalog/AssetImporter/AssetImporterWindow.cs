using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DraconisNexus
{
    public class AssetImporterWindow : EditorWindow
    {
        private string sourcePath = "";
        private string targetPath = "Assets/";
        private bool includeSubdirectories = true;
        private string[] supportedExtensions = new string[] { 
            ".fbx", ".png", ".jpg", ".jpeg", ".tga", ".mat", 
            ".prefab", ".unity", ".cs", ".shader", ".anim", ".controller" 
        };
        private string importStatus = "";
        private bool isImporting = false;
        private float importProgress = 0f;
        private Vector2 scrollPosition;
        public static void ShowWindow()
        {
            var window = GetWindow<AssetImporterWindow>("Asset Importer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5);
            
            // Title
            EditorGUILayout.LabelField("Asset Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Scroll view for the content
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Source Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source Folder:", GUILayout.Width(100));
            EditorGUILayout.TextField(sourcePath);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Source Folder", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    sourcePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Target Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Path:", GUILayout.Width(100));
            targetPath = EditorGUILayout.TextField(targetPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                string path = EditorUtility.SaveFolderPanel("Select Target Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    targetPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else if (!string.IsNullOrEmpty(path))
                {
                    importStatus = "Error: Target must be within the Assets folder";
                }
            }
            EditorGUILayout.EndHorizontal();

            includeSubdirectories = EditorGUILayout.Toggle("Include Subdirectories", includeSubdirectories);

            EditorGUILayout.Space(10);

            // Import Button
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(sourcePath) || isImporting);
            if (GUILayout.Button("Import Assets", GUILayout.Height(30)))
            {
                ImportAssets();
            }
            EditorGUI.EndDisabledGroup();

            // Progress bar
            if (isImporting)
            {
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, importProgress, "Importing... " + (importProgress * 100).ToString("F0") + "%");
                Repaint();
            }

            // Status message
            if (!string.IsNullOrEmpty(importStatus))
            {
                EditorGUILayout.HelpBox(importStatus, MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private async void ImportAssets()
        {
            if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
            {
                importStatus = "Error: Source directory does not exist";
                return;
            }

            isImporting = true;
            importStatus = "Preparing to import assets...";
            importProgress = 0f;
            Repaint();

            try
            {
                // Get all files matching the supported extensions
                var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(sourcePath, "*.*", searchOption)
                    .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToArray();

                if (files.Length == 0)
                {
                    importStatus = "No supported files found in the source directory";
                    return;
                }

                // Create target directory if it doesn't exist
                if (!AssetDatabase.IsValidFolder(targetPath))
                {
                    Directory.CreateDirectory(Application.dataPath + targetPath.Substring("Assets".Length));
                    AssetDatabase.Refresh();
                }

                // Import each file
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    string relativePath = file.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar);
                    string targetFile = Path.Combine(targetPath, relativePath);
                    string targetDir = Path.GetDirectoryName(targetFile);

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    // Copy file
                    File.Copy(file, targetFile, true);

                    // Update progress
                    importProgress = (i + 1) / (float)files.Length;
                    importStatus = $"Importing {i + 1} of {files.Length}: {Path.GetFileName(file)}";
                    Repaint();

                    // Small delay to keep the UI responsive
                    await Task.Delay(10);
                }


                AssetDatabase.Refresh();
                importStatus = $"Successfully imported {files.Length} assets to {targetPath}";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during asset import: {e.Message}");
                importStatus = $"Error: {e.Message}";
            }
            finally
            {
                isImporting = false;
                importProgress = 0f;
                Repaint();
            }
        }
    }
}
