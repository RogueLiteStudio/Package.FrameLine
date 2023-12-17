using UnityEngine;
namespace FrameLine
{
    public static class ActionOperateHelper
    {
        public static void RemoveSelectedAction(this FrameLineEditorView gui)
        {
            if (gui.SelectedActions.Count == 0)
                return;
            gui.RegistUndo("remove action");
            foreach (var id in gui.SelectedActions)
            {
                int idx = gui.Group.Actions.FindIndex((a) => a.GUID == id);
                if (idx >= 0)
                {
                    gui.Group.Actions.RemoveAt(idx);
                    gui.OnRemoveAction(id);
                }
            }
            gui.SelectedActions.Clear();
        }

        public static void PasteActions(this FrameLineEditorView gui, FrameLineClipboard.ActionData[] actions)
        {
            if (actions == null || actions.Length == 0)
                return;
            gui.RegistUndo("paste action");
            gui.SelectedActions.Clear();
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
                gui.Group.Actions.Add(action);
                gui.OnAddAction(action);
                gui.SelectedActions.Add(action.GUID);
            }
        }

        public static void MoveSelectedActions(this FrameLineEditorView gui, int offsetFrame)
        {
            foreach (var id in gui.SelectedActions)
            {
                var action = gui.Group.Find(id);
                if (action == null)
                    continue;
                int startFrame = action.StartFrame - offsetFrame;
                startFrame = Mathf.Clamp(startFrame, 0, gui.FrameCount - 1);
                action.StartFrame = startFrame;
            }
        }

        public static void MoveActionStart(this FrameActionGroup group, FrameAction action, int frame)
        {
            if (frame < 0)
                return;
            int endFrame = FrameActionUtil.GetClipEndFrame(group, action);
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

        public static void SetSelectLengthToEnd(this FrameLineEditorView gui)
        {
            if (gui.SelectedActions.Count == 0)
                return;
            gui.RegistUndo("set length to end");
            foreach (var id in gui.SelectedActions)
            {
                var action = gui.Group.Find(id);
                if (action != null)
                    action.Length = gui.Group.FrameCount - action.StartFrame;
            }
        }
    }
}
