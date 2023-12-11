using UnityEngine;
namespace FrameLine
{
    public class FrameLineAction : ISerializationCallbackReceiver
    {
        public string GUID;
        public int StartFrame;
        public int Length = 1;
        public bool Enable = true;
        public string Name;
        public string Comment;


        [SerializeField]
        private SerializationData jsonData;

        [System.NonSerialized]
        private IFrameLineAction data;
        public IFrameLineAction Data
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
        public void SetData(IFrameLineAction nodeData)
        {
            data = nodeData;
            OnBeforeSerialize();
        }
        public void Deserialize()
        {
            data = TypeSerializerHelper.Deserialize(jsonData) as IFrameLineAction;
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