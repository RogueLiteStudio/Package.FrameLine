using UnityEngine;

namespace FrameLine
{
    public class ActionDragEndOperate : DragOperateBase
    {
        private FrameAction action;
        public ActionDragEndOperate(FrameLineEditorView view, FrameAction action) : base(view)
        {
            this.action = action;
            lastFrame = FrameActionUtil.GetActionEndFrame(view.Group, action);
        }

        protected override void OnDrag(Vector2 pos, int frame)
        {
            EditorView.Group.MoveActionEnd(action, frame);
        }

        public override FrameActionHitPartType GetDragePart(FrameAction action)
        {
            if (this.action.GUID == action.GUID)
            {
                return FrameActionHitPartType.RightCtrl;
            }
            return FrameActionHitPartType.None;
        }
    }
}
