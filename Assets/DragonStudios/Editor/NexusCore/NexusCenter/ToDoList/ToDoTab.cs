using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace DraconisNexus
{
    public class ToDoTab : EditorWindow
    {
        private const string PrefsKey = "DraconisNexus.ToDoList.Tasks";

        [Serializable]
        private struct TaskData
        {
            public string Description;
            public string Notes;
            public long DueDateTicks;
        }


        [Serializable]
        private class Wrapper
        {
            public TaskData[] Pending;
            public TaskData[] Completed;
        }

        private List<TaskData> _pendingTasks = new List<TaskData>();
        private List<TaskData> _completedTasks = new List<TaskData>();
        private Vector2 _pendingScroll;
        private Vector2 _completedScroll;
        private Vector2 _scrollPosition;
        private string _newDescription = "";
        private string _newNotes = "";
        private string _newDueDate = DateTime.Today.ToString("dd-MM-yyyy");
        private int _viewIndex = 0;
        private int _editPendingIndex = -1;
        private readonly string[] _viewNames = { "Pending", "Completed" };
        private const int ComingUpDays = 3;
        
        public static void ShowWindow()
        {
            var window = GetWindow<ToDoTab>("To-Do List");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadTasks();
        }

        public void OnDisable()
        {
            SaveTasks();
        }

        public void OnGUI()
        {
            // Main container with padding
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            // Title with icon and padding
            EditorGUILayout.BeginHorizontal(EditorStyles.inspectorFullWidthMargins);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("To-Do List", NexusStyles.HeaderLabelStyle, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Divider with padding
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            rect.xMin = 0;
            rect.xMax = EditorGUIUtility.currentViewWidth;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(5);

            // Main content area with scroll view
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true));

            // Add new task section with proper spacing
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(NexusStyles.HelpBoxStyle);
            
            // Section header with padding
            EditorGUILayout.LabelField("Add New Task", NexusStyles.SectionHeaderStyle);
            EditorGUILayout.Space(5);

            // Description field with proper layout
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Description", GUILayout.Width(80));
            GUI.SetNextControlName("NewDescriptionField");
            _newDescription = EditorGUILayout.TextField("", _newDescription, NexusStyles.TextFieldStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Notes field with proper layout
            EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);
            _newNotes = EditorGUILayout.TextArea(_newNotes, NexusStyles.TextAreaStyle, 
                GUILayout.MinHeight(60), GUILayout.ExpandWidth(true));
            
            EditorGUILayout.Space(5);
            
            // Due date field with buttons and proper layout
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Due Date", GUILayout.Width(80));
            _newDueDate = EditorGUILayout.TextField("", _newDueDate, NexusStyles.TextFieldStyle, 
                GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("Today", NexusStyles.ButtonStyle, GUILayout.Width(60)))
            {
                _newDueDate = DateTime.Today.ToString("dd-MM-yyyy");
                GUI.FocusControl(null);
            }
            
            if (GUILayout.Button("Clear", NexusStyles.ButtonStyle, GUILayout.Width(60)))
            {
                _newDueDate = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            // Add button with proper spacing
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Task", NexusStyles.LargeButtonStyle, GUILayout.Width(200), 
                GUILayout.Height(36)) && !string.IsNullOrEmpty(_newDescription))
            {
                AddNewTask();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            // Handle Enter key for adding tasks
            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return && 
                    GUI.GetNameOfFocusedControl() == "NewDescriptionField")
                {
                    AddNewTask();
                    Event.current.Use();
                }
            }

            // Task list section
            EditorGUILayout.Space(15);
            
            // Tab selection with proper spacing
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _viewIndex = GUILayout.Toolbar(_viewIndex, _viewNames, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);

            // Draw the appropriate task list
            if (_viewIndex == 0)
                DrawPending();
            else
                DrawCompleted();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void AddNewTask()
        {
            var task = new TaskData
            {
                Description = _newDescription,
                Notes = _newNotes,
                DueDateTicks = 0
            };

            if (DateTime.TryParse(_newDueDate, out DateTime dueDate))
            {
                task.DueDateTicks = dueDate.Ticks;
            }

            _pendingTasks.Add(task);
            _newDescription = "";
            _newNotes = "";
            _newDueDate = DateTime.Today.ToString("dd-MM-yyyy");
            GUI.FocusControl(null);
            SaveTasks();
        }

        private void CompleteTask(int index)
        {
            if (index >= 0 && index < _pendingTasks.Count)
            {
                _completedTasks.Add(_pendingTasks[index]);
                _pendingTasks.RemoveAt(index);
                SaveTasks();
            }
        }

        private void DrawPending()
        {
            var today = DateTime.Today;
            _pendingScroll = EditorGUILayout.BeginScrollView(_pendingScroll);

            for (int i = 0; i < _pendingTasks.Count; i++)
            {
                var task = _pendingTasks[i];
                var due = new DateTime(task.DueDateTicks);
                var delta = (due - today).TotalDays;

                if (_editPendingIndex == i)
                {
                    // In-place edit
                    task.Description = EditorGUILayout.TextField("Task", task.Description);
                    task.Notes = EditorGUILayout.TextArea(task.Notes, GUILayout.Height(40));

                    var dueStr = due.ToString("dd-MM-yyyy");
                    var newDue = EditorGUILayout.TextField("Due", dueStr, GUILayout.Width(100));
                    if (DateTime.TryParseExact(
                            newDue,
                            "dd-MM-yyyy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out var pdEdit))
                    {
                        task.DueDateTicks = pdEdit.Ticks;
                    }

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_editicon.sml").image, "Edit"),
                        NexusStyles.ButtonStyle, GUILayout.Width(30)))
                    {
                        _editPendingIndex = i;
                        _newDescription = task.Description;
                        _newNotes = task.Notes;
                        _newDueDate = task.DueDateTicks != 0 ?
                            new DateTime(task.DueDateTicks).ToString("dd-MM-yyyy") : "";
                    }

                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_FilterSelectedOnly").image, "Complete"),
                        NexusStyles.ButtonStyle, GUILayout.Width(30)))
                    {
                        CompleteTask(i);
                    }

                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Delete"),
                        NexusStyles.ButtonStyle, GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Task",
                            "Are you sure you want to delete this task?", "Yes", "No"))
                        {
                            _pendingTasks.RemoveAt(i);
                            SaveTasks();
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    // Color-code
                    if (delta < 0) GUI.color = Color.red;
                    else if (delta <= ComingUpDays) GUI.color = Color.yellow;
                    else GUI.color = Color.green;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"• {task.Description}", GUILayout.ExpandWidth(true));
                    GUI.color = Color.white;

                    if (!string.IsNullOrEmpty(task.Notes) && GUILayout.Button("Notes", GUILayout.Width(50)))
                        EditorUtility.DisplayDialog("Notes", task.Notes, "OK");
                    else
                        GUILayout.Space(58); // Maintain layout when no notes

                    if (GUILayout.Button("Edit", GUILayout.Width(40)))
                        _editPendingIndex = i;

                    var doneClicked = GUILayout.Button("Done", GUILayout.Width(50));
                    GUILayout.EndHorizontal();

                    if (doneClicked)
                    {
                        _completedTasks.Add(task);
                        _pendingTasks.RemoveAt(i);
                        SaveTasks();
                        break;
                    }
                }

                GUI.color = Color.white;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCompleted()
        {
            _completedScroll = EditorGUILayout.BeginScrollView(_completedScroll);

            for (int i = 0; i < _completedTasks.Count; i++)
            {
                var task = _completedTasks[i];
                var due = new DateTime(task.DueDateTicks);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"✓ {task.Description}", EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Delete"),
                    NexusStyles.ButtonStyle, GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("Delete Completed Task",
                        "Are you sure you want to delete this completed task?", "Yes", "No"))
                    {
                        _completedTasks.RemoveAt(i);
                        SaveTasks();
                    }
                }

                if (GUILayout.Button("Restore", GUILayout.Width(60)))
                {
                    _pendingTasks.Add(task);
                    _completedTasks.RemoveAt(i);
                    SaveTasks();
                    break;
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }


        private void SaveTasks()
        {
            var wrapper = new Wrapper
            {
                Pending = _pendingTasks.ToArray(),
                Completed = _completedTasks.ToArray()
            };
            var json = JsonUtility.ToJson(wrapper, true);
            EditorPrefs.SetString(PrefsKey, json);
        }


        private void LoadTasks()
        {
            var json = EditorPrefs.GetString(PrefsKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<Wrapper>(json);
                    _pendingTasks = new List<TaskData>(wrapper.Pending);
                    _completedTasks = new List<TaskData>(wrapper.Completed);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load tasks: {e.Message}");
                    _pendingTasks = new List<TaskData>();
                    _completedTasks = new List<TaskData>();
                }
            }
        }
    }
}
