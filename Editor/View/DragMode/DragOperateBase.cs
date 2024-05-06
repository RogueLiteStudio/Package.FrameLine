using UnityEngine;

namespace FrameLine
{
    public abstract class DragOperateBase
    {
        protected FrameLineEditorView EditorView;
        protected int lastFrame;
        protected bool hasModify { get; private set; }
        public bool HasDraged { get; private set; }
        public DragOperateBase(FrameLineEditorView view)
        {
            EditorView = view;
        }
        public void Drag(Vector2 pos)
        {
            HasDraged = true;
            int frame = FrameLineUtil.PosToFrame(pos.x);

            if (frame != lastFrame)
            {
                if (!hasModify)
                {
                    EditorView.RegisterUndo("drag clip start");
                    hasModify = true;
                }
                OnDrag(pos, frame);
                lastFrame = frame;
            }
        }
        protected abstract void OnDrag(Vector2 pos, int frame);
        public virtual void OnDragEnd() 
        {
            if (hasModify)
            {
                FrameTrackUtil.UpdateAllTrack(EditorView);
            }
        }

        public virtual FrameActionHitPartType GetDragePart(FrameAction action)
        {
            return FrameActionHitPartType.None;
        }
    }
}
