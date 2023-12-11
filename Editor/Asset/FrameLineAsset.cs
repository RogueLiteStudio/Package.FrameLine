using System.Collections.Generic;
using UnityEngine;
namespace FrameLine
{
    public class FrameLineAsset : ScriptableObject
    {
        public string Comment;
        public List<FrameActionGroup> Groups = new List<FrameActionGroup>();
    }
}