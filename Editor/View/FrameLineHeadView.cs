using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    [System.Serializable]
    public class FrameLineHeadView
    {
        public bool OnGUI(FrameLineEditorView editorView, Vector2 size)
        {
            bool repaint = false;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(ViewStyles.FrameBarHeight), GUILayout.Width(size.x)))
            {
                DrawToolBar(editorView);
            }
            using (new GUI.ClipScope(new Rect(0, ViewStyles.FrameBarHeight, size.x, size.y - ViewStyles.FrameBarHeight)))
            {
                float frameHeight = editorView.Group.Actions.Count * (ViewStyles.ClipHeight + ViewStyles.ClipVInterval) + ViewStyles.ClipVInterval;
                Rect rect = new Rect(0, -editorView.ScrollPos.y, size.x, frameHeight);
                using (new GUILayout.AreaScope(rect))
                {
                    var e = Event.current;
                    if (rect.Contains(e.mousePosition))
                    {
                        repaint |= editorView.EventHandler.OnTrackHeadEvent(e);
                    }
                    FrameLineDrawer.DrawTrackHead(editorView, new Vector2(size.x, size.y - ViewStyles.FrameBarHeight));
                }
            }
            return repaint;
        }

        protected virtual void DrawToolBar(FrameLineEditorView editorView)
        {
            using (new EditorGUI.DisabledScope(editorView.Group == null || editorView.Group.FrameCount < 1))
            {
                using (new GUILayout.HorizontalScope(GUILayout.Height(ViewStyles.FrameBarHeight)))
                {
                    //第一帧
                    if (GUILayout.Button(BuiltInIcon.Instance.FirstKey, EditorStyles.toolbarButton))
                    {
                        editorView.SetFrameLocation(0);
                    }
                    //后退按钮
                    if (GUILayout.Button(BuiltInIcon.Instance.PrevKey, EditorStyles.toolbarButton))
                    {
                        editorView.SetFrameLocation(editorView.CurrentFrame - 1);
                    }
                    //播放暂停按钮
                    if (GUILayout.Button(editorView.IsPlaying ? BuiltInIcon.Instance.Pause : BuiltInIcon.Instance.Play, EditorStyles.toolbarButton))
                    {
                        editorView.SetPlayState(!editorView.IsPlaying);
                        editorView.FramePassTime = 0;
                    }
                    //前进按钮
                    if (GUILayout.Button(BuiltInIcon.Instance.NextKey, EditorStyles.toolbarButton))
                    {
                        editorView.SetFrameLocation(editorView.CurrentFrame + 1);
                        editorView.FramePassTime = 0;
                    }
                    //最后一帧
                    if (GUILayout.Button(BuiltInIcon.Instance.LastKey, EditorStyles.toolbarButton))
                    {
                        editorView.SetFrameLocation(editorView.Group.FrameCount);
                        editorView.FramePassTime = 0;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(BuiltInIcon.Instance.Add, EditorStyles.toolbarButton))
                    {
                        var types = FrameLineProcess.GetActionTypes(editorView.Asset);
                        if (types != null && types.Count > 0)
                        {
                            GenericMenu menu = new GenericMenu();
                            foreach (var t in types)
                            {
                                menu.AddItem(new GUIContent(FrameLineUtil.GetTypeShowName(t)), false, (createType) => 
                                {
                                    editorView.RegistUndo("create action");
                                    var action = FrameLineUtil.CreateAction(editorView.Group, (System.Type)createType, editorView.CurrentFrame, 1);
                                    editorView.SelectedActions.Clear();
                                    editorView.SelectedActions.Add(action.GUID);
                                    editorView.RebuildTrack();
                                    editorView.ScrollToFrame(editorView.CurrentFrame);
                                }, t);
                            }
                            menu.ShowAsContext();
                        }
                    }
                }
            }
        }
    }
}
