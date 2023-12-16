using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    public class BuiltInIcon
    {
        public GUIContent Play;
        public GUIContent Pause;
        public GUIContent PrevKey;
        public GUIContent NextKey;
        private BuiltInIcon()
        {
            Play = EditorGUIUtility.IconContent("Animation.Play");
            Pause = EditorGUIUtility.IconContent("PauseButton");
            PrevKey = EditorGUIUtility.IconContent("Animation.PrevKey");
            NextKey = EditorGUIUtility.IconContent("Animation.NextKey");
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
