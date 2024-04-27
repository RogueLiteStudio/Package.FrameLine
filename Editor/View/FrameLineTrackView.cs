using UnityEngine;

namespace FrameLine
{
    [System.Serializable]
    public class FrameLineTrackView
    {
        public bool OnGUI(FrameLineEditorView editorView, Vector2 size)
        {
            int framCount = editorView.Group.FrameCount;
            int actionMaxFrame = Mathf.Max(framCount + 2, Mathf.RoundToInt((size.x - 20) / ViewStyles.FrameWidth));
            bool repaint = false;
            foreach (var action in editorView.Group.Actions)
            {
                if (action.Length > 0)
                {
                    actionMaxFrame = Mathf.Max(actionMaxFrame, action.StartFrame + action.Length);
                }
                else
                {
                    actionMaxFrame = Mathf.Max(actionMaxFrame, action.StartFrame);
                }
            }
            //actionMaxFrame是为了兼容Group的FrameCount被修改，Action的StartFrame或者EndFrame超出区域
            framCount = Mathf.Max(framCount, actionMaxFrame);
            float frameHeight = editorView.Group.Actions.Count * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval) + ViewStyles.ClipVInterval;
            float frameWidth = framCount * ViewStyles.FrameWidth + 10;
            Rect clipRect = new Rect(0, 0, size.x - ViewStyles.ScrollBarSize, size.y - ViewStyles.ScrollBarSize);
            using (new GUI.ClipScope(clipRect))
            {
                //画帧标号背景条
                GUI.Box(new Rect(0 - ViewStyles.ScrollBarSize*0.5f, 0, size.x, ViewStyles.FrameBarHeight), "");
                //整个可以左右滚动的区域
                using (new GUILayout.AreaScope(new Rect(-editorView.ScrollPos.x, 0, frameWidth, clipRect.height)))
                {
                    FrameLineDrawer.DrawFrameLineBackGround(editorView, new Rect(new Vector2(editorView.ScrollPos.x, 0), clipRect.size), framCount);
                    {
                        var e = Event.current;
                        if (e.mousePosition.y < ViewStyles.FrameBarHeight || editorView.EventHandler.IsDragFrameBar)
                        {
                            repaint |= editorView.EventHandler.OnFrameBarEvent(e);
                        }
                    }
                    float viewHeight = Mathf.Max(frameHeight, clipRect.height);
                    //轨道条区域
                    using (new GUI.ClipScope(new Rect(0, ViewStyles.FrameBarHeight, frameWidth, viewHeight)))
                    {
                        Rect trackViewRect = new Rect(0, -editorView.ScrollPos.y, frameWidth, viewHeight);
                        using (new GUILayout.AreaScope(trackViewRect))
                        {
                            var e = Event.current;
                            Vector2 mousePos = e.mousePosition;
                            Rect showRect = new Rect(editorView.ScrollPos.x, editorView.ScrollPos.y, size.x - ViewStyles.ScrollBarSize, size.y - ViewStyles.FrameBarHeight - ViewStyles.ScrollBarSize);
                            bool mouseInView = showRect.Contains(mousePos);
                            if (mouseInView)
                            {
                                if (editorView.EventHandler.OnFrameClipsEvent(e))
                                {
                                    repaint = true;
                                    e.Use();
                                }
                            }
                            FrameLineDrawer.DrawFrameActions(editorView);
                        }
                    }
                }
            }

            //滚动条
            {
                float viewHeight = size.y - ViewStyles.FrameBarHeight - ViewStyles.ScrollBarSize;
                Rect vBarRect = new Rect(size.x - ViewStyles.ScrollBarSize, 0, ViewStyles.ScrollBarSize, size.y - ViewStyles.ScrollBarSize);
                editorView.ScrollPos.y = GUI.VerticalScrollbar(vBarRect, editorView.ScrollPos.y, viewHeight, 0, frameHeight);
                editorView.ScrollPos.y = Mathf.Max(0, editorView.ScrollPos.y);
                float viewWidth = size.x - ViewStyles.ScrollBarSize;
                Rect hBarRect = new Rect(0, size.y - ViewStyles.ScrollBarSize, size.x - ViewStyles.ScrollBarSize, ViewStyles.ScrollBarSize);
                editorView.ScrollPos.x = GUI.HorizontalScrollbar(hBarRect, editorView.ScrollPos.x, viewWidth, 0, frameWidth);
                editorView.ScrollPos.x = Mathf.Max(0, editorView.ScrollPos.x);
            }
            return repaint;
        }
    }
}
