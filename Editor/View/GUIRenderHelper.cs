using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace FrameLine
{
    public enum BorderType
    {
        None,
        Top = 1,
        Left = 2,
        Right = 4,
        Bottom = 8,
        All = -1
    }

    public static class GUIRenderHelper
    {
        public static Vector4 GetRectBorderRadius(float borderRadius, BorderType borderType)
        {
            Vector4 borderRadiuses = Vector4.zero;
            if (borderRadius > 0)
            {
                borderRadiuses.x = (borderType & BorderType.Top) != 0 || (borderType & BorderType.Left) != 0 ? borderRadius : 0;
                borderRadiuses.y = (borderType & BorderType.Top) != 0 || (borderType & BorderType.Right) != 0 ? borderRadius : 0;
                borderRadiuses.z = (borderType & BorderType.Bottom) != 0 || (borderType & BorderType.Right) != 0 ? borderRadius : 0;
                borderRadiuses.w = (borderType & BorderType.Bottom) != 0 || (borderType & BorderType.Left) != 0 ? borderRadius : 0;
            }
            return borderRadiuses;
        }
        //画圆角矩形
        public static void DrawRect(Rect rect, Color color, float borderRadius, BorderType borderType)
        {
            Vector4 borderRadiuses = GetRectBorderRadius(borderRadius, borderType);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, Vector4.zero, borderRadiuses);
        }
        //话普通矩形
        public static void DrawRect(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, Vector4.zero, 0);
        }

        public static void DrawWireRect(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, Vector4.one, 0);
        }
        public static void DrawWireRect(Rect rect, Color color, float borderRadius, BorderType borderType)
        {
            Vector4 borderRadiuses = GetRectBorderRadius(borderRadius, borderType);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, Vector4.one, borderRadiuses);
        }

        public static void DrawCircle(Vector2 pos, Color color, float radius)
        {
            Vector2 halfSize = new Vector2(radius, radius);

            Rect rect = new Rect(pos - halfSize, halfSize * 2);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, true, 0, color, 0, radius);
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            using (new Handles.DrawingScope(color))
            {
                Handles.DrawLine(start, end, thickness);
            }
        }

        public static void DrawFrameTag(int frame, float frameWidth, Color color)
        {
            float width = 7f;
            float height = 10f;
            Rect rect = new Rect((frame + 0.5f)*frameWidth - width *0.5f, 0, width, height);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, Vector4.zero, new Vector4(0, 0, width, width));
        }
    }
}
