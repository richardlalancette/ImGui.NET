using System.Collections.Generic;
using System.Numerics;

namespace ImGui.NET.SampleProgram
{
    public static class Vector2Component
    {
        public static dynamic Draw(KeyValuePair<string, dynamic> keyValuePair, ref NodeData nodeData)
        {
            Vector2 vector = keyValuePair.Value;
            ImGuiNET.ImGui.DragFloat2(keyValuePair.Key, ref vector);
            return vector;
        }
    }
}