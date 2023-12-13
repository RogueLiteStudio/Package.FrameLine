using UnityEngine;

namespace FrameLine
{
    public class ActionDragStartOperate : DragOperateBase
    {
        private FrameAction action;
        public ActionDragStartOperate(FrameLineEditorView view, FrameAction action) : base(view)
        {
            this.action = action;
            lastFrame = action.StartFrame;
        }

        protected override void OnDrag(Vector2 pos, int frame)
        {
            EditorView.Group.MoveActionStart(action, frame);
        }
        public override FrameActionHitPartType GetDragePart(FrameAction action)
        {
            if (this.action == action)
            {
                return FrameActionHitPartType.LeftCtrl;
            }
            return FrameActionHitPartType.None;
        }
    }
}
