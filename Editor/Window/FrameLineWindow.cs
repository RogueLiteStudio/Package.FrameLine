using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FrameLine
{
    public class FrameLineWindow : EditorWindow
    {
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        internal static bool OnGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as FrameLineAsset;
            return asset != null && OpenAsset(asset);
        }

        public static bool OpenAsset(FrameLineAsset asset)
        {
            var windowType = FrameLineProcess.GetEditorWindowType(asset);
            if (windowType != null)
            {
                var window = GetWindow(windowType, false, null) as FrameLineWindow;
                window.OnOpenAsset(asset);
                return true;
            }
            return false;
        }

        public FrameLineEditorView EditorView;
        public Toolbar TopToolbar { get; protected set; }
        public VisualElement EditorViewRoot { get; protected set; }
        public VisualElement InspectorView { get; protected set; }
        protected virtual string UXMLPath => null;
        public void CreateGUI()
        {
            if (!string.IsNullOrEmpty(UXMLPath))
            {
                var visualTree = UnityEngine.Resources.Load<VisualTreeAsset>(UXMLPath);
                visualTree.CloneTree(rootVisualElement);
                TopToolbar = rootVisualElement.Q<Toolbar>("toolbar");
                EditorViewRoot = rootVisualElement.Q<VisualElement>("editorViewRoot");
            }
            OnCreateLayOut();
            if (EditorView)
            {
                EditorViewRoot.Add(EditorView.RootView);
            }
        }

        protected virtual FrameLineEditorView CreateViewEditor(FrameLineAsset asset)
        {
            FrameLineEditorView editorView = CreateInstance<FrameLineEditorView>();
            editorView.Asset = asset;
            editorView.SwitchGroup(asset.Groups[0].GUID);
            editorView.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            return editorView;
        }

        public virtual void OnOpenAsset(FrameLineAsset asset)
        {
            if (EditorView == null)
            {
                EditorView = CreateViewEditor(asset);
                EditorViewRoot?.Add(EditorView.RootView);
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(this, "switch asset");
                Undo.RegisterCompleteObjectUndo(EditorView, "switch asset");
                EditorView.Asset = asset;
                EditorView.SwitchGroup(asset.Groups[0].GUID);
            }
        }

        protected void OnDestroy()
        {
            if (EditorView != null)
            {
                DestroyImmediate(EditorView);
                EditorView = null;
            }
        }

        protected virtual void OnCreateLayOut()
        {
            if (TopToolbar == null)
            {
                rootVisualElement.Add(TopToolbar = new Toolbar());
            }
            if (EditorViewRoot == null)
            {
                var split = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Horizontal);
                rootVisualElement.Add(split);
                split.Add(EditorViewRoot = new VisualElement());
                split.Add(InspectorView = new VisualElement());
            }
        }
    }
}
