using System.Collections.Generic;
using System.Numerics;

namespace NodeEditor
{
    public static class Vector3Component
    {
        public static dynamic Draw(KeyValuePair<string, dynamic> keyValuePair, ref NodeData nodeData)
        {
            Vector3 vector = keyValuePair.Value;
            ImGuiNET.ImGui.DragFloat3(keyValuePair.Key, ref vector);
            return vector;
        }
    }
}