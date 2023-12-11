using System;

namespace FrameLine
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomFrameLineProcessAttribute : Attribute
    {
        public Type AssetType { get; private set; }

        public CustomFrameLineProcessAttribute(Type frameLineType)
        {
            AssetType = frameLineType;
        }
    }
}
