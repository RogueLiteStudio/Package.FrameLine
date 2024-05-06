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

    public interface ISimulateGizmosable
    {
        void DrawGizmos(FrameLineSimulator simulator, bool isSelect, int startFrame, int endFrame, int currentFrame);
    }
}
