using System.Collections.Generic;

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
        public Dictionary<string, Style> Styles  { get; } = new Dictionary<string, Style>();

        public StyleSheet()
        {
        }

        public StyleSheet(Dictionary<string, Style> styles)
        {
            Styles = styles;
        }

        public virtual void Apply()
        {
            foreach (KeyValuePair<string,Style> style in Styles)
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
    }
}