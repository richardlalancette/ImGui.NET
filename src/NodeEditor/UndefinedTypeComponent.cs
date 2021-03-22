using System.Collections.Generic;
using System.Drawing;
using ImGui.Extensions;
using ImGuiNET;
using Im = ImGuiNET.ImGui;

namespace NodeEditor
{
    public class UndefinedTypeComponent
    {
        public static dynamic Draw(KeyValuePair<string, dynamic> keyValuePair, ref NodeData data)
        {
            Im.PushStyleColor(ImGuiCol.Text, StyleSheet.ImColor(Color.Coral));
            Im.Text("Missing component Implementation:");
            Im.Text(keyValuePair.Value.GetType().ToString());
            Im.PopStyleColor();
            
            return keyValuePair.Value;
        }
    }
}