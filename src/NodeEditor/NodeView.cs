using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ImGui.NET.NodeEditor;
using Im = ImGuiNET.ImGui;

namespace NodeEditor
{
    public class NodeView
    {
        public void Draw(string name, Node node)
        {
            NodeData data = node.Data;
            Im.Text(string.Format($"{name}"));
            Im.Spacing();

            foreach (KeyValuePair<string, dynamic> keyValuePair in data)
            {
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