using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ImGui.NET.SampleProgram
{
    public class Style
    {
        public virtual void Apply()
        {
        }

        public virtual void Restore()
        {
        }
    };

    public class StyleSheet
    {
        public static readonly Vector2 NodeWindowPadding = new Vector2(8.0f, 8.0f);
        public const int BackgroundChannel = 0;
        public const int ForegroundChannel = 1;
        public const float DefaultNodeWidth = 160.0f;
        public const float NodeSlotRadius = 4.0f;
        public const uint NodeBorderColor = 0xFF999999;
        public const uint DarkGreyColor = 0xFF101010;
        public const float GridSize = 64.0f;
        public const float LinkDefaultThickness = 5.0f;
        public const int NodeListDefaultWidth = 200;
        public const uint GreyColor = 0xFF2B2B2B;
        public const uint LightGrey = 0xFFC0C0C0;
        public const uint Red = 0xFF0000FF;
        public const uint White = 0xFFFFFFFF;
        public const uint LinkColor = 0xFF111111;
        public const uint LinkBorderColor = 0xFFCCCCCC;

        public Dictionary<string, Style> Styles { get; } = new Dictionary<string, Style>();

        public StyleSheet()
        {
        }

        public StyleSheet(Dictionary<string, Style> styles)
        {
            Styles = styles;
        }

        public virtual void Apply()
        {
            foreach (KeyValuePair<string, Style> style in Styles)
            {
                style.Value.Apply();
            }
        }

        public virtual void Restore()
        {
            foreach (KeyValuePair<string, Style> style in Styles)
            {
                style.Value.Restore();
            }
        }


        public static uint ImColor(Color color)
        {
            byte r = (byte) (color.R);
            byte g = (byte) (color.G);
            byte b = (byte) (color.B);
            byte a = color.A;
            return (uint) (a << 24 | b << 16 | g << 8 | r);
        }
    }
}