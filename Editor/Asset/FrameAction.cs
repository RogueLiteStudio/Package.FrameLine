using UnityEngine;
namespace FrameLine
{
    [System.Serializable]
    public class FrameAction : ISerializationCallbackReceiver
    {
        public string GUID;
        public int StartFrame;
        public int Length = 1;
        public bool Enable = true;
        public string Name;
        public string Comment;
        public string TypeGUID => jsonData.TypeGUID;

        [SerializeField]
        private SerializationData jsonData;

        [System.NonSerialized]
        private IFrameAction data;
        public IFrameAction Data
        {
            get
            {
                if (data == null)
                {
                    Deserialize();
                }
                return data;
            }
        }
        public void SetData(IFrameAction nodeData)
        {
            data = nodeData;
            OnBeforeSerialize();
        }
        public void Deserialize()
        {
            data = TypeSerializerHelper.Deserialize(jsonData) as IFrameAction;
        }

        public void OnAfterDeserialize()
        {
            data = null;
        }
        public void OnBeforeSerialize()
        {
            if (data != null)
                jsonData = TypeSerializerHelper.Serialize(data);
        }
    }
}