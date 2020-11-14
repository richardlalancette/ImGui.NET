﻿using ImGuiNET;

namespace ImGui.NET.SampleProgram
{
    public class ImStyleWindowController : ImWindowController
    {
        public ImStyleWindowController(string title, int width, int height, ImGuiCond condition) : base(title, width, height, condition)
        {
        }

        protected override void DrawWindowElements()
        {
            ImGuiNET.ImGui.ShowStyleEditor();
        }
    }
}