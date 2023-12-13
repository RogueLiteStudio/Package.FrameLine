using System.Collections.Generic;
using UnityEngine;
namespace FrameLine
{
    public class FrameLineAsset : ScriptableObject
    {
        public string Comment;
        public List<FrameActionGroup> Groups = new List<FrameActionGroup>();

        public FrameActionGroup FindGroup(string id)
        {
            foreach (var group in Groups)
            {
                if (group.GUID == id)
                    return group;
            }
            return null;
        }
    }
}