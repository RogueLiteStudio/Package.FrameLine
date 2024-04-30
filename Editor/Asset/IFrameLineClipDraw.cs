using UnityEngine;

namespace FrameLine
{
    public interface IFrameLineClipDraw
    {
        void OnDrawClip(FrameLineEditorView editorView,Vector2 size);
        DragOperateBase OnDragEvent(FrameLineEditorView editorView, int frameOffset, FrameActionHitResult hitResult) => null;
    }
}
