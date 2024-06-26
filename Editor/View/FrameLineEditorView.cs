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
        public Vector2 LocalPos;
        public int Frame;
    }

    public class FrameLineEditorView : ScriptableObject, IPropertyEditorContext
    {
        public const float FrameTime = 1 / 30f;
        public FrameLineAsset Asset;
        public FrameLineGUIEvent EventHandler;
        public string GroupId;

        public FrameLineSimulator Simulator;
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
        public float FrameWidth => ViewStyles.FrameWidth;
        public float ClipHeight=> ViewStyles.ClipHeight;
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

        protected IVisualElementScheduledItem playingTimer;
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
                    _trackView.RegisterCallback<MouseMoveEvent>((evt) => _trackView.MarkDirtyRepaint());
                    playingTimer = _rootView.schedule.Execute(OnTick).Every(10);
                    playingTimer.Pause();

                    _rootView.userData = Asset;
                }
                return _rootView;
            }
        }
        private bool hasDelayCall;

        public void DelayRefreshPreView()
        {
            if (hasDelayCall || IsPlaying)
                return;
            hasDelayCall = true;
            EditorApplication.delayCall += () =>
            {
                hasDelayCall = false;
                SetFrameLocation(CurrentFrame);
            };
        }

        public virtual void OnInit()
        {
        }

        protected virtual void OnEnable()
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
        public virtual void RegisterUndo(string name, bool needSave = true)
        {
            Undo.RegisterCompleteObjectUndo(Asset, name);
            if (needSave)
            {
                EditorUtility.SetDirty(Asset);
            }
            Undo.RegisterCompleteObjectUndo(this, name);
            DelayRefreshPreView();
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
            var track = GetTrack(action.Data.GetType());
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
        protected FrameLineTrack GetTrack(System.Type type)
        {
            var track = Tracks.Find(it => it.TypeGUID == type.Name);
            if (track == null)
            {
                track = new FrameLineTrack()
                {
                    TypeGUID = type.Name,
                    TypeColor = ViewStyles.GetActionColor(type),
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

            var simulator = FrameLineEditorCollector.instance.FindSimulate(this);
            if (simulator)
            {
                simulator.MarkReBuild();
                DelayRefreshPreView();
            }
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
            _headView?.MarkDirtyRepaint();
            _trackView?.MarkDirtyRepaint();
        }

        protected virtual void DrawTrackHead()
        {
            var size = _headView.layout.size;
            if (float.IsNaN(size.x) || Group == null)
                return;
            VisableFrameStart = Mathf.FloorToInt(ScrollPos.x / ViewStyles.FrameWidth);
            VisableTrackStart = Mathf.FloorToInt(ScrollPos.y / (ViewStyles.FrameWidth + ViewStyles.ClipVInterval));
            VisableTrackEnd = Mathf.CeilToInt((size.y + ScrollPos.y - VisableTrackStart * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval)) / (ViewStyles.ClipHeight + ViewStyles.ClipVInterval));
            using (new GUI.ClipScope(new Rect(Vector2.zero, size)))
            {
                if (HeadView.OnGUI(this, size) || Event.current.type == EventType.Used)
                {
                    _headView.MarkDirtyRepaint();
                    _trackView.MarkDirtyRepaint();
                }
            }
        }

        protected virtual void DrawTrack()
        {
            var size = _trackView.layout.size;
            if (float.IsNaN(size.x) || Group == null)
                return;
            VisableFrameEnd = Mathf.CeilToInt((size.x + ScrollPos.x - VisableFrameStart * ViewStyles.FrameWidth) / ViewStyles.FrameWidth) + VisableFrameStart;
            using (new GUI.ClipScope(new Rect(Vector2.zero, size)))
            {
                if (TrackView.OnGUI(this, size) || Event.current.type == EventType.Used)
                {
                    _headView.MarkDirtyRepaint();
                    _trackView.MarkDirtyRepaint();
                }
            }
        }

        public Vector2 InspectorActionGUIPos;
        public Vector2 InspectorBaseGUIPos;
        public int InspectorSelectTable;
        private static string[] TabelNames = new string[] { "Action", "当前组", "资源" };
        private Dictionary<string, FrameActionInspector> inspectors = new Dictionary<string, FrameActionInspector>();
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
                        foreach (var kv in inspectors)
                        {
                            if (kv.Value.IsFocus && SelectedActions.Contains(kv.Key))
                            {
                                kv.Value.IsFocus = false;
                                kv.Value.OnFocusChanged(kv.Value.IsFocus);
                            }
                        }
                        if (SelectedActions.Count > 0)
                        {
                            foreach (var action in SelectedActions)
                            {
                                var a = Group.Find(action);
                                if (!inspectors.TryGetValue(action, out var inspector))
                                {
                                    inspector = FrameActionInspector.CreateInspector(this, a);
                                    inspectors[action] = inspector;
                                }
                                inspector.Action = a;
                                if (!inspector.IsFocus)
                                {
                                    inspector.IsFocus = true;
                                    inspector.OnFocusChanged(inspector.IsFocus);
                                }
                                inspector.OnInsperctorGUI();
                            }
                        }
                    }
                }
            }
            else
            {
                if (InspectorSelectTable == 2 || Group == null)
                {
                    LayoutGUI("描述", ref Asset.Comment, EditorGUILayout.TextArea);
                    DrawAssetInspector();
                }
                else
                {
                    using (var scroll = new EditorGUILayout.ScrollViewScope(InspectorBaseGUIPos))
                    {
                        InspectorBaseGUIPos = scroll.scrollPosition;
                        using (new GUILayout.VerticalScope())
                        {
                            LayoutGUI("组名", ref Group.Name, EditorGUILayout.TextField);
                            LayoutGUI("描述", ref Group.Description, EditorGUILayout.TextArea);
                            using (new EditorGUI.DisabledScope(Group.Disable))
                            {
                                GUILayout.Label($"帧数: {Group.FrameCount}");
                                DrawGroupInspector();
                            }
                        }
                    }
                }
            }
        }

        public void SetPlayState(bool playing)
        {
            if (IsPlaying == playing)
                return;
            IsPlaying = playing;
            FramePassTime = 0;
            if (IsPlaying)
                playingTimer.Resume();
            else
                playingTimer.Pause();
        }

        protected void OnTick(TimerState timerState)
        {
            FramePassTime += (timerState.deltaTime * 0.001f);
            if (FramePassTime > FrameTime)
            {
                FramePassTime %= FrameTime;
                SetFrameLocation(CurrentFrame + 1);
            }
        }

        public void SetFrameLocation(int frame)
        {
            if (frame >= Group.FrameCount)
            {
                CurrentFrame = 0;
                if (IsPlaying)
                {
                    OnGroupSimulateEnd();
                }
            }
            else if (frame < 0)
            {
                CurrentFrame = 0;
            }
            else
            {
                CurrentFrame = frame;
            }
            ScrollToFrame(CurrentFrame);
            var simulator = FrameLineEditorCollector.instance.FindSimulate(this);
            if (simulator)
            {
                simulator.Simulate(Group, CurrentFrame, SelectedActions);
            }
            SceneView.RepaintAll();
        }

        //预览播放状态，当前组播放完毕，主要用于自动播放下一个动画实现连贯播放
        protected virtual void OnGroupSimulateEnd() { }

        public virtual void OnSceneGUI(SceneView sceneView)
        {
            foreach (var action in Group.Actions)
            {
                if (action.Data is IGizmosable gizmosable)
                {
                    int endFrame = FrameActionUtil.GetActionEndFrame(Group, action);
                    gizmosable.DrawGizmos(this, IsSlecected(action), action.StartFrame, endFrame, CurrentFrame);
                }
            }
            var simulator = FrameLineEditorCollector.instance.FindSimulate(this);
            if (simulator)
            {
                simulator.OnSceneGUI(Group, CurrentFrame, SelectedActions);
            }
        }

        public void OnPropertyModify()
        {
            RegisterUndo("action property modif");
        }
        protected virtual void DrawGroupInspector()
        {
        }

        protected virtual void DrawAssetInspector()
        {
        }
        protected bool LayoutGUI<T>(string label, ref T v, System.Func<T, GUILayoutOption[], T> func, params GUILayoutOption[] options)
        {
            using(new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label);
                EditorGUI.BeginChangeCheck();
                var newVal = func(v, options);
                if (EditorGUI.EndChangeCheck())
                {
                    OnPropertyModify();
                    v = newVal;
                    return true;
                }
            }
            return false;
        }
    }

    public class TFrameLineEditorView<T> : FrameLineEditorView where T : FrameLineAsset
    {
        public T TAsset=> Asset as T;
    }
}