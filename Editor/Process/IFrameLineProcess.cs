using System;

namespace FrameLine
{
    public interface IFrameLineProcess
    {
        Type EditorWindowType { get; }
        Type EditorViewType => typeof(FrameLineEditorView);
        FrameLineAsset OnCreateAction();
        bool CheckIsValidNodeType(Type type);
        void OnSave(FrameLineAsset asset);
    }
}
