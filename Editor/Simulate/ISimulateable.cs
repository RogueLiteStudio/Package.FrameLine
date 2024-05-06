namespace FrameLine
{
    public interface ISimulateable
    {
        System.Type GetSimulatorType();
    }

    public interface IGizmosable
    {
        void DrawGizmos(FrameLineEditorView editorView, bool isSelect, int startFrame, int endFrame, int currentFrame);
    }
}
