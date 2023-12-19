using UnityEngine;

namespace FrameLine
{
    public static class FrameActionUtil
    {
        public static int SortByStartFrame(FrameAction a, FrameAction b)
        {
            return a.StartFrame - b.StartFrame;

        }
        public static int GetActionEndFrame(FrameActionGroup group, FrameAction action)
        {
            if (action.Data is IFrameEvent)
            {
                return action.StartFrame;
            }
            if (action.Length > 0)
            {
                return Mathf.Clamp(action.Length + action.StartFrame - 1, action.StartFrame, group.FrameCount);
            }
            return group.FrameCount - 1;
        }

        public static bool IsOverlap(FrameAction a, FrameAction b)
        {
            if (a.StartFrame <= b.StartFrame)
            {
                if (a.Length <= 0 || a.StartFrame + a.Length > b.StartFrame)
                {
                    return true;
                }
            }
            if (b.StartFrame <= a.StartFrame)
            {
                if (b.Length <= 0 || b.StartFrame + b.Length > a.StartFrame)
                {
                    return true;
                }
            }
            return false;
        }
    }
}