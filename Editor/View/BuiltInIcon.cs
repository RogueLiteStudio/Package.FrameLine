using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    public class BuiltInIcon
    {
        public GUIContent Play;
        public GUIContent Pause;
        public GUIContent FirstKey;
        public GUIContent LastKey;
        public GUIContent PrevKey;
        public GUIContent NextKey;
        public GUIContent Add;
        private BuiltInIcon()
        {
            Play = EditorGUIUtility.IconContent("Animation.Play");
            Pause = EditorGUIUtility.IconContent("PauseButton");
            FirstKey = EditorGUIUtility.IconContent("Animation.FirstKey");
            LastKey = EditorGUIUtility.IconContent("Animation.LastKey");
            PrevKey = EditorGUIUtility.IconContent("Animation.PrevKey");
            NextKey = EditorGUIUtility.IconContent("Animation.NextKey");
            Add = EditorGUIUtility.IconContent("d_CreateAddNew");
        }

        private static BuiltInIcon _instance;
        public static BuiltInIcon Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuiltInIcon();
                }
                return _instance;
            }
        }
    }
}
