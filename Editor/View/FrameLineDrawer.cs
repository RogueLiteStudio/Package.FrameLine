using UnityEditor;
using UnityEngine;
namespace FrameLine
{
    public static class FrameLineDrawer
    {
        public static void DrawFrameLineBackGround(FrameLineEditorView gui, Rect showRect, int showMaxFrame)
        {
            int startIndex = Mathf.Clamp(gui.VisableFrameStart, 0, gui.Group.FrameCount);
            int endIndex = Mathf.Clamp(gui.VisableFrameEnd, 0, showMaxFrame);
            for (int i = startIndex; i <= endIndex; ++i)
            {
                float xPos = i * ViewStyles.FrameWidth;
                if (i <= gui.Group.FrameCount)
                {
                    if (i % 5 == 0)
                    {
                        using (new Handles.DrawingScope(ViewStyles.BGGridLineColor))
                        {
                            Handles.DrawLine(new Vector2(xPos, showRect.yMin), new Vector2(xPos, showRect.yMax));
                        }
                    }
                    else
                    {
                        using (new Handles.DrawingScope(ViewStyles.BGGridDotLineColor))
                        {
                            Handles.DrawDottedLine(new Vector2(xPos, showRect.yMin + ViewStyles.FrameBarHeight*0.5f), new Vector2(xPos, showRect.yMax), 5f);
                        }
                    }
                }
                if (i < endIndex && i % 5 == 0)
                {
                    GUI.Label(new Rect(xPos, 0, ViewStyles.FrameWidth, ViewStyles.FrameBarHeight*0.5f), i.ToString(), ViewStyles.FrameNumStyle);
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
            float viewOffsetY = viewTrackIndex * ViewStyles.TrackHeight;
            int visableSubTrackCount = track.Foldout ? track.Count : 1;
            float trackHeight = ViewStyles.TrackHeight * visableSubTrackCount - ViewStyles.ClipVInterval;
            Rect rect = new Rect(0, viewOffsetY, width, trackHeight);
            GUIRenderHelper.DrawRect(rect, ViewStyles.TrackBGColor, 5, BorderType.Left);
            Rect colorRect = rect;
            colorRect.width = ViewStyles.TrackFoldSize;
            GUIRenderHelper.DrawRect(colorRect, track.TypeColor, 5, BorderType.Left);
            Rect titleRect = rect;
            titleRect.height = ViewStyles.ClipHeight;
            Rect foldRect = new Rect(0, viewOffsetY, width-50, ViewStyles.ClipHeight);
            track.Foldout = EditorGUI.Foldout(foldRect, track.Foldout, track.Name, true);
            for (int i=0; i<track.Count; ++i)
            {
                var action = editorView.Group.Find(track.Actions[i]);
                if (action == null)
                    continue;
                int endFrame = FrameActionUtil.GetActionEndFrame(editorView.Group, action);
                if (action.StartFrame > editorView.VisableFrameEnd - 2)
                {
                    //提示开始帧，点击跳转到开始帧
                    Rect tipRect = new Rect(width - 50, viewOffsetY + i*ViewStyles.TrackHeight, 50, ViewStyles.ClipHeight);
                    if (GUI.Button(tipRect, $"{action.StartFrame}>", ViewStyles.ActionSkipStyle))
                    {
                        editorView.ScrollToFrame(action.StartFrame);
                    }
                }
                if (endFrame < editorView.VisableFrameStart)
                {
                    //提示结束帧，点击跳转到开始帧
                    Rect tipRect = new Rect(width - 50, viewOffsetY + i * ViewStyles.TrackHeight, 50, ViewStyles.ClipHeight);
                    if (GUI.Button(tipRect, $"<{endFrame}", ViewStyles.ActionSkipStyle))
                    {
                        editorView.ScrollToFrame(action.StartFrame);
                    }
                }
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
                    
                    if (action.Data is IFrameEvent)
                    {
                        frameCount = 1;
                    }
                    else if (action.Length <= 0)
                    {
                        frameCount = editorView.FrameCount - action.StartFrame;
                    }
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
                if (action.Data is IFrameEvent)
                {
                    frameCount = 1;
                }
                else if (action.Length <= 0)
                {
                    frameCount = editorView.FrameCount - action.StartFrame;
                }
                Rect fullRect = new Rect(offsetX, offsetY, ViewStyles.FrameWidth * frameCount, ViewStyles.ClipHeight);
                if (action.Data is IFrameEvent)
                {
                    //事件类型Action
                    //画背景
                    GUIRenderHelper.DrawRect(fullRect, ViewStyles.EventColor, ViewStyles.ClipCtrlWidth, BorderType.Bottom);
                    //底部颜色条
                    Rect colorRect = fullRect;
                    colorRect.y += colorRect.height - ViewStyles.ClipColorHeight;
                    colorRect.height = ViewStyles.ClipColorHeight;
                    GUIRenderHelper.DrawRect(colorRect, track.TypeColor, ViewStyles.ClipCtrlWidth, BorderType.Bottom);
                }
                else
                {
                    //画背景
                    GUIRenderHelper.DrawRect(fullRect, ViewStyles.ClipColor, ViewStyles.ClipCtrlWidth, BorderType.All);
                    //底部颜色条
                    Rect colorRect = fullRect;
                    colorRect.y += colorRect.height - ViewStyles.ClipColorHeight;
                    colorRect.height = ViewStyles.ClipColorHeight;
                    GUIRenderHelper.DrawRect(colorRect, track.TypeColor, ViewStyles.ClipCtrlWidth, BorderType.Bottom);
                    //左侧控制区域
                    FrameActionHitPartType dragPart = editorView.EventHandler.GetDragePart(action);
                    if (dragPart == FrameActionHitPartType.LeftCtrl)
                    {
                        Rect clipLeftCtrlRect = new Rect(offsetX, offsetY, ViewStyles.ClipCtrlWidth, ViewStyles.ClipHeight);
                        GUIRenderHelper.DrawRect(clipLeftCtrlRect, ViewStyles.ClipSelectCtrlColor, ViewStyles.ClipCtrlWidth, BorderType.Left);
                    }
                    //右侧
                    int clipEndFrame = action.StartFrame + frameCount - 1;
                    if (action.Length > 0)
                    {
                        if (dragPart == FrameActionHitPartType.RightCtrl)
                        {
                            Rect clipRightCtrlRect = new Rect((clipEndFrame + 1) * ViewStyles.FrameWidth - ViewStyles.ClipCtrlWidth,
                                offsetY,
                                ViewStyles.ClipCtrlWidth,
                                ViewStyles.ClipHeight);
                            GUIRenderHelper.DrawRect(clipRightCtrlRect, ViewStyles.ClipSelectCtrlColor, ViewStyles.ClipCtrlWidth, BorderType.Right);
                        }
                    }
                }
                if (editorView.IsSlecected(action))
                {
                    GUIRenderHelper.DrawWireRect(fullRect, ViewStyles.SelectClipWireFrameColor, ViewStyles.ClipCtrlWidth, BorderType.All);
                }
            }
        }
    }
}
