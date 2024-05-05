using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    internal class FrameLineEditorCollector : ScriptableSingleton<FrameLineEditorCollector>
    {
        [System.Serializable]
        public class Unit
        {
            public FrameLineAsset Asset;
            public FrameLineEditorView View;
            public EditorWindow Window;
            public FrameLineSimulator Simulator;
        }
        [SerializeField]
        private List<Unit> units = new List<Unit>();

        public FrameLineEditorView CreateEditor(FrameLineAsset asset, EditorWindow window)
        {
            var u = units.Find(it => it.Asset == asset && it.Window == window);
            var viewType = FrameLineProcess.GetEditorViewType(asset);
            if (u!= null && u.View.GetType() != viewType)
            {
                Undo.ClearUndo(u.View);
                DestroyImmediate(u.View);
                units.Remove(u);
                u = null;
            }
            if (u == null)
            {
                u = new Unit { Asset = asset, Window = window };
                u.View = CreateInstance(viewType) as FrameLineEditorView;
                u.View.Asset = asset;
                u.View.hideFlags = HideFlags.HideAndDontSave;
                if (asset.Groups.Count > 0)
                {
                    u.View.SwitchGroup(asset.Groups[0].GUID);
                }
                units.Add(u);
                u.View.OnInit();
            }
            return u.View;
        }

        public void UpdateSimulator(EditorWindow window, FrameLineEditorView view, bool enable)
        {
            foreach (var u in units)
            {
                if (u.Window == window)
                {
                    if (u.View != view || !enable)
                    {
                        if (u.Simulator)
                        {
                            DestroyImmediate(u.Simulator);
                        }
                    }
                    else if (u.View == view && enable)
                    {
                        if (u.Simulator == null)
                        {
                            u.Simulator = FrameLineSimulator.CreateSimulate(u.Asset, null);
                        }
                    }
                }
            }
        }

        public FrameLineSimulator FindSimulate(FrameLineEditorView view)
        {
            var u = units.Find(it => it.View == view);
            return u?.Simulator;
        }

        public void OnWindowDestroy(EditorWindow window)
        {
            Undo.ClearUndo(window);
            for (int i=0; i<units.Count; ++i)
            {
                var u = units[i];
                if (u.Window == window)
                {
                    if (u.Simulator)
                    {
                        DestroyImmediate(u.Simulator);
                    }
                    if (u.View)
                    {
                        Undo.ClearUndo(u.View);
                        DestroyImmediate(u.View);
                    }
                    if (u.Asset)
                    {
                        Undo.ClearUndo(u.Asset);
                        if (EditorUtility.IsDirty(u.Asset))
                        {
                            AssetDatabase.SaveAssetIfDirty(u.Asset);
                            FrameLineProcess.OnAssetSave(u.Asset);
                        }
                    }
                    units.RemoveAt(i);
                    --i;
                }
            }
        }

        public void OnSaveAll(EditorWindow window)
        {
            for (int i = 0; i < units.Count; ++i)
            {
                var u = units[i];
                if (u.Window == window)
                {
                    if (u.Asset)
                    {
                        if (EditorUtility.IsDirty(u.Asset))
                        {
                            AssetDatabase.SaveAssetIfDirty(u.Asset);
                            FrameLineProcess.OnAssetSave(u.Asset);
                        }
                    }
                }
            }
        }
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            foreach (var u in units)
            {
                if (u.Simulator)
                {
                    DestroyImmediate(u.Simulator);
                    u.Simulator = null;
                }
            }
        }
    }
}
