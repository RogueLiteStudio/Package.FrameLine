using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
namespace FrameLine
{
    public static class ViewStyles
    {
        public const float ScrollBarSize = 15;
        public const float FrameWidth = 25;//帧宽度
        public const float ClipCtrlWidth = 5;//帧片段左右控制滑块宽度
        public const float FrameBarHeight = 21;//顶部时间条高度
        public const float ClipHeight = 30;//帧片段高度
        public const float ClipColorHeight = 5;//帧片段底部颜色条高度
        public const float ClipVInterval = 3;//帧片段垂直间隔
        public const float TrackHeight = ClipHeight + ClipVInterval;//轨道条高度
        public const float ToolBarHeight = 20;

        public const float TrackHeadWidth = 150;
        public const float TrackFoldSize = 10;
        public static readonly Color BGGridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public static readonly Color BGGridDotLineColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        public static readonly Color SelectFrameBackGroundColor = new Color32(67, 205, 128, 40);
        public static readonly Color TrackBGColor = new Color32(80, 80, 80, 60);
        public static readonly Color ClipCtrlColor = new Color32(67, 205, 128, 100);
        public static readonly Color ClipSelectCtrlColor = new Color32(67, 205, 128, 255);
        public static readonly Color ClipColor = new Color32(150, 150, 150, 150);
        public static readonly Color EventColor = new Color32(200, 200, 200, 150);
        public static readonly Color InvalidClipColor = new Color32(100, 100, 100, 100);
        public static readonly Color SelectClipWireFrameColor = new Color32(0, 255, 255, 180);
        public static readonly GUIStyle FrameNumStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
        public static readonly GUIStyle ActionSkipStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight }.SetTextColor(Color.yellow);

        private static GUIStyle SetTextColor(this GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            return style;
        }

        private static Dictionary<System.Type, Color> s_typeColors = new Dictionary<System.Type, Color>();

        public static Color GetActionColor(System.Type actionType)
        {
            if (!s_typeColors.TryGetValue(actionType, out var color))
            {
                var att = actionType.GetCustomAttribute<FrameActionColorAttribute>();
                if (att != null)
                {
                    if (!ColorUtility.TryParseHtmlString(att.Color, out color))
                    {
                        color = ClipColor;
                    }
                }
                else
                {
                    color = ClipColor;
                }
                s_typeColors.Add(actionType, color);
            }
            return color;
        }
    }

}
