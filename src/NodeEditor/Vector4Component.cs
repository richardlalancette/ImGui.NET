using System.Collections.Generic;
using System.Numerics;

namespace NodeEditor
{
    public static class Vector4Component
    {
        public static dynamic Draw(KeyValuePair<string, dynamic> keyValuePair, ref NodeData nodeData)
        {
            Vector4 vector = keyValuePair.Value;
            ImGuiNET.ImGui.DragFloat4(keyValuePair.Key, ref vector);
            return vector;
        }
    }
}