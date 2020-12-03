using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json.Linq;

namespace ImGui.NET.SampleProgram
{
    public static class Vector4Component
    {
        public static void Draw(KeyValuePair<string, JToken> keyValuePair, ref NodeData nodeData)
        {
            Vector4 color = keyValuePair.Value.ToObject<Vector4>();
            var newGuid = "##color" + keyValuePair.Key;
            ImGuiNET.ImGui.ColorEdit4(newGuid, ref color);
            ImGuiNET.ImGui.SameLine();
            ImGuiNET.ImGui.Text($"{keyValuePair.Key}");
            nodeData.SetField(keyValuePair.Key, color);
        }
    }
}