using System.Collections.Generic;
using UnityEngine;

namespace FrameLine
{
    public abstract class FrameLineSimulate : ScriptableObject
    {
        public Transform SceneRoot;
        [SerializeField]
        protected List<FrameLineSceneObject> sceneObjects = new List<FrameLineSceneObject>();
        [SerializeField]
        protected string currentGroupGUID;
        public virtual void OnDestroy()
        {
            foreach (var obj in sceneObjects)
            {
                obj.Destroy();
            }
            sceneObjects.Clear();
        }

        public virtual void OnSceneGUI(FrameActionGroup group, int frameIndex)
        {
            foreach (var action in group.Actions)
            {
                if (action.Enable && action.Data is IGizmosable gizmosable)
                {
                    int length = action.Length;
                    if (length <= 0)
                    {
                        length = group.FrameCount - action.StartFrame;
                    }
                    int offset = frameIndex - action.StartFrame;
                    bool isSelcect = IsSlecected(action);
                    gizmosable.DrawGizmos(this, isSelcect, offset, length);
                }
            }
        }
        public void Simulate(FrameActionGroup group, int frameIndex)
        {
            if (currentGroupGUID != group.GUID)
            {
                currentGroupGUID = group.GUID;
                foreach (var obj in sceneObjects)
                {
                    obj.Destroy();
                }
                sceneObjects.Clear();
            }
            //清理已经被删除的Action
            for (int i = sceneObjects.Count - 1; i >= 0; --i)
            {
                var obj = sceneObjects[i];
                if (!group.Actions.Exists(it => it.GUID == obj.ClipGUID))
                {
                    obj.Destroy();
                    sceneObjects.RemoveAt(i);
                }
            }
            OnBeforSimulate(group, frameIndex);

        }
        protected virtual bool IsSlecected(FrameAction action) { return false; }
        protected virtual void OnBeforSimulate(FrameActionGroup group, int frameIndex) { }
        protected virtual void OnAfterSimulate(FrameActionGroup group, int frameIndex) { }
    }
}
