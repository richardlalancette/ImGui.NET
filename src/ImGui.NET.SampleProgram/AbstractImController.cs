﻿using System.Numerics;
using ImGuiNET;

namespace ImGui.NET.SampleProgram
{
    internal abstract class AbstractImController
    {
        protected bool Enabled = true;
        public abstract void Draw();
    }

    internal class DemoController : AbstractImController
    {
        public override void Draw()
        {
            if (Enabled)
            {
                // Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                ImGuiNET.ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGuiNET.ImGui.ShowDemoWindow(ref Enabled);
            }
        }
    }
}