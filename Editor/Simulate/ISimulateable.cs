namespace FrameLine
{
    public interface ISimulateable
    {
        public void Simulate(FrameLineSimulate simulate, int frameOffset, int length);
    }

    public interface IGizmosable
    {
        void DrawGizmos(FrameLineSimulate simulate, bool isSelect, int frameOffset, int length);
    }
}
