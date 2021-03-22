using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ImGui.Extensions
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

    /*! This needs to be refactored at some point. Can't have a stylesheet with hardcoded values :D */
    public class StyleSheet
    {
        public readonly float GridSize = 64.0f;
        public readonly float LinkDefaultThickness = 5.0f;
        public readonly uint DarkGreyColor = 0xFF101010;
        public readonly uint GreyColor = 0xFF2B2B2B;
        public readonly uint LightGrey = 0xFFC0C0C0;
        public readonly uint LinkBorderColor = 0xFFCCCCCC;
        public readonly uint LinkColor = 0xFF111111;
        public readonly uint Red = 0xFF0000FF;
        public readonly uint White = 0xFFFFFFFF;
        public readonly Vector2 NodeWindowPadding = new(8.0f, 8.0f);
        public readonly float DefaultNodeWidth = 160.0f;
        public readonly float NodeSlotRadius = 4.0f;
        public readonly uint NodeBorderColor = 0xFF999999;
        public readonly int BackgroundChannel = 0;
        public readonly int ForegroundChannel = 1;
        public readonly int NodeListDefaultWidth = 200;
        
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
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;
            byte a = color.A;
            return (uint) (a << 24 | b << 16 | g << 8 | r);
        }
    }
}