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
        void OnCreate(FrameLineSimulator context, FrameAction action);
        void OnUpdate(FrameLineSimulator context, FrameAction action, SimulateFrameData frameData);
        void OnDispose(FrameLineSimulator context, FrameAction action);
    }

    public abstract class TActionSimulator<TSimulate, TAction> : IActionSimulator
        where TSimulate : FrameLineSimulator
        where TAction : IFrameAction
    {
        protected TSimulate context;
        public void OnCreate(FrameLineSimulator context, FrameAction action)
        {
            this.context = context as TSimulate;
            OnCreate((TAction)action.Data);
        }

        public void OnUpdate(FrameLineSimulator context, FrameAction action, SimulateFrameData frameData)
        {
            OnUpdate((TAction)action.Data, frameData);
        }

        public void OnDispose(FrameLineSimulator context, FrameAction action)
        {
            OnDispose((TAction)action.Data);
        }

        protected virtual void OnCreate(TAction action) { }
        protected abstract void OnUpdate(TAction action, SimulateFrameData frameData);
        protected virtual void OnDispose(TAction action) { }
    }
}
