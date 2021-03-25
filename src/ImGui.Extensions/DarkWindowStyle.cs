﻿using System.Numerics;
using ImGuiNET;

namespace ImGui.Extensions
{
    public class DarkWindowStyle : Style
    {
        public override void Apply()
        {
            base.Apply();
            ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
        }

        public override void Restore()
        {
            ImGuiNET.ImGui.PopStyleVar();
            base.Restore();
        }
    }
}