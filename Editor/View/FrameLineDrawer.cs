using UnityEditor;
using UnityEngine;
namespace FrameLine
{
    public static class FrameLineDrawer
    {
        public static void DrawFrameLineBackGround(FrameLineEditorView gui, Rect showRect, int showMaxFrame)
        {
            using (new Handles.DrawingScope(new Color(0.5f, 0.5f, 0.5f, 0.5f)))
            {
                int startIndex = Mathf.Clamp(gui.VisableFrameStart, 0, gui.Group.FrameCount);
                int endIndex = Mathf.Clamp(gui.VisableFrameEnd, 0, showMaxFrame);
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    float xPos = i * ViewStyles.FrameWidth;
                    if (i <= gui.Group.FrameCount)
                        Handles.DrawLine(new Vector2(xPos, showRect.yMin), new Vector2(xPos, showRect.yMax));
                    if (i < endIndex)
                        GUI.Label(new Rect(xPos, 0, ViewStyles.FrameWidth, ViewStyles.FrameBarHeight), i.ToString(), ViewStyles.FrameNumStyle);
                }
            }
            if (gui.CurrentFrame >= gui.VisableFrameStart && gui.CurrentFrame <= gui.VisableFrameEnd)
            {
                Rect rect = new Rect(gui.CurrentFrame * ViewStyles.FrameWidth, 0, ViewStyles.FrameWidth, showRect.height);
                GUIRenderHelper.DrawRect(rect, ViewStyles.SelectFrameBackGroundColor, 5, BorderType.Top);
            }
        }

        public static void DrawTrackHead(FrameLineEditorView editorView, Vector2 size)
        {
            int trackIndex = 0;
            foreach (var track in editorView.Tracks)
            {
                if (track.Count == 0)
                    continue;
                if (trackIndex > editorView.VisableTrackEnd)
                    return;
                int trackVisableCount = (track.Foldout ? track.Count : 1);
                do
                {
                    if (trackIndex + trackVisableCount <= editorView.VisableTrackStart)
                        break;
                    int startIndex = Mathf.Clamp(trackIndex, editorView.VisableFrameStart, editorView.VisableTrackEnd) - trackIndex;
                    int endIndex = Mathf.Clamp(trackIndex + trackVisableCount, editorView.VisableFrameStart, editorView.VisableTrackEnd) - trackIndex;
                    DrawTrackHead(editorView, track, trackIndex, size.x);
                } while (false);
                trackIndex += trackVisableCount;
            }
        }

        public static void DrawFrameActions(FrameLineEditorView editorView)
        {
            int trackIndex = 0;
            foreach (var track in editorView.Tracks)
            {
                if (track.Count == 0)
                    continue;
                if (trackIndex > editorView.VisableTrackEnd)
                    return;
                int trackVisableCount = (track.Foldout ? track.Count : 1);
                do
                {
                    if (trackIndex + trackVisableCount <= editorView.VisableTrackStart)
                        break;
                    int startIndex = Mathf.Clamp(trackIndex, editorView.VisableTrackStart, editorView.VisableTrackEnd) - trackIndex;
                    int endIndex = Mathf.Clamp(trackIndex + trackVisableCount - 1, editorView.VisableFrameStart, editorView.VisableTrackEnd) - trackIndex;
                    DrawTrack(editorView, track, trackIndex, startIndex, endIndex);
                } while (false);
                trackIndex += trackVisableCount;
            }
        }

        public static void DrawTrackHead(FrameLineEditorView editorView, FrameLineTrack track, int viewTrackIndex, float width)
        {
            float viewOffsetY = viewTrackIndex * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval);
            int visableSubTrackCount = track.Foldout ? track.Count : 1;
            float trackHeight = ViewStyles.TrackHeight * visableSubTrackCount - ViewStyles.ClipVInterval;
            Rect rect = new Rect(ViewStyles.TrackFoldSize, viewOffsetY, width - ViewStyles.TrackFoldSize, trackHeight);
            GUIRenderHelper.DrawRect(rect, ViewStyles.TrackBGColor, 5, BorderType.Left);
            Rect titleRect = rect;
            titleRect.height = ViewStyles.ClipHeight;
            GUI.Label(titleRect, track.Name);
            if (track.Count > 1)
            {
                Rect foldRect = new Rect(0, viewOffsetY, ViewStyles.TrackFoldSize, ViewStyles.ClipHeight);
                track.Foldout = EditorGUI.Foldout(foldRect, track.Foldout, "");
            }
        }
        public static void DrawTrack(FrameLineEditorView editorView, FrameLineTrack track, int viewTrackIndex, int startSubIndex, int endSubIndex)
        {
            float viewOffsetY = viewTrackIndex * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval);
            int visableSubTrackCount = track.Foldout ? track.Count : 1;
            float trackHeight = ViewStyles.TrackHeight * visableSubTrackCount - ViewStyles.ClipVInterval;
            Rect rect = new Rect(0, viewOffsetY, editorView.FrameCount * ViewStyles.FrameWidth, trackHeight);
            GUIRenderHelper.DrawRect(rect, ViewStyles.TrackBGColor);
            if (!track.Foldout)
            {
                //画被折叠的轨道
                for (int i = 1; i < track.Count; ++i)
                {
                    var action = editorView.Group.Find(track.Actions[i]);
                    if (action.StartFrame > editorView.VisableFrameEnd || (action.Length > 0 && action.StartFrame + action.Length < editorView.VisableFrameStart))
                        continue;
                    float offsetY = viewOffsetY;
                    float offsetX = action.StartFrame * ViewStyles.FrameWidth;
                    int frameCount = action.Length;
                    if (action.Length <= 0 || frameCount > (editorView.FrameCount - action.StartFrame))
                        frameCount = editorView.FrameCount - action.StartFrame;
                    Rect clipRect = new Rect(offsetX, offsetY, ViewStyles.FrameWidth * frameCount, ViewStyles.ClipHeight);
                    GUIRenderHelper.DrawRect(clipRect, ViewStyles.InvalidClipColor, 5, BorderType.All);
                }
            }
            for (int i = 0; i < track.Count; ++i)
            {
                var action = editorView.Group.Find(track.Actions[i]);
                if (i < startSubIndex || i > endSubIndex)
                    continue;
                if (action.StartFrame > editorView.VisableFrameEnd || (action.Length > 0 && action.StartFrame + action.Length < editorView.VisableFrameStart))
                    continue;
                float offsetY = viewOffsetY + i * ViewStyles.TrackHeight;
                float offsetX = action.StartFrame * ViewStyles.FrameWidth;
                int frameCount = action.Length;
                if (action.Length <= 0 || frameCount > (editorView.FrameCount - action.StartFrame))
                    frameCount = editorView.FrameCount - action.StartFrame;
                //左侧控制区域
                Rect clipLeftCtrlRect = new Rect(offsetX, offsetY, ViewStyles.ClipCtrlWidth, ViewStyles.ClipHeight);
                FrameActionHitPartType dragPart = editorView.EventHandler.GetDragePart(action);
                Color color = dragPart == FrameActionHitPartType.LeftCtrl ? ViewStyles.ClipSelectCtrlColor : ViewStyles.ClipCtrlColor;
                GUIRenderHelper.DrawRect(clipLeftCtrlRect, color, ViewStyles.ClipCtrlWidth, BorderType.Left);
                //右侧
                int clipEndFrame = action.StartFrame + frameCount - 1;
                Rect clipRightCtrlRect = new Rect((clipEndFrame + 1) * ViewStyles.FrameWidth - ViewStyles.ClipCtrlWidth,
                    offsetY,
                    ViewStyles.ClipCtrlWidth,
                    ViewStyles.ClipHeight);
                if (action.Length > 0)
                {
                    color = dragPart == FrameActionHitPartType.RightCtrl ? ViewStyles.ClipSelectCtrlColor : ViewStyles.ClipCtrlColor;
                    GUIRenderHelper.DrawRect(clipRightCtrlRect, color, ViewStyles.ClipCtrlWidth, BorderType.Right);
                }
                else
                {
                    GUIRenderHelper.DrawRect(clipRightCtrlRect, ViewStyles.ClipColor);
                }
                //中间区域
                Rect clipRect = new Rect(clipLeftCtrlRect.xMax, offsetY, clipRightCtrlRect.xMin - clipLeftCtrlRect.xMax, ViewStyles.ClipHeight);
                GUIRenderHelper.DrawRect(clipRect, ViewStyles.ClipColor);
                if (editorView.IsSlecected(action))
                {
                    Rect fullRect = new Rect(offsetX, offsetY, ViewStyles.FrameWidth * frameCount, ViewStyles.ClipHeight);
                    GUIRenderHelper.DrawWireRect(fullRect, ViewStyles.SelectClipWireFrameColor, ViewStyles.ClipCtrlWidth, BorderType.All);
                }
            }
        }
    }
}
