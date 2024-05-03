using System;

namespace FrameLine
{
    public interface IFrameLineProcess
    {
        Type EditorWindowType { get; }
        Type EditorViewType => typeof(FrameLineEditorView);
        Type SimulatorType { get; }
        FrameLineAsset OnCreateAction();
        bool CheckIsValidNodeType(Type type);
        void OnSave(FrameLineAsset asset);
    }
}
