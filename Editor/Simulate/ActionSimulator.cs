namespace FrameLine
{
    public struct SimulateFrameData
    {
        public int FrameOffset;
        public int Length;
        public float FrameTime;
        public bool IsSelected;
        public ActionSimulateState State;
        public readonly float Time => FrameTime * FrameOffset;
    }

    public enum ActionSimulateState
    {
        Start,
        Update,
        Exit,
    }

    public interface IActionSimulator
    {
        void OnCreate(FrameLineSimulate context, FrameAction action);
        void OnUpdate(FrameLineSimulate context, FrameAction action, SimulateFrameData frameData);
        void OnDispose(FrameLineSimulate context, FrameAction action);
    }

    public abstract class TActionSimulator<TSimulate, TAction> : IActionSimulator
        where TSimulate : FrameLineSimulate
        where TAction : IFrameAction
    {
        protected TSimulate context;
        public void OnCreate(FrameLineSimulate context, FrameAction action)
        {
            this.context = context as TSimulate;
            OnCreate((TAction)action.Data);
        }

        public void OnUpdate(FrameLineSimulate context, FrameAction action, SimulateFrameData frameData)
        {
            OnUpdate((TAction)action.Data, frameData);
        }

        public void OnDispose(FrameLineSimulate context, FrameAction action)
        {
            OnDispose((TAction)action.Data);
        }

        protected virtual void OnCreate(TAction action) { }
        protected abstract void OnUpdate(TAction action, SimulateFrameData frameData);
        protected virtual void OnDispose(TAction action) { }
    }
}
