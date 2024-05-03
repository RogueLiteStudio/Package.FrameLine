namespace FrameLine
{
    public interface ISimulateable
    {
        System.Type GetSimulatorType();
    }

    public interface IGizmosable
    {
        void DrawGizmos(FrameLineSimulate simulate, bool isSelect, int frameOffset, int length);
    }
}
