using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace FrameLine
{
    public class FrameLineClipboard : ScriptableSingleton<FrameLineClipboard>
    {
        [System.Serializable]
        public struct ActionData
        {
            public int StartFrame;
            public int Length;
            public bool Enable;
            public string Name;
            public string Comment;
            public SerializationData Data;
        }
        [System.Serializable]
        public class AssetTypeClipboard
        {
            public string TypeName;
            public ActionData[] Actions;
        }

        public List<SerializationData> ActionDataClips = new List<SerializationData>();
        public List<AssetTypeClipboard> ActionClipboards = new List<AssetTypeClipboard>();

        public void CopyActionData(FrameAction action)
        {
            var data = TypeSerializerHelper.Serialize(action.Data);
            ActionDataClips.RemoveAll(it => it.TypeGUID == data.TypeGUID);
            ActionDataClips.Add(data);
        }
        public bool PastePropertyCheck(FrameAction action)
        {
            return ActionDataClips.Exists(it => it.TypeGUID == action.TypeGUID);
        }

        public void PasteActionProperty(FrameAction action)
        {
            int index = ActionDataClips.FindIndex(it => it.TypeGUID == action.TypeGUID);
            if (index >= 0)
            {
                var data = TypeSerializerHelper.Deserialize(ActionDataClips[index]) as IFrameEvent;
                action.SetData(data);
            }
        }

        public void CopyActions(FrameLineEditorView gui)
        {
            AssetTypeClipboard clipboard = new AssetTypeClipboard();
            clipboard.TypeName = gui.GetType().FullName;
            clipboard.Actions = ActionToClipboardData(gui.SelectedActions.Select(it=> gui.Group.Find(it)).Where(it=>it != null));
            ActionClipboards.RemoveAll(it => it.TypeName == clipboard.TypeName);
            ActionClipboards.Add(clipboard);
        }

        public bool PasteActionsCheck(FrameLineEditorView gui)
        {
            var typeName = gui.GetType().FullName;
            return ActionClipboards.Exists(it => it.TypeName == typeName);
        }

        public bool PasteActions(FrameLineEditorView gui)
        {
            var typeName = gui.GetType().FullName;
            int idx = ActionClipboards.FindIndex(it => it.TypeName == typeName);
            if (idx >= 0)
            {
                gui.PasteActions(ActionClipboards[idx].Actions);
                return true;
            }
            return false;
        }

        public static ActionData[] ActionToClipboardData(IEnumerable<FrameAction> actionRefs)
        {
            return actionRefs.Select(action => new ActionData
            {
                StartFrame = action.StartFrame,
                Length = action.Length,
                Enable = action.Enable,
                Name = action.Name,
                Comment = action.Comment,
                Data = TypeSerializerHelper.Serialize(action.Data),
            }).ToArray();
        }
    }
}
