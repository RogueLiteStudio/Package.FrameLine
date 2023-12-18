using System;

namespace FrameLine
{
    public interface IFrameLineProcess
    {
        Type EditorWindowType { get; }
        FrameLineAsset OnCreateAction();
        bool CheckIsValidNodeType(Type type);
        void OnSave(FrameLineAsset asset);
    }
}
