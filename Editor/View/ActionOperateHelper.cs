using UnityEngine;
namespace FrameLine
{
    public static class ActionOperateHelper
    {
        public static void RemoveSelectedAction(this FrameLineEditorView view)
        {
            if (view.SelectedActions.Count == 0)
                return;
            view.RegisterUndo("remove action");
            foreach (var id in view.SelectedActions)
            {
                int idx = view.Group.Actions.FindIndex((a) => a.GUID == id);
                if (idx >= 0)
                {
                    view.Group.Actions.RemoveAt(idx);
                    view.OnRemoveAction(id);
                }
            }
            view.SelectedActions.Clear();
            view.RebuildTrack();
        }

        public static void PasteActions(this FrameLineEditorView view, FrameLineClipboard.ActionData[] actions)
        {
            if (actions == null || actions.Length == 0)
                return;
            view.RegisterUndo("paste action");
            view.SelectedActions.Clear();
            foreach (var data in actions) 
            {
                var action = new FrameAction 
                {
                    GUID = System.Guid.NewGuid().ToString(),
                    StartFrame = data.StartFrame,
                    Length = data.Length,
                    Enable = data.Enable,
                    Name = data.Name,
                    Comment = data.Comment,
                };
                action.SetData(TypeSerializerHelper.Deserialize(data.Data) as IFrameAction);
                view.Group.Actions.Add(action);
                view.OnAddAction(action);
                view.SelectedActions.Add(action.GUID);
            }
            view.RebuildTrack();
        }

        public static void MoveSelectedActions(this FrameLineEditorView view, int offsetFrame)
        {
            foreach (var id in view.SelectedActions)
            {
                var action = view.Group.Find(id);
                if (action == null)
                    continue;
                int startFrame = action.StartFrame - offsetFrame;
                startFrame = Mathf.Clamp(startFrame, 0, view.FrameCount - 1);
                action.StartFrame = startFrame;
            }
        }

        public static void MoveActionStart(this FrameActionGroup group, FrameAction action, int frame)
        {
            if (frame < 0)
                return;
            int endFrame = FrameActionUtil.GetActionEndFrame(group, action);
            if (frame > endFrame)
                return;
            int lastStart = action.StartFrame;
            action.StartFrame = frame;
            if (action.Length > 0)
            {
                int length = action.Length - (frame - lastStart);
                action.Length = Mathf.Max(length, 1);
            }
        }

        public static void MoveActionEnd(this FrameActionGroup group, FrameAction action, int frame)
        {
            if (frame >= group.FrameCount || frame < action.StartFrame)
                return;
            if (frame >= group.FrameCount && frame >= action.StartFrame + action.Length)
                return;
            if (action.Length <= 0 && frame == (group.FrameCount - 1))
                return;
            action.Length = Mathf.Max(frame - action.StartFrame + 1, 1);
        }

        public static void SetSelectLengthToEnd(this FrameLineEditorView view)
        {
            if (view.SelectedActions.Count == 0)
                return;
            view.RegisterUndo("set length to end");
            foreach (var id in view.SelectedActions)
            {
                var action = view.Group.Find(id);
                if (action != null && action.Data is IFrameClip)
                    action.Length = view.Group.FrameCount - action.StartFrame;
            }
        }
    }
}
