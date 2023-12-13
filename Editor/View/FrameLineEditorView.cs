using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
    public class BuiltInIcon
    {
        public GUIContent Play;
        public GUIContent Pause;
        public GUIContent PrevKey;
        public GUIContent NextKey;
        private BuiltInIcon()
        {
            Play = EditorGUIUtility.IconContent("Animation.Play");
            Pause = EditorGUIUtility.IconContent("PauseButton");
            PrevKey = EditorGUIUtility.IconContent("Animation.PrevKey");
            NextKey = EditorGUIUtility.IconContent("Animation.NextKey");
        }

        private static BuiltInIcon _instance;
        public static BuiltInIcon Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuiltInIcon();
                }
                return _instance;
            }
        }
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

        protected virtual void DrawToolBar()
        {
            using(new EditorGUI.DisabledScope(Group == null || Group.FrameCount < 1))
            {
                using (new GUILayout.HorizontalScope())
                {
                    //后退按钮
                    if (GUILayout.Button(BuiltInIcon.Instance.PrevKey, EditorStyles.toolbarButton))
                    {
                        CurrentFrame = Mathf.Max(0, CurrentFrame - 1);
                        FramePassTime = 0;
                    }
                    //播放暂停按钮
                    if (GUILayout.Button(IsPlaying ? BuiltInIcon.Instance.Pause : BuiltInIcon.Instance.Play, EditorStyles.toolbarButton))
                    {
                        IsPlaying = !IsPlaying;
                        FramePassTime = 0;
                    }
                    //前进按钮
                    if (GUILayout.Button(BuiltInIcon.Instance.NextKey, EditorStyles.toolbarButton))
                    {
                        CurrentFrame = Mathf.Min(Group.FrameCount, CurrentFrame + 1);
                        FramePassTime = 0;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+"))
                    {
                        GenericMenu menu = new GenericMenu();
                        OnAddCreateMenue(menu);
                        menu.ShowAsContext();
                    }
                }
            }
        }
        protected virtual void OnAddCreateMenue(GenericMenu menu) { }
        public virtual void RegistUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(Asset, name);
            EditorUtility.SetDirty(Asset);

            Undo.RegisterCompleteObjectUndo(this, name);
            //Undo.RegisterCompleteObjectUndo(Window, name);
        }

        public virtual void OnFrameBarMenue(GenericMenu menu)
        {

        }

        public virtual void OnTrackHeadMenue(GenericMenu menu)
        {

        }

        public virtual void OnSelectActionMenue(GenericMenu menu)
        {

        }

        public void SwitchGroup(string groupId)
        {
            CurrentFrame = 0;
            FramePassTime = 0;
            _group = Asset.FindGroup(groupId);
            GroupId = groupId;
            SelectedActions.Clear();
            FrameTrackUtil.RebuildTrack(this);
        }

        //public FrameAction AddAction<T>() where T : IFrameEvent, new()
        //{
        //    RegistUndo("创建");
        //    T data = new T();
        //    var action = Asset.AddAction(GroupId, CurrentFrame, data);
        //    OnAddAction(action);
        //    return action;
        //}

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

        public bool OnDraw(Vector2 size)
        {
            if (Group == null)
                return false;

            int showTrackCount = Group.Actions.Count;
            float frameHeight = showTrackCount * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval) + ViewStyles.ClipVInterval;
            float framWidth = Group.FrameCount * ViewStyles.FrameWidth + 10;

            //滚动位置
            float xOffset = ScrollPos.x * framWidth;
            float yOffset = ScrollPos.y * frameHeight;
            VisableFrameStart = Mathf.FloorToInt(xOffset / ViewStyles.FrameWidth);
            VisableTrackStart = Mathf.FloorToInt(yOffset / (ViewStyles.FrameWidth + ViewStyles.ClipVInterval));

            //轨道头部
            Rect trackHeadRect = new Rect(0, 0, ViewStyles.TrackHeadWidth, size.y - ViewStyles.ScrollBarSize);
            bool rePaint = false;
            using (new GUILayout.AreaScope(trackHeadRect))
            {
                //轨道头部按钮区域
                using (new GUILayout.AreaScope(new Rect(0, 0, trackHeadRect.width, ViewStyles.FrameBarHeight), "", EditorStyles.toolbar))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        DrawToolBar();
                    }
                }
                Rect rect = new Rect(0, ViewStyles.FrameBarHeight, trackHeadRect.width, trackHeadRect.height - ViewStyles.FrameBarHeight);
                VisableTrackEnd = Mathf.CeilToInt((rect.height + yOffset - VisableTrackStart * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval)) / (ViewStyles.ClipHeight + ViewStyles.ClipVInterval));
                //轨道头部
                using (new GUI.ClipScope(rect))
                {
                    //滚动位置
                    Rect viewRect = new Rect(0, -yOffset, trackHeadRect.width, frameHeight);
                    using (new GUILayout.AreaScope(viewRect))
                    {
                        var e = Event.current;
                        if (viewRect.Contains(e.mousePosition))
                        {
                            rePaint |= EventHandler.OnTrackHeadEvent(e);
                        }
                        FrameLineDrawer.DrawTrackHead(this);
                    }
                }
            }
            //轨道区域大小
            Vector2 trackAreaSize = new Vector2(framWidth, frameHeight);
            //轨道区域
            Rect frameRect = new Rect(ViewStyles.TrackHeadWidth, 0, size.x - ViewStyles.TrackHeadWidth - ViewStyles.ScrollBarSize, size.y - ViewStyles.ScrollBarSize);
            //轨道在窗口中显示的大小
            Vector2 trackAreaInViewSize = new Vector2(frameRect.width, frameRect.height - ViewStyles.FrameBarHeight);
            using (new GUI.ClipScope(frameRect))
            {
                VisableFrameEnd = Mathf.CeilToInt((frameRect.width + xOffset - VisableFrameStart * ViewStyles.FrameWidth) / ViewStyles.FrameWidth) + VisableFrameStart;
                //画帧标号背景条
                GUI.Box(new Rect(0, 0, frameRect.width, ViewStyles.FrameBarHeight), "");
                //帧长度区域|<-所有帧->|，水平滚动区域
                using (new GUILayout.AreaScope(new Rect(-xOffset, 0, framWidth, frameRect.height)))
                {
                    FrameLineDrawer.DrawFrameLineBackGround(this, new Rect(new Vector2(xOffset, 0), frameRect.size));
                    {
                        var e = Event.current;
                        if (e.mousePosition.y < ViewStyles.FrameBarHeight || EventHandler.IsDragFrameBar)
                        {
                            rePaint |= EventHandler.OnFrameBarEvent(e);
                        }
                    }
                    float viewHeight = Mathf.Max(frameHeight, frameRect.height);
                    //轨道条区域
                    using (new GUI.ClipScope(new Rect(0, ViewStyles.FrameBarHeight, framWidth, viewHeight)))
                    {
                        Rect trackViewRect = new Rect(0, -yOffset, framWidth, viewHeight);
                        using (new GUILayout.AreaScope(trackViewRect))
                        {
                            var e = Event.current;
                            Vector2 mousePos = e.mousePosition;
                            bool mouseInView = trackViewRect.Contains(mousePos);
                            if (mouseInView)
                            {
                                if (EventHandler.OnFrameClipsEvent(e))
                                {
                                    e.Use();
                                    rePaint = true;
                                }
                            }
                            FrameLineDrawer.DrawFrameActions(this, mouseInView, mousePos);
                        }
                    }
                }
            }
            //滚动条
            {
                Rect vBarRect = new Rect(size.x - ViewStyles.ScrollBarSize, 0, ViewStyles.ScrollBarSize, size.y - ViewStyles.ScrollBarSize);
                float vSize = Mathf.Clamp01(trackAreaInViewSize.y / trackAreaSize.y);
                ScrollPos.y = GUI.VerticalScrollbar(vBarRect, ScrollPos.y, vSize, 0, 1);
                Rect hBarRect = new Rect(ViewStyles.TrackHeadWidth, size.y - ViewStyles.ScrollBarSize, size.x - ViewStyles.TrackHeadWidth - ViewStyles.ScrollBarSize, ViewStyles.ScrollBarSize);
                float hSize = Mathf.Clamp01(trackAreaInViewSize.x / trackAreaSize.x);
                ScrollPos.x = GUI.HorizontalScrollbar(hBarRect, ScrollPos.x, hSize, 0, 1);
            }
            Simulate();
            return rePaint;
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