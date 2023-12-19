using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
namespace FrameLine
{
    public enum FrameActionHitPartType
    {
        None,
        Normal,
        LeftCtrl,
        RightCtrl,
    }
    public struct FrameActionHitResult
    {
        public Vector2 ClickPos;
        public FrameAction Action;
        public FrameActionHitPartType HitPart;
        public int Frame;
    }

    public class FrameLineEditorView : ScriptableObject
    {
        public FrameLineAsset Asset;
        public FrameLineGUIEvent EventHandler;
        public string GroupId;

        [System.NonSerialized]
        private FrameActionGroup _group;
        public FrameActionGroup Group 
        {
            get
            {
                if (_group == null || _group.GUID != GroupId)
                {
                    _group = Asset.FindGroup(GroupId);
                }
                return _group;
            }
        }
        public List<string> SelectedActions = new List<string>();
        public List<FrameLineTrack> Tracks = new List<FrameLineTrack>();

        public int CurrentFrame;
        public Vector2 ScrollPos;
        public bool IsPlaying { get; set; }
        public float FramePassTime { get; set; }
        public int FrameCount => Group.FrameCount;

        //滚动区域可见信息
        public int VisableFrameStart { get; private set; }
        public int VisableFrameEnd { get; private set; }
        public int VisableTrackStart { get; private set; }
        public int VisableTrackEnd { get; private set; }

        public FrameLineHeadView HeadView = new FrameLineHeadView();
        public FrameLineTrackView TrackView = new FrameLineTrackView();

        private TwoPaneSplitView _rootView;
        private IMGUIContainer _headView;
        private IMGUIContainer _trackView;
        public VisualElement RootView
        {
            get
            {
                if (_rootView == null)
                {
                    _rootView = new TwoPaneSplitView(0, ViewStyles.TrackHeadWidth, TwoPaneSplitViewOrientation.Horizontal);
                    _rootView.Add(_headView = new IMGUIContainer(DrawTrackHead));
                    _rootView.Add(_trackView = new IMGUIContainer(DrawTrack));
                }
                return _rootView;
            }
        }

        private void OnEnable()
        {
            if (EventHandler == null)
                EventHandler = new FrameLineGUIEvent(this);
        }

        protected virtual void OnDestroy()
        {
            Undo.ClearUndo(Asset);
            Undo.ClearUndo(this);
            if (EditorUtility.IsDirty(Asset))
            {
                FrameLineProcess.OnAssetSave(Asset);
                AssetDatabase.SaveAssetIfDirty(Asset);
            }
        }

        public bool IsSlecected(FrameAction action)
        {
            return SelectedActions.Contains(action.GUID);
        }
        public virtual void OnAddCreateMenue(GenericMenu menu) { }
        public virtual void RegistUndo(string name, bool needSave = true)
        {
            Undo.RegisterCompleteObjectUndo(Asset, name);
            if (needSave)
            {
                EditorUtility.SetDirty(Asset);
            }
            Undo.RegisterCompleteObjectUndo(this, name);
        }

        public virtual void OnFrameBarMenue(GenericMenu menu)
        {

        }

        public virtual void OnTrackHeadMenue(GenericMenu menu, FrameLineTrack track)
        {

        }

        public virtual void OnSelectActionMenue(GenericMenu menu)
        {

        }

        public void SwitchGroup(string groupId)
        {
            //此处不要加Undo，在操作切换的地方加
            CurrentFrame = 0;
            FramePassTime = 0;
            _group = Asset.FindGroup(groupId);
            GroupId = groupId;
            SelectedActions.Clear();
            FrameTrackUtil.RebuildTrack(this);
        }

        public FrameLineTrack OnAddAction(FrameAction action)
        {
            var track = GetTrack(action.Data.GetType().Name);
            if (track.Name == null)
            {
                track.Name = FrameLineUtil.GetTypeShowName(action.Data.GetType());
            }
            track.Add(action.GUID);
            return track;
        }
        public void OnRemoveAction(string actionID)
        {
            foreach (var track in Tracks)
            {
                if (track.Remove(actionID))
                    break;
            }
        }
        protected FrameLineTrack GetTrack(string typeGUID)
        {
            var track = Tracks.Find(it => it.TypeGUID == typeGUID);
            if (track == null)
            {
                track = new FrameLineTrack()
                {
                    TypeGUID = typeGUID,
                };
                Tracks.Add(track);
            }
            return track;
        }

        public void RefreshView()
        {
            _group = Asset.FindGroup(GroupId);
            FrameTrackUtil.RebuildTrack(this);
            _headView?.MarkDirtyRepaint();
            _trackView?.MarkDirtyRepaint();
        }

        public void ScrollToFrame(int frame)
        {
            bool left = VisableFrameStart >= frame;
            bool right = VisableFrameEnd < (frame + 2);
            if ( right || left)
            {
                int visableFrameCount = VisableFrameEnd - VisableFrameStart;
                if (right)
                {
                    ScrollPos.x = (frame - visableFrameCount + 2.5f) * ViewStyles.FrameWidth;
                }
                else if (left)
                {
                    ScrollPos.x = frame * ViewStyles.FrameWidth;
                }
            }
        }

