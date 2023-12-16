using System.Reflection;
using UnityEngine;

namespace FrameLine
{
    public static class FrameLineUtil
    {
        public static int PosToFrame(float pos)
        {
            return Mathf.Max(0, Mathf.FloorToInt(pos / ViewStyles.FrameWidth));
        }

        public static string GetTypeShowName(System.Type type)
        {
            var disPlay = type.GetCustomAttribute<DisplayNameAttribute>();
            if (disPlay != null)
                return disPlay.Name;
            return type.Name;
        }

        public static FrameActionGroup CreateGroup(FrameLineAsset asset)
        {
            FrameActionGroup group = new FrameActionGroup()
            {
                GUID = System.Guid.NewGuid().ToString(),
            };
            asset.Groups.Add(group);
            return group;
        }

        public static FrameAction CreateAction(FrameActionGroup group, System.Type type, int startFrame, int length)
        {
            var action = new FrameAction
            {
                GUID = System.Guid.NewGuid().ToString(),
                StartFrame = startFrame,
                Length = length,
                Enable = true,
                Name = GetTypeShowName(type),
            };
            action.SetData(System.Activator.CreateInstance(type) as IFrameAction);
            group.Actions.Add(action);
            return action;
        }
    }
}
