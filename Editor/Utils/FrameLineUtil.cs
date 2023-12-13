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
    }
}