        protected virtual void DrawTrackHead()
        {
            var size = _headView.layout.size;
            if (float.IsNaN(size.x))
                return;
            VisableFrameStart = Mathf.FloorToInt(ScrollPos.x / ViewStyles.FrameWidth);
            VisableTrackStart = Mathf.FloorToInt(ScrollPos.y / (ViewStyles.FrameWidth + ViewStyles.ClipVInterval));
            VisableTrackEnd = Mathf.CeilToInt((size.y + ScrollPos.y - VisableTrackStart * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval)) / (ViewStyles.ClipHeight + ViewStyles.ClipVInterval));
            using (new GUI.ClipScope(new Rect(Vector2.zero, size)))
            {
                if (HeadView.OnGUI(this, size))
                {
                    _headView.MarkDirtyRepaint();
                    _trackView.MarkDirtyRepaint();
                }
            }
        }

        protected virtual void DrawTrack()
        {
            var size = _trackView.layout.size;
            if (float.IsNaN(size.x))
                return;
            VisableFrameEnd = Mathf.CeilToInt((size.x + ScrollPos.x - VisableFrameStart * ViewStyles.FrameWidth) / ViewStyles.FrameWidth) + VisableFrameStart;
            using (new GUI.ClipScope(new Rect(Vector2.zero, size)))
            {
                if (TrackView.OnGUI(this, size))
                {
                    _headView.MarkDirtyRepaint();
                    _trackView.MarkDirtyRepaint();
                }
            }
        }

        public Vector2 InspectorActionGUIPos;
        public Vector2 InspectorBaseGUIPos;
        public int InspectorSelectTable;
        private static string[] TabelNames = new string[] { "Action", "基础" };
        public void OnInspectorGUI()
        {
            InspectorSelectTable = GUILayout.Toolbar(InspectorSelectTable, TabelNames);
            if (InspectorSelectTable == 0 && SelectedActions.Count > 0)
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(InspectorActionGUIPos))
                {
                    InspectorActionGUIPos = scroll.scrollPosition;
                    using (new GUILayout.VerticalScope())
                    {
                        if (SelectedActions.Count > 0)
                        {
                            //foreach (var action in SelectedActions)
                            //{
                            //    action.Action.Editor.OnInspectorGUI(Asset, OnActionContextMenue);
                            //}
                        }
                    }
                }
            }
            else
            {
                //using (var scroll = new EditorGUILayout.ScrollViewScope(InspectorBaseGUIPos))
                //{
                //    InspectorBaseGUIPos = scroll.scrollPosition;
                //    using (new GUILayout.VerticalScope())
                //    {
                //        Editor?.Draw();
                //        if (Group != null)
                //        {
                //            GUILayout.Space(20);
                //            GUILayout.Label("当前组：", EditorStyles.boldLabel);
                //            Group.Drawer?.Draw(null, Group, Asset);
                //        }
                //    }
                //}
            }
        }

        protected virtual void OnActionContextMenue(GenericMenu menu, FrameAction action)
        {
            if (FrameLineClipboard.instance.PastePropertyCheck(action))
            {
                menu.AddItem(new GUIContent("粘贴属性"), false, () => { RegistUndo("Paste Property"); FrameLineClipboard.instance.PasteActionProperty(action); });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(("粘贴属性")));
            }
        }

        protected void Simulate()
        {
            //if (Group == null)
            //    return;
            //if (IsPlaying)
            //{
            //    FramePassTime += Window.DeltaTime;
            //    if (FramePassTime > FrameLineEditorWindow.FrameTime)
            //    {
            //        FramePassTime %= FrameLineEditorWindow.FrameTime;
            //        CurrentFrame++;
            //        if (CurrentFrame >= Group.FrameCount)
            //        {
            //            CurrentFrame = 0;
            //            if (IsPlaying)
            //                OnGroupSimulateEnd();
            //        }
            //    }
            //}
            //if (Window.EnableSimulate)
            //{
            //    var s = FrameLineEditorContext.instance.GetSimulate(this, Asset);
            //    s.Simulate(Group, CurrentFrame);
            //}
            //else
            //{
            //    FrameLineEditorContext.instance.ClearSimulateByGUI(this);
            //}
        }

        //预览播放状态，当前组播放完毕，主要用于自动播放下一个动画实现连贯播放
        protected virtual void OnGroupSimulateEnd() { }

        public virtual void OnSceneGUI(SceneView sceneView)
        {
            //if (!Window.EnableSimulate)
            //    return;
            //var s = FrameLineEditorContext.instance.FindSimulate(this, Asset);
            //if (!s)
            //    return;
            //if (Window.ShowBone)
            //{
            //    s.DrawBone();
            //}
            //if (Window.ShowSceneGizmos)
            //{
            //    s.OnSceneGUI(Group, CurrentFrame);
            //}
        }
    }
}