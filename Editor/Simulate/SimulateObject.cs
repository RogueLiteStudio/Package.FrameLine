using UnityEngine;

namespace FrameLine
{
    public class SimulateObject : ScriptableObject
    {
        [SerializeField]
        protected FrameLineSimulate Owner;
        [SerializeField]
        internal string ClipGUID;

        public virtual void OnReset()
        {

        }
    }
}
