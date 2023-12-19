using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrameLine
{
    public static class FrameLineProcess
    {
        private static Dictionary<Type, IReadOnlyList<Type>> _actionTypes = new Dictionary<Type, IReadOnlyList<Type>>();
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
                                if (attribute != null)
                                {
                                    if (!attribute.AssetType.IsSubclassOf(typeof(FrameLineAsset)))
                                    {
                                        UnityEngine.Debug.LogError($"{type.Name} 的 CustomFrameLineProcess 类型错误，不是 FrameLineAsset 的子类");
                                    }
                                    else if (process.ContainsKey(attribute.AssetType))
                                    {
                                        process.Remove(attribute.AssetType);
                                    }
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
        public static IReadOnlyList<Type> GetActionTypes(FrameLineAsset asset)
        {
            var type = asset.GetType();
            if (!_actionTypes.TryGetValue(type, out var tys))
            {
                if (Process.TryGetValue(type, out var proc))
                {
                    var list = new List<Type>();
                    tys = list;
                    _actionTypes.Add(type, tys);
                    foreach (var assemble in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var t in assemble.GetTypes())
                        {
                            if (t.IsInterface || t.IsAbstract)
                                continue;
                            if (proc.CheckIsValidNodeType(t))
                            {
                                list.Add(t);
                            }
                        }
                    }
                }
            }
            return tys;
        }
    }
}
