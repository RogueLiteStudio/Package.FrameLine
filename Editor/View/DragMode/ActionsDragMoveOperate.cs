using UnityEngine;

namespace FrameLine
{
    public class ActionsDragMoveOperate : DragOperateBase
    {
        public ActionsDragMoveOperate(FrameLineEditorView view, int frame) : base(view)
        {
            lastFrame = frame;
        }

        protected override void OnDrag(Vector2 pos, int frame)
        {
            foreach (var id in EditorView.SelectedActions)
            {
                var action = EditorView.Group.Find(id);
                if (action == null)
                    continue;

                int startFrame = action.StartFrame + (frame - lastFrame);
                startFrame = Mathf.Clamp(startFrame, 0, EditorView.FrameCount - 1);
                action.StartFrame = startFrame;
            }
        }
    }
}
