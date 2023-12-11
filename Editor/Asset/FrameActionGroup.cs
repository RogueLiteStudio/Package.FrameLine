using System.Collections.Generic;

namespace FrameLine
{
    [System.Serializable]
    public class FrameActionGroup
    {
        public string GUID;
        public string Name;
        public string Description;
        public int FrameCount;
        public List<FrameAction> Actions = new List<FrameAction>();
    }
}
