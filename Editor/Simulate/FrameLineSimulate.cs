using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    [System.Serializable]
    public struct SimulateResource
    {
        public string Key;
        public string ActionGUID;
        public Object Resource;
    }

    public abstract class FrameLineSimulate : ScriptableObject
    {
        protected struct Simulator
        {
            public string GUID;
            public bool IsUpdate;
            public FrameAction Action;
            public IActionSimulator ActionSimulator;
        }
        public Transform SceneRoot;
        [SerializeField]
        protected List<SimulateResource> resources = new List<SimulateResource>();
        [SerializeField]
        protected List<Simulator> simulators = new List<Simulator>();
        [SerializeField]
        protected string currentGroupGUID;

        private bool needRebuild = true;

        public void MarkReBuild()
        {
            needRebuild = true;
        }

        protected virtual void OnDisable()
        {
            foreach (var s in simulators)
            {
                s.ActionSimulator.OnDispose(this, s.Action);
            }
            simulators.Clear();
        }

        protected virtual void OnDestroy()
        {
            foreach (var s in resources)
            {
                if (s.Resource)
                {
                    string path = AssetDatabase.GetAssetPath(s.Resource);
                    if (string.IsNullOrEmpty(path))
                    {
                        DestroyImmediate(s.Resource);
                    }
                }
            }
            resources.Clear();
        }

        public T GetResource<T>(string guid, string key) where T : Object
        {
            int idx = resources.FindIndex(it => it.Key == key && it.ActionGUID == guid);
            if (idx >= 0)
            {
                return resources[idx].Resource as T;
            }
            var res = LoadResource<T>(key);
            resources.Add(new SimulateResource { Key = key, ActionGUID = guid, Resource = res });
            return null;
        }

        protected abstract T LoadResource<T>(string path) where T : Object;

        public void Simulate(FrameActionGroup group, int frameIndex, List<string> selectedActions)
        {
            if (needRebuild)
            {
                ReBuild(group);
            }
            OnBeforSimulate(group, frameIndex);
            for (int i=0; i<simulators.Count; ++i)
            {
                var s = simulators[i];
                s.Action = group.Find(s.GUID);
                if (s.Action == null)
                {
                    s.ActionSimulator.OnDispose(this, s.Action);
                    simulators.RemoveAt(i);
                    --i;
                    continue;
                }
                bool isSelect = selectedActions == null || selectedActions.Contains(s.Action.GUID);
                bool isInAction = s.Action.StartFrame <= frameIndex && (s.Action.Length <= 0 || s.Action.StartFrame + s.Action.Length > frameIndex);
                if (!isInAction && !s.IsUpdate)
                    continue;
                ActionSimulateState state = ActionSimulateState.Update;
                if (!isInAction)
                {
                    if (s.IsUpdate)
                    {
                        s.IsUpdate = false;
                        state = ActionSimulateState.Exit;
                    }
                }
                else
                {
                    if (!s.IsUpdate)
                    {
                        s.IsUpdate = true;
                        state = ActionSimulateState.Start;
                    }
                }

                SimulateFrameData frameData = new SimulateFrameData
                {
                    FrameOffset = frameIndex - s.Action.StartFrame,
                    Length = s.Action.Length,
                    FrameTime = 1 / 30f,
                    IsSelected = isSelect,
                    State = state,
                };
                s.ActionSimulator.OnUpdate(this, s.Action, frameData);
                simulators[i] = s;
            }
            OnAfterSimulate(group, frameIndex);
        }

        protected void ReBuild(FrameActionGroup group)
        {
            List<Simulator> cache = new List<Simulator>();
            foreach (var a in group.Actions)
            {
                if (a.Data is not ISimulateable simulateable)
                    continue;
                int idx = simulators.FindIndex(it => it.GUID == a.GUID);
                if (idx >= 0)
                {
                    var s = simulators[idx];
                    if (s.ActionSimulator.GetType() == simulateable.GetSimulatorType())
                    {
                        s.Action = a;
                        cache.Add(s);
                        simulators.RemoveAt(idx);
                        continue;
                    }
                }
                var simulator = System.Activator.CreateInstance(simulateable.GetSimulatorType()) as IActionSimulator;
                simulator.OnCreate(this, a);
                var data = new Simulator
                {
                    GUID = a.GUID,
                    Action = a,
                    ActionSimulator = simulator,
                };
                cache.Add(data);
            }
            foreach (var s in simulators)
            {
                s.ActionSimulator.OnDispose(this, s.Action);
            }
            simulators.Clear();
            simulators.AddRange(cache);
        }

        protected virtual void OnBeforSimulate(FrameActionGroup group, int frameIndex) { }
        protected virtual void OnAfterSimulate(FrameActionGroup group, int frameIndex) { }
    }
}
