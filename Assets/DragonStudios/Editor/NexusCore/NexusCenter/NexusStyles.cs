using UnityEngine;
using UnityEditor;

namespace DraconisNexus
{
    public static class NexusStyles
    {
        private static GUIStyle _headerLabelStyle;
        private static GUIStyle _largeButtonStyle;
        private static GUIStyle _centeredBoxStyle;
        private static GUIStyle _sectionHeaderStyle;
        private static GUIStyle _textAreaStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _textFieldStyle;
        private static GUIStyle _helpBoxStyle;
        private static GUIStyle _boldFoldoutStyle;

        public static GUIStyle HeaderLabelStyle
        {
            get
            {
                if (_headerLabelStyle == null)
                {
                    _headerLabelStyle = new GUIStyle(EditorStyles.largeLabel)
                    {
                        fontSize = 18,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(10, 10, 15, 15),
                        fixedHeight = 45,
                        wordWrap = true,
                        clipping = TextClipping.Overflow
                    };
                }
                return _headerLabelStyle;
            }
        }

        public static GUIStyle LargeButtonStyle
        {
            get
            {
                if (_largeButtonStyle == null)
                {
                    _largeButtonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 36,
                        margin = new RectOffset(10, 10, 8, 8),
                        padding = new RectOffset(15, 15, 8, 8),
                        alignment = TextAnchor.MiddleCenter,
                        clipping = TextClipping.Overflow,
                        wordWrap = true
                    };
                    _largeButtonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
                    _largeButtonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.9f));
                    _largeButtonStyle.active.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.9f));
                }
                return _largeButtonStyle;
            }
        }

        public static GUIStyle CenteredBoxStyle
        {
            get
            {
                if (_centeredBoxStyle == null)
                {
                    _centeredBoxStyle = new GUIStyle(GUI.skin.box)
                    {
                        padding = new RectOffset(15, 15, 15, 15),
                        margin = new RectOffset(5, 5, 5, 5),
                        alignment = TextAnchor.MiddleCenter,
                        stretchWidth = true
                    };
                }
                return _centeredBoxStyle;
            }
        }

        public static GUIStyle SectionHeaderStyle
        {
            get
            {
                if (_sectionHeaderStyle == null)
                {
                    _sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        padding = new RectOffset(10, 10, 6, 6), // Increased horizontal padding
                        margin = new RectOffset(5, 5, 5, 5),
                        wordWrap = true, // Allow text to wrap to next line
                        clipping = TextClipping.Overflow // Allow text to overflow instead of being clipped
                    };
                }
                return _sectionHeaderStyle;
            }
        }

        public static GUIStyle TextAreaStyle
        {
            get
            {
                if (_textAreaStyle == null)
                {
                    _textAreaStyle = new GUIStyle(EditorStyles.textArea)
                    {
                        wordWrap = true,
                        padding = new RectOffset(8, 8, 8, 8),
                        margin = new RectOffset(5, 5, 5, 5),
                        clipping = TextClipping.Clip,
                        alignment = TextAnchor.UpperLeft,
                        stretchWidth = true,
                        stretchHeight = true,
                        fixedHeight = 0
                    };
                }
                return _textAreaStyle;
            }
        }

        public static GUIStyle ToggleStyle
        {
            get
            {
                if (_toggleStyle == null)
                {
                    _toggleStyle = new GUIStyle(EditorStyles.toggle)
                    {
                        margin = new RectOffset(5, 5, 5, 5),
                        padding = new RectOffset(20, 0, 2, 2)
                    };
                }
                return _toggleStyle;
            }
        }

        public static GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        margin = new RectOffset(2, 2, 2, 2),
                        padding = new RectOffset(8, 8, 4, 4)
                    };
                }
                return _buttonStyle;
            }
        }

        public static GUIStyle TextFieldStyle
        {
            get
            {
                if (_textFieldStyle == null)
                {
                    _textFieldStyle = new GUIStyle(EditorStyles.textField)
                    {
                        margin = new RectOffset(5, 5, 5, 5),
                        padding = new RectOffset(5, 5, 5, 5)
                    };
                }
                return _textFieldStyle;
            }
        }

        public static GUIStyle HelpBoxStyle
        {
            get
            {
                if (_helpBoxStyle == null)
                {
                    _helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(5, 5, 5, 5)
                    };
                }
                return _helpBoxStyle;
            }
        }

        public static GUIStyle BoldFoldoutStyle
        {
            get
            {
                if (_boldFoldoutStyle == null)
                {
                    _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                    {
                        fontStyle = FontStyle.Bold,
                        fontSize = 12,
                        padding = new RectOffset(20, 0, 2, 2)
                    };
                }
                return _boldFoldoutStyle;
            }
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
