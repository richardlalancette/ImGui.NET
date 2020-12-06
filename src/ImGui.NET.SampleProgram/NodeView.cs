﻿using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Newtonsoft.Json;

namespace ImGui.NET.SampleProgram
{
    public class NodeView
    {
        public void Draw(string name, Node node)
        {
            NodeData data = node.Data;
            ImGuiNET.ImGui.Text(string.Format($"{name}"));

            foreach (KeyValuePair<string, dynamic> keyValuePair in data)
            {
                // var t = keyValuePair.Value.GetType();
                // var v = keyValuePair.Value;
                // var a = keyValuePair.Value.GetType().GetType();
                // var tname = t.ToString();
                // bool isColor = keyValuePair.Value is System.Drawing.Color;

                TypeSwitch.Do(
                    keyValuePair.Value,
                    TypeSwitch.Case<Color>(() => { data[keyValuePair.Key] = SystemColorComponent.Draw(keyValuePair, ref data); }),
                    TypeSwitch.Case<Vector2>(() => { data[keyValuePair.Key] = Vector2Component.Draw(keyValuePair, ref data); }),
                    TypeSwitch.Case<Vector3>(() => { data[keyValuePair.Key] = Vector3Component.Draw(keyValuePair, ref data); }),
                    TypeSwitch.Case<Vector4>(() => { data[keyValuePair.Key] = Vector4Component.Draw(keyValuePair, ref data); }),
                    TypeSwitch.Case<ImageMagick.IMagickImage>(x => { data[keyValuePair.Key] = MagickImageComponent.Draw(keyValuePair, ref data); }),
                    TypeSwitch.Default(() => { data[keyValuePair.Key] = UndefinedTypeComponent.Draw(keyValuePair, ref data); }));
            }

            // string serializeData = JsonConvert.SerializeObject(data, Formatting.Indented);
            // Im.Text($"{serializeData}");
            // string serializeObject = JsonConvert.SerializeObject(node, Formatting.Indented);
            // ImGuiNET.ImGui.Text($"{serializeObject}");
        }
    }
}