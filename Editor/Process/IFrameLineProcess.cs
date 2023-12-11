using System;

namespace FrameLine
{
    public interface IFrameLineProcess
    {
        Type EditorWindowType { get; }
        FrameLineAsset OnCreateAction();
        void OnSave(FrameLineAsset asset);
    }
}
