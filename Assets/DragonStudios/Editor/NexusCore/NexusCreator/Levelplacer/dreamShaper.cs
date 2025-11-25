using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace DraconisNexus
{
    public class DreamShaperWindow : EditorWindow
    {
        //────────────────────────────────────────────────────────────────────────────
        // Configuration & State
        //────────────────────────────────────────────────────────────────────────────

        [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
        private int activeIndex = 0;
        private bool alignToNormal = true;
        private float spawnHeightOffset = 0f;
        private Vector2 scrollPos;

        private GameObject lastSpawnedInstance = null;

        // Add “Spawn” as a tool mode
        private enum ToolMode
        {
            Spawn,
            Move,
            Rotate,
            Scale,
            None
        }

        private ToolMode currentMode = ToolMode.Spawn;

        //────────────────────────────────────────────────────────────────────────────
        // Initialization & Cleanup
        //────────────────────────────────────────────────────────────────────────────
        
        public static void ShowWindow()
        {
            var window = GetWindow<DreamShaperWindow>("Dream Shaper");
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        //────────────────────────────────────────────────────────────────────────────
        // Window UI
        //────────────────────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DreamShaper", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Prefab list
            EditorGUILayout.LabelField("Spawnable Prefabs:", EditorStyles.miniBoldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(120));
            int removeAt = -1;
            for (int i = 0; i < prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
                if (GUILayout.Button("X", GUILayout.Width(20))) removeAt = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeAt >= 0) prefabs.RemoveAt(removeAt);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Prefab Slot"))
                prefabs.Add(null);

            EditorGUILayout.Space();

            // Active prefab selector
            if (prefabs.Count > 0)
            {
                activeIndex = Mathf.Clamp(activeIndex, 0, prefabs.Count - 1);
                activeIndex = EditorGUILayout.Popup("Active Prefab", activeIndex, GetPrefabNames());
            }
            else
            {
                EditorGUILayout.HelpBox("No prefabs to spawn. Add slots above.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Spawn settings
            alignToNormal = EditorGUILayout.Toggle("Align To Surface Normal", alignToNormal);
            spawnHeightOffset = EditorGUILayout.FloatField("Height Offset", spawnHeightOffset);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Select the mode below:\n" +
                "• Spawn → Left-click in Scene to place objects.\n" +
                "• Move/Rotate/Scale → Drag the yellow handles.",
                MessageType.None);

            EditorGUILayout.Space();

            // Mode toolbar: Spawn, Move, Rotate, Scale, None
            currentMode = (ToolMode)GUILayout.Toolbar(
                (int)currentMode,
                new string[] { "Spawn", "Move", "Rotate", "Scale", "None" },
                GUILayout.Height(30)
            );
        }

        private string[] GetPrefabNames()
        {
            var names = new string[prefabs.Count];
            for (int i = 0; i < prefabs.Count; i++)
                names[i] = prefabs[i] != null ? prefabs[i].name : "<Unassigned>";
            return names;
        }

        //────────────────────────────────────────────────────────────────────────────
        // Scene GUI: Spawn & Transform Handles
        //────────────────────────────────────────────────────────────────────────────

        private void OnSceneGUI(SceneView sceneView)
        {
            var e = Event.current;

            // ── 1) SPAWN MODE ────────────────────────────────────────────────────────
            if (currentMode == ToolMode.Spawn
                && e.type == EventType.MouseDown
                && e.button == 0
                && !e.alt)
            {
                if (prefabs.Count > 0 && prefabs[activeIndex] != null)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        lastSpawnedInstance = SpawnAt(hit.point, hit.normal);
                        e.Use();
                    }
                }
            }

            // ── 2) MANIPULATE MODE ───────────────────────────────────────────────────
            if (lastSpawnedInstance != null
                && (currentMode == ToolMode.Move
                    || currentMode == ToolMode.Rotate
                    || currentMode == ToolMode.Scale))
            {
                // a) Disable Unity's built‐in tool so our handles get the events
                Tools.current = Tool.None;
                // b) Capture mouse for our custom handles
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                HandleUtility.AddDefaultControl(controlID);

                Transform t = lastSpawnedInstance.transform;
                Handles.color = Color.yellow;

                EditorGUI.BeginChangeCheck();

                Vector3 newPos = t.position;
                Quaternion newRot = t.rotation;
                Vector3 newScale = t.localScale;

                switch (currentMode)
                {
                    case ToolMode.Move:
                        newPos = Handles.PositionHandle(t.position, t.rotation);
                        break;
                    case ToolMode.Rotate:
                        newRot = Handles.RotationHandle(t.rotation, t.position);
                        break;
                    case ToolMode.Scale:
                        newScale = Handles.ScaleHandle(
                            t.localScale,
                            t.position,
                            t.rotation,
                            HandleUtility.GetHandleSize(t.position)
                        );
                        break;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, currentMode + " Handle");
                    t.position = newPos;
                    t.rotation = newRot;
                    t.localScale = newScale;
                    EditorUtility.SetDirty(t);

                    // c) Force repaint so you see updates instantly
                    SceneView.RepaintAll();
                }
            }
            else
            {
                // If you’re not in Move/Rotate/Scale, restore the default tool
                Tools.current = Tool.View;
            }
        }

        //────────────────────────────────────────────────────────────────────────────
        // Helper: Instantiate a prefab with undo & mark the scene dirty
        //────────────────────────────────────────────────────────────────────────────

        private GameObject SpawnAt(Vector3 point, Vector3 normal)
        {
            GameObject prefab = prefabs[activeIndex];
            Quaternion rot = alignToNormal
                ? Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, normal), normal)
                : Quaternion.identity;
            Vector3 pos = point + normal * spawnHeightOffset;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(
                prefab, EditorSceneManager.GetActiveScene()) as GameObject;

            Undo.RegisterCreatedObjectUndo(instance, "Spawn Prefab");
            instance.transform.position = pos;
            instance.transform.rotation = rot;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return instance;
        }
    }
}


