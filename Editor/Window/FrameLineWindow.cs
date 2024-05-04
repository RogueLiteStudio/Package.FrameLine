using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElementExtern;

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
        [SerializeField]
        protected FrameLineAsset currentAsset;
        public FrameLineEditorView EditorView;
        protected FrameLineSimulator currentSimulate;
        public Toolbar TopToolbar { get; protected set; }
        public Toggle PreviewToggle { get; protected set; }
        public VisualElement EditorViewRoot { get; protected set; }
        public IMGUIContainer InspectorView { get; protected set; }
        protected RadioButtonList groupListView;
        public void CreateGUI()
        {
            rootVisualElement.Add(TopToolbar = new Toolbar());
            var splitTop = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);
            splitTop.Add(CreateListView());
            var split = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Horizontal);
            split.Add(EditorViewRoot = new VisualElement());
            split.Add(InspectorView = new IMGUIContainer(DrawInspector));
            splitTop.Add(split);
            rootVisualElement.Add(splitTop);
            OnCreateLayOut();
            TopToolbar.Add(PreviewToggle = new Toggle("预览"));
            PreviewToggle.RegisterValueChangedCallback((evt)=> RefreshPreview());
            PreviewToggle.labelElement.style.minWidth = 0;
            if (EditorView && EditorView.Simulator)
            {
                PreviewToggle.SetValueWithoutNotify(true);
            }
            if (currentAsset)
            {
                RefreshGroupList();
            }
            if (EditorView)
            {
                EditorViewRoot.Add(EditorView.RootView);
            }
        }

        protected virtual FrameLineEditorView CreateViewEditor(FrameLineAsset asset)
        {
            return FrameLineEditorCollector.instance.CreateEditor(asset, this);
        }

        public virtual void OnOpenAsset(FrameLineAsset asset)
        {
            if (currentAsset == asset)
                return;
            if (EditorView)
            {
                if (EditorView.Simulator)
                {
                    DestroyImmediate(EditorView.Simulator);
                    EditorView.Simulator = null;
                }
                EditorView.RootView.RemoveFromHierarchy();
            }
            if (EditorView)
            {
                Undo.RegisterCompleteObjectUndo(this, "switch asset");
            }
            currentAsset = asset;
            EditorView = CreateViewEditor(asset);
            EditorViewRoot?.Add(EditorView.RootView);
            EditorView.SwitchGroup(asset.Groups[0].GUID);
            RefreshGroupList();
        }

        protected virtual void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        protected virtual void RefreshPreview()
        {
            if (!EditorView || PreviewToggle == null)
                return;
            bool hasPreview = EditorView.Simulator != null;
            if (hasPreview != PreviewToggle.value)
            {
                if (EditorView.Simulator)
                {
                    DestroyImmediate(EditorView.Simulator);
                    currentSimulate = null;
                }
                else
                {
                    EditorView.Simulator = FrameLineSimulator.CreateSimulate(currentAsset, null);
                    EditorView.SetFrameLocation(EditorView.CurrentFrame);
                    currentSimulate = EditorView.Simulator;
                }
            }
        }

        protected void OnUndoRedo()
        {
            if (currentAsset)
            {
                EditorView = CreateViewEditor(currentAsset);
            }
            if (currentSimulate)
            {
                if (!EditorView || EditorView.Simulator != currentSimulate)
                {
                    DestroyImmediate(currentSimulate);
                }
            }
            if (EditorView)
            {
                var first = EditorViewRoot.Children().FirstOrDefault();
                if (first.userData != EditorView.RootView.userData)
                {
                    first.RemoveFromHierarchy();
                    EditorViewRoot.Add(EditorView.RootView);
                }
                EditorView.RefreshView();
            }
            RefreshPreview();
            RefreshGroupList();
            OnRefresh();
        }

        protected virtual void OnRefresh()
        {
        }
        protected void RefreshGroupList()
        {
            groupListView.Refresh(currentAsset.Groups, (group) => group.Name, (group) => group.GUID, EditorView.GroupId);
        }
        protected void OnDestroy()
        {
            if (EditorView && EditorView.Simulator)
            {
                DestroyImmediate(EditorView.Simulator);
            }
            FrameLineEditorCollector.instance.OnWindowDestroy(this);
        }

        protected virtual void OnCreateLayOut()
        {
        }

        private void DrawInspector()
        {
            EditorView?.OnInspectorGUI();
        }

        protected VisualElement CreateListView()
        {
            VisualElement content = new VisualElement();
            ScrollView scrollView = new ScrollView();
            content.Add(scrollView);
            scrollView.Add(groupListView = new RadioButtonList());
            groupListView.OnSelect = (key) =>
            {
                EditorView.RegistUndo("switch action");
                EditorView.SwitchGroup(key);
            };
            return content;
        }
    }
}
