using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    [System.Serializable]
    public class FrameLineGUIEvent
    {
        private DragOperateBase dragOperate;
        [SerializeField]
        private FrameLineEditorView EditorView;
        public bool IsDragFrameBar { get; private set; }

        public FrameLineGUIEvent(FrameLineEditorView gui)
        {
            EditorView = gui;
        }

        public FrameActionHitPartType GetDragePart(FrameAction action)
        {
            if (dragOperate != null)
                return dragOperate.GetDragePart(action);
            return FrameActionHitPartType.None;
        }
        public FrameActionHitResult HitTest(Vector2 point)
        {
            int hitFrame = FrameLineUtil.PosToFrame(point.x);
            FrameActionHitResult result = new FrameActionHitResult() { Frame = hitFrame, ClickPos = point };

            if (hitFrame < EditorView.FrameCount && hitFrame >= 0)
            {
                int hitTrackIndex = Mathf.FloorToInt(point.y / ViewStyles.TrackHeight);
                int preTrackCount = 0;
                for (int i = 0; i < EditorView.Tracks.Count; ++i)
                {
                    var track = EditorView.Tracks[i];
                    int hitSubIndex = hitTrackIndex - preTrackCount;
                    if (hitSubIndex < 0)
                        break;
                    int subTrakCount = track.Foldout ? track.Count : 1;
                    preTrackCount += subTrakCount;
                    if (hitSubIndex >= subTrakCount)
                        continue;

                    var action = EditorView.Group.Find(track.Actions[hitSubIndex]);
                    if (action.StartFrame <= hitFrame)
                    {
                        int endFrame = FrameActionUtil.GetActionEndFrame(EditorView.Group, action);
                        if (endFrame >= hitFrame)
                        {
                            do 
                            {
                                if (action.Data is not IFrameEvent)
                                {
                                    float frameOffset = point.x % ViewStyles.FrameWidth;
                                    if (frameOffset <= ViewStyles.ClipCtrlWidth && action.StartFrame == hitFrame)
                                    {
                                        result.HitPart = FrameActionHitPartType.LeftCtrl;
                                        break;
                                    }
                                    else if (action.Length > 0 && endFrame == hitFrame && (frameOffset >= (ViewStyles.FrameWidth - ViewStyles.ClipCtrlWidth)))
                                    {
                                        result.HitPart = FrameActionHitPartType.RightCtrl;
                                        break;
                                    }
                                }
                                result.HitPart = FrameActionHitPartType.Normal;
                            } while (false);
                            result.Action = action;
                        }
                    }
                    if (result.Action != null)
                        break;
                }
            }
            return result;
        }

        public bool OnTrackHeadEvent(Event e)
        {
            if (e.type == EventType.Repaint || e.type == EventType.Layout)
                return false;
            if (IsDragFrameBar)
                return false;
            if (e.button == 1 && e.type == EventType.MouseUp)
            {
                FrameLineTrack hitTrack = null;
                int hitTrackIndex = Mathf.FloorToInt(e.mousePosition.y / ViewStyles.TrackHeight);
                int trackIndex = 0;
                for (int i=0; i<EditorView.Tracks.Count; ++i)
                {
                    var track = EditorView.Tracks[i];
                    if (track.Count == 0)
                        continue;
                    if (trackIndex > hitTrackIndex)
                        break;
                    int trackVisableCount = (track.Foldout ? track.Count : 1);
                    if (trackIndex <= hitTrackIndex && trackIndex + trackVisableCount > hitTrackIndex)
                    {
                        hitTrack = track;
                        break;
                    }
                }
                GenericMenu menu = new GenericMenu();
                if (hitTrack != null)
                {
                    menu.AddItem(new GUIContent("全选"), false, () => 
                    {
                        EditorView.SelectedActions.Clear();
                        EditorView.SelectedActions.AddRange(hitTrack.Actions);
                    });
                    menu.AddItem(new GUIContent("展开"), hitTrack.Foldout, () => hitTrack.Foldout = !hitTrack.Foldout);
                }
                EditorView.OnTrackHeadMenue(menu, hitTrack);
                if (menu.GetItemCount() > 0)
                {
                    menu.ShowAsContext();
                }
                e.Use();
                return true;
            }
            return false;
        }

        public bool OnFrameBarEvent(Event e)
        {
            if (e.type == EventType.Repaint || e.type == EventType.Layout)
                return false;
            if (e.button == 0)
            {
                if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    int selectFrame = Mathf.FloorToInt(e.mousePosition.x / ViewStyles.FrameWidth);
                    if (selectFrame >= 0 && selectFrame < EditorView.Group.FrameCount)
                    {
                        if (EditorView.CurrentFrame != selectFrame)
                        {
                            EditorView.CurrentFrame = selectFrame;
                            if (selectFrame == EditorView.VisableFrameStart)
                            {
                                EditorView.ScrollToFrame(selectFrame - 1);
                            }
                            else if (selectFrame >= (EditorView.VisableFrameEnd - 2))
                            {
                                EditorView.ScrollToFrame(selectFrame + 1);
                            }
                        }
                    }
                    IsDragFrameBar = true;
                    e.Use();
                    return true;
                }
                else if(e.type == EventType.MouseUp)
                {
                    IsDragFrameBar = false;
                    e.Use();
                    return true;
                }
            }
            if (e.button == 1 && e.type == EventType.MouseUp)
            {
                GenericMenu menu = new GenericMenu();
                EditorView.OnFrameBarMenue(menu);
                if (menu.GetItemCount() > 0)
                {
                    menu.ShowAsContext();
                }
                e.Use();
                return true;
            }
            return false;
        }

        public bool OnFrameClipsEvent(Event e)
        {
            if (e.type == EventType.Repaint || e.type == EventType.Layout)
                return false;
            if (IsDragFrameBar)
                return false;
            if (dragOperate != null && e.button == 0)
            {
                //普通的点击也会触发DragOperate,所以这里不能直接返回
                if (e.type == EventType.MouseDrag)
                {
                    dragOperate.Drag(e.mousePosition);
                    return true;
                }
                else if (e.type == EventType.MouseUp)
                {
                    if (dragOperate.HasDraged)
                    {
                        dragOperate.OnDragEnd();
                    }
                    else
                    {
                        bool isMultSelect = (e.modifiers & (EventModifiers.Control | EventModifiers.Command)) != 0;
                        if (!isMultSelect)
                        {
                            var hitTest = HitTest(e.mousePosition);
                            if (hitTest.HitPart == FrameActionHitPartType.Normal || hitTest.HitPart == FrameActionHitPartType.None)
                            {
                                EditorView.SelectedActions.Clear();
                                if (hitTest.Action != null)
                                {
                                    EditorView.SelectedActions.Add(hitTest.Action.GUID);
                                }
                            }
                        }
                    }
                    dragOperate = null;
                    return true;
                }
            }
            if (e.button == 0)
            {
                bool isMultSelect = (e.modifiers & (EventModifiers.Control | EventModifiers.Command)) != 0;
                if (e.type == EventType.MouseDown)
                {
                    var hitTest = HitTest(e.mousePosition);
                    switch (hitTest.HitPart)
                    {
                        case FrameActionHitPartType.None:
                            if (!isMultSelect)
                                EditorView.SelectedActions.Clear();
                            return true;
                        case FrameActionHitPartType.Normal:
                            if (isMultSelect)
                            {
                                int selectedIdx = EditorView.SelectedActions.IndexOf(hitTest.Action.GUID);
                                if (selectedIdx >= 0)
                                {
                                    EditorView.SelectedActions.RemoveAt(selectedIdx);
                                }
                                else
                                {
                                    EditorView.SelectedActions.Add(hitTest.Action.GUID);
                                }
                            }
                            else
                            {
                                if (!EditorView.SelectedActions.Contains(hitTest.Action.GUID))
                                {
                                    EditorView.SelectedActions.Clear();
                                    EditorView.SelectedActions.Add(hitTest.Action.GUID);
                                }
                            }
                            dragOperate = new ActionsDragMoveOperate(EditorView, hitTest.Frame);
                            return true;
                        case FrameActionHitPartType.LeftCtrl:
                            dragOperate = new ActionDragStartOperate(EditorView, hitTest.Action);
                            return true;
                        case FrameActionHitPartType.RightCtrl:
                            dragOperate = new ActionDragEndOperate(EditorView, hitTest.Action);
                            return true;
                        default:
                            break;
                    }
                }
            }
            if (e.button == 1)
            {
                if (e.type == EventType.MouseDown)
                {
                    if (EditorView.SelectedActions.Count > 0)
                    {
                        GenericMenu menu = new GenericMenu();
                        EditorView.OnSelectActionMenue(menu);
                        menu.AddItem(new GUIContent("设置时长到动画结束"), false, () => EditorView.SetSelectLengthToEnd());
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("删除"), false, () => EditorView.RemoveSelectedAction());
                        menu.ShowAsContext();
                    }
                    
                }
            }
            if (e.type == EventType.KeyDown)
            {
                if (e.command || e.control)
                {
                    switch (e.keyCode)
                    {
                        case KeyCode.A:
                            EditorView.SelectedActions.Clear();
                            foreach (var action in EditorView.Group.Actions)
                            {
                                EditorView.SelectedActions.Add(action.GUID);
                            }
                            return true;
                        case KeyCode.D:
                            if (EditorView.SelectedActions.Count > 0)
                            {
                                var datas = FrameLineClipboard.ActionToClipboardData(EditorView.SelectedActions.Select(it => EditorView.Group.Find(it)).Where(it=>it!=null));
                                EditorView.PasteActions(datas);
                                return true;
                            }
                            break;
                        case KeyCode.C:
                            if (EditorView.SelectedActions.Count > 0)
                            {
                                FrameLineClipboard.instance.CopyActions(EditorView);
                                return true;
                            }
                    	    break;
                        case KeyCode.V:
                            if (FrameLineClipboard.instance.PasteActions(EditorView))
                                return true;
                            break;
                    }
                }
                else if (e.keyCode == KeyCode.Delete)
                {
                    EditorView.RemoveSelectedAction();
                    return true;
                }
            }
            return false;
        }
    }
}
