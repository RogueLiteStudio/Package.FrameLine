using UnityEditor;

namespace FrameLine
{
    public interface IFrameLineClipMenu
    {
        void OnClipMenu(FrameLineEditorView editorView, GenericMenu menu, int frameOffset);
    }
}
