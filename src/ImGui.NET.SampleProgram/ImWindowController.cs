using System.Numerics;
using ImGuiNET;

namespace ImGui.NET.SampleProgram
{
    class ImWindowController : AbstractImController
    {
        public readonly string Title;
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public ImGuiCond SizeCondition { get; set; } = ImGuiCond.FirstUseEver;
        private ImGuiWindowFlags WindowStyle { get; set; }

        public ImWindowController(string title)
        {
            Title = title;
        }

        public ImWindowController(string title, int width, int height)
        {
            Title = title;
            Width = width;
            Height = height;
        }

        public ImWindowController(string title, int width, int height, ImGuiCond sizeCondition)
        {
            Title = title;
            Width = width;
            Height = height;
            SizeCondition = sizeCondition;
        }

        public ImWindowController(string title, int width, int height, ImGuiCond sizeCondition, ImGuiWindowFlags windowStyle)
        {
            Title = title;
            Width = width;
            Height = height;
            SizeCondition = sizeCondition;
            WindowStyle = windowStyle;
        }

        public override void Draw()
        {
            if (Enabled)
            {
                ImGuiNET.ImGui.SetNextWindowSize(new Vector2(Width, Height), SizeCondition);
                ImGuiNET.ImGui.Begin(Title, ref Enabled, WindowStyle);
                DrawWindowElements();
                ImGuiNET.ImGui.End();
            }
        }

        protected virtual void DrawWindowElements()
        {
        }
    }
}