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
        public Toolbar TopToolbar { get; protected set; }
        public VisualElement EditorViewRoot { get; protected set; }
        public VisualElement InspectorView { get; protected set; }
        protected RadioButtonList groupListView;
        protected virtual string UXMLPath => null;
        public void CreateGUI()
        {
            if (!string.IsNullOrEmpty(UXMLPath))
            {
                var visualTree = Resources.Load<VisualTreeAsset>(UXMLPath);
                visualTree.CloneTree(rootVisualElement);
                TopToolbar = rootVisualElement.Q<Toolbar>("toolbar");
                EditorViewRoot = rootVisualElement.Q<VisualElement>("editorViewRoot");
            }
            OnCreateLayOut();
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
            if (EditorView == null)
            {
                currentAsset = asset;
                EditorView = CreateViewEditor(asset);
                EditorViewRoot?.Add(EditorView.RootView);
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(this, "switch asset");
                Undo.RegisterCompleteObjectUndo(EditorView, "switch asset");
                currentAsset = asset;
                EditorView.Asset = asset;
                EditorView.SwitchGroup(asset.Groups[0].GUID);
            }
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
        protected void OnUndoRedo()
        {
            if (currentAsset)
            {
                EditorView = CreateViewEditor(currentAsset);
            }
            if (EditorView)
            {
                EditorView.RefreshView();
            }
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
            FrameLineEditorCollector.instance.OnWindowDestroy(this);
        }

        protected virtual void OnCreateLayOut()
        {
            if (TopToolbar == null)
            {
                rootVisualElement.Add(TopToolbar = new Toolbar());
            }
            if (EditorViewRoot == null)
            {
                var splitTop = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);
                splitTop.Add(CreateListView());
                var split = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Horizontal);
                split.Add(EditorViewRoot = new VisualElement());
                split.Add(InspectorView = new VisualElement());
                splitTop.Add(split);
                rootVisualElement.Add(splitTop);
            }
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
