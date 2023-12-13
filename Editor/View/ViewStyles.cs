using UnityEditor;
using UnityEngine;
namespace FrameLine
{
    public static class ViewStyles
    {
        public const float ScrollBarSize = 15;
        public const float FrameWidth = 35;//帧宽度
        public const float ClipCtrlWidth = 6;//帧片段左右控制滑块宽度
        public const float FrameBarHeight = 21;//顶部时间条高度
        public const float ClipHeight = 30;//帧片段高度
        public const float ClipVInterval = 3;//帧片段垂直间隔
        public const float TrackHeight = ClipHeight + ClipVInterval;//轨道条高度
        public const float ToolBarHeight = 20;

        public const float TrackHeadWidth = 150;
        public const float TrackFoldSize = 10;

        public static readonly Color SelectFrameBackGroundColor = new Color32(67, 205, 128, 80);
        public static readonly Color TrackBGColor = new Color32(80, 80, 80, 60);
        public static readonly Color ClipCtrlColor = new Color32(67, 205, 128, 100);
        public static readonly Color ClipSelectCtrlColor = new Color32(67, 205, 128, 255);
        public static readonly Color ClipColor = new Color32(100, 100, 100, 150);
        public static readonly Color InvalidClipColor = new Color32(139, 0, 0, 180);
        public static readonly Color SelectClipWireFrameColor = new Color32(0, 255, 255, 180);
        public static readonly GUIStyle FrameNumStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
    }

}
