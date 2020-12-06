using System;
using System.Collections.Generic;
using System.Numerics;

namespace ImGui.NET.SampleProgram
{
    public class MagickImageComponent
    {
        public static dynamic Draw(in KeyValuePair<string, dynamic> keyValuePair, ref NodeData data)
        {
            ImGuiNET.ImGui.Image((IntPtr) 0, Vector2.One * 300.0f);

            return keyValuePair.Value;
        }
    }
}