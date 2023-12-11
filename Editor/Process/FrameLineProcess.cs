using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrameLine
{
    public static class FrameLineProcess
    {
        private static Dictionary<Type, IFrameLineProcess> process;
        public static Dictionary<Type, IFrameLineProcess> Process
        {
            get
            {
                if (process == null)
                {
                    process = new Dictionary<Type, IFrameLineProcess>();
                    foreach (var assemble in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var type in assemble.GetTypes())
                        {
                            if (type.IsInterface || type.IsAbstract)
                                continue;
                            if (typeof(IFrameLineProcess).IsAssignableFrom(type))
                            {
                                var attribute = type.GetCustomAttribute<CustomFrameLineProcessAttribute>(false);
                                if (process.ContainsKey(attribute.AssetType))
                                {
                                    process.Remove(attribute.AssetType);
                                }
                                process.Add(attribute.AssetType, Activator.CreateInstance(type) as IFrameLineProcess);
                            }
                        }
                    }
                }
                return process;
            }
        }

        public static Type GetEditorWindowType(FrameLineAsset asset)
        {
            var type = asset.GetType();
            if (Process.TryGetValue(type, out var proc))
            {
                return proc.EditorWindowType;
            }
            return null;
        }
        public static FrameLineAsset OnAssetCreateAction(Type assetType)
        {
            if (Process.TryGetValue(assetType, out var proc))
            {
                return proc.OnCreateAction();
            }
            return null;
        }
        public static void OnAssetSave(FrameLineAsset asset)
        {
            var type = asset.GetType();
            if (Process.TryGetValue(type, out var proc))
            {
                proc.OnSave(asset);
            }
        }
    }
}
