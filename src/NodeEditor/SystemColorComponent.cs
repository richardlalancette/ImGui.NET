using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ImGuiNET;

namespace NodeEditor
{
    public static class SystemColorComponent
    {
        public static dynamic Draw(KeyValuePair<string, dynamic> keyValuePair, ref NodeData nodeData)
        {
            Color color = keyValuePair.Value;
            Vector4 v4 = new Vector4(color.R/255.0f, color.G/255.0f, color.B/255.0f, color.A/255.0f);
            ImGuiNET.ImGui.ColorEdit4(keyValuePair.Key, ref v4, ImGuiColorEditFlags.PickerHueWheel);
            return Color.FromArgb((int) (v4.W*255.0f), (int) (v4.X*255.0f), (int) (v4.Y*255.0f), (int) (v4.Z*255.0f));
        }
    }
}