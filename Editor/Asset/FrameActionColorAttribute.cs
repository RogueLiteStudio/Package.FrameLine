using System;

namespace FrameLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class FrameActionColorAttribute : Attribute
    {
        public string Color { get; private set; }

        public FrameActionColorAttribute(string color)
        {
            Color = color;
        }
    }
}
