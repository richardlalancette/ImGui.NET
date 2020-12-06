using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ImGui.NET.SampleProgram
{
    public static class Color4Component
    {
        public static dynamic Draw(KeyValuePair<string, dynamic> keyValuePair, ref NodeData nodeData)
        {
            Vector4 color = keyValuePair.Value;
            var newGuid = "##color" + keyValuePair.Key;
            ImGuiNET.ImGui.ColorEdit4(newGuid, ref color);
            ImGuiNET.ImGui.SameLine();
            ImGuiNET.ImGui.Text($"{keyValuePair.Key}");
            
            return Color.FromArgb((int) color.W, (int) color.X, (int) color.Y, (int) color.Z);
        }
    }
}