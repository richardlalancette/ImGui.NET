using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json.Linq;

namespace ImGui.NET.SampleProgram
{
    public class MagickImageComponent
    {
        public static void Draw(in KeyValuePair<string, JToken> keyValuePair, ref NodeData data)
        {
            ImGuiNET.ImGui.Image((IntPtr) 0, Vector2.One * 300.0f);
        }
    }
}