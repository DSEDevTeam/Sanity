using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DraconisNexus
{
    public class ZenWindow : EditorWindow
    {
        private static readonly List<string> funnyMessages = new List<string>
        {
            "Keep clicking... I believe in you!",
            "The button is getting shinier...",
            "Have you tried turning it off and on again?",
            "This is not the button you're looking for...",
            "Click me like you mean it!",
            "You're doing great! Keep going!",
            "Is this button getting smaller or are you clicking faster?",
            "The answer to life, the universe, and everything is... keep clicking!"
        };

        private const int CLICKS_FOR_SPECIAL = 10;
        private int clickCount = 0;
        private string currentMessage = "Click for Zen";
        private bool specialTriggered = false;
        public static void ShowWindow()
        {
            var window = GetWindow<ZenWindow>("Zen Mode");
            window.minSize = new Vector2(300, 150);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Zen Mode", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Display the current message
            EditorGUILayout.LabelField(currentMessage, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(20);

            // The magic button
            if (GUILayout.Button("Find Your Zen", GUILayout.Height(40)))
            {
                OnZenButtonClicked();
            }

            // Reset button
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Reset"))
            {
                clickCount = 0;
                specialTriggered = false;
                currentMessage = "Click for Zen";
            }
        }

        private void OnZenButtonClicked()
        {
            clickCount++;

            if (clickCount >= CLICKS_FOR_SPECIAL && !specialTriggered)
            {
                SpecialAction();
                specialTriggered = true;
            }
            else if (!specialTriggered)
            {
                // Show a random funny message
                currentMessage = funnyMessages[Random.Range(0, funnyMessages.Count)];
                
                // Play a sound for feedback
                EditorApplication.Beep();
            }
        }

        private void SpecialAction()
        {
            currentMessage = "CONGRATULATIONS!\n\n NOW GO TO BED\n\n(GO TO BED)";
            
            // Display a dialog
            if (EditorUtility.DisplayDialog("Zen Achieved!", 
                "You've reached the ultimate state of Zen!\n\nWould you like to celebrate with a random color?", 
                "Yes!", "No thanks"))
            {
                // Change the editor's background tint as a fun effect
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.Link);
                EditorGUI.DrawRect(new Rect(0, 0, 10000, 10000), new Color(color.r, color.g, color.b, 0.1f));
            }
        }
    }
}
