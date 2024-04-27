using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FrameLine
{
    public class TFrameLineWindow<TAsset> : FrameLineWindow where TAsset : FrameLineAsset
    {
        private ObjectField assetSelect;
        public TAsset Asset => currentAsset as TAsset;

        public override void OnOpenAsset(FrameLineAsset asset)
        {
            base.OnOpenAsset(asset);
            assetSelect?.SetValueWithoutNotify(asset);
        }

        protected override void OnCreateLayOut()
        {
            base.OnCreateLayOut();
            var createBtn = TopToolbar.Q<ToolbarButton>("createAction");
            if (createBtn == null)
            {
                createBtn = new ToolbarButton(CreateAction) { text = "创建" };
                TopToolbar.Add(createBtn);
            }
            else
            {
                createBtn.clicked += CreateAction;
            }
            assetSelect = TopToolbar.Q<ObjectField>("assetSelect");
            if (assetSelect == null)
            {
                assetSelect = new ObjectField();
            }
            assetSelect.objectType = typeof(TAsset);
            assetSelect.allowSceneObjects = true;

            if (EditorView)
            {
                assetSelect.SetValueWithoutNotify(Asset);
            }
            assetSelect.RegisterValueChangedCallback(OnGraphSelectChange);
            TopToolbar.Add(assetSelect);
        }
        protected void CreateAction()
        {
            var graph = FrameLineProcess.OnAssetCreateAction(typeof(TAsset));
            if (graph != null)
            {
                OpenAsset(graph);
            }
        }

        protected void OnGraphSelectChange(ChangeEvent<UnityEngine.Object> evt)
        {
            var asset = evt.newValue as TAsset;
            if (asset)
            {
                OpenAsset(asset);
            }
        }
    }
}
