using CodeGen;
using PropertyEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FrameLine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomActionEditorAttribute : Attribute
    {
        public Type Type { get; private set; }
        public CustomActionEditorAttribute(Type type)
        {
            Type = type;
        }
    }

    public class FrameActionInspector
    {
        protected FrameLineEditorView EditorView { get; private set; }
        public FrameAction Action { get; internal set; }
        protected IDrawer drawer;
        public bool IsFocus { get; internal set; }
        private bool foldout = true;
        private string typeName;
        private string tooltip;
        public static GUIStyle backGrountStyle = new GUIStyle("OL box NoExpand") { padding = new RectOffset(1, 0, 0, 0) };
        public static GUIStyle toolbar = new GUIStyle("IN Title") { padding = new RectOffset(0, 0, 2, 0) };

        public virtual void OnCreate()
        {
            Type type = Action.Data.GetType();
            drawer = DrawerCollector.CreateDrawer(type);
            var dpName = type.GetCustomAttribute<DisplayNameAttribute>();
            typeName = dpName != null ? dpName.Name : type.Name;
            if (dpName != null)
                tooltip = dpName.ToolTip;
        }

        public virtual void OnFocusChanged(bool focus)
        {
        }

        public virtual void OnDestroy()
        {
        }

        public void OnInsperctorGUI()
        {
            using (new GUILayout.VerticalScope(backGrountStyle))
            {
                using (new GUILayout.HorizontalScope(toolbar))
                {
                    EditorGUI.BeginChangeCheck();
                    bool enable = EditorGUILayout.Toggle(Action.Enable, GUILayout.Width(20));
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorView.RegistUndo("action enable modify");
                        Action.Enable = enable;
                    }
                    if (GUILayout.Button("", "PaneOptions"))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("复制属性"), false, () => FrameLineClipboard.instance.CopyActionData(Action));
                        if (FrameLineClipboard.instance.PastePropertyCheck(Action))
                        {
                            menu.AddItem(new GUIContent("粘贴属性"), false, () => 
                            {
                                EditorView.RegistUndo("action paste");
                                FrameLineClipboard.instance.PasteActionProperty(Action);
                            });
                        }
                        OnNodeMenu(menu);
                        menu.ShowAsContext();
                    }
                    GUILayout.Space(5);
                    foldout = EditorGUILayout.Foldout(foldout, new GUIContent($"{typeName} - {Action.Name}", tooltip), true);
                    if (!foldout)
                        return;
                }
                using (new GUILayout.VerticalScope(ClassTypeDrawer.ContentStyle))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("起始帧");
                        EditorGUI.BeginChangeCheck();
                        int start = EditorGUILayout.IntField(Action.StartFrame);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorView.RegistUndo("action start frame modify");
                            Action.StartFrame = Mathf.Max(start, 0);
                        }
                        if (Action.Data is IFrameClip)
                        {
                            GUILayout.Label("时长");
                            EditorGUI.BeginChangeCheck();
                            int length = EditorGUILayout.IntField(Action.Length);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorView.RegistUndo("action length modify");
                                Action.Length = length;
                            }
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("名字");
                        EditorGUI.BeginChangeCheck();
                        string name = EditorGUILayout.TextField(Action.Name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorView.RegistUndo("action name modify");
                            Action.Name = name;
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("描述");
                        EditorGUI.BeginChangeCheck();
                        string commit = EditorGUILayout.TextArea(Action.Comment);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorView.RegistUndo("action comment modify");
                            Action.Comment = commit;
                        }
                    }
                    DrawActionInspector();
                }
            }
        }
        protected bool LayoutGUI<T>(string label, ref T v, Func<T, GUILayoutOption[], T> func, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label);
                EditorGUI.BeginChangeCheck();
                var newVal = func(v, options);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorView.OnPropertyModify();
                    v = newVal;
                    return true;
                }
            }
            return false;
        }
        protected virtual void DrawActionInspector()
        {
            drawer.Draw(null, Action.Data, EditorView);
        }

        protected virtual void OnNodeMenu(GenericMenu menu)
        {
        }

        private static Dictionary<Type, Type> inspectorTypes;
        internal static FrameActionInspector CreateInspector(FrameLineEditorView editorView, FrameAction action)
        {
            if (inspectorTypes == null)
            {
                inspectorTypes = new Dictionary<Type, Type>();
                
                foreach (var type in TypeCollector<FrameActionInspector>.Types)
                {
                    var attr = type.GetCustomAttribute<CustomActionEditorAttribute>();
                    if (attr != null)
                    {
                        inspectorTypes[attr.Type] = type;
                    }
                }
            }
            if (inspectorTypes.TryGetValue(action.Data.GetType(), out Type inspectorType))
            {
                var inspector = Activator.CreateInstance(inspectorType) as FrameActionInspector;
                inspector.EditorView = editorView;
                inspector.Action = action;
                inspector.OnCreate();
                return inspector;
            }
            var defaultInspector = new FrameActionInspector() { EditorView = editorView, Action = action };
            defaultInspector.OnCreate();
            return defaultInspector;
        }
    }

    public class TFrameActionInspector<T> : FrameActionInspector where T : IFrameAction
    {
        public  T ActionData => (T)Action.Data;
    }
}
