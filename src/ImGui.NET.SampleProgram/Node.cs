using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;
using Im = ImGuiNET.ImGui;

// https: //mikecodes.net/2020/05/11/in-app-scripts-with-c-roslyn/
// https://www.newtonsoft.com/json/help/html/CreateJsonAnonymousObject.htm

/*!
 JObject rss =
    new JObject(
        new JProperty("channel",
            new JObject(
                new JProperty("title", "James Newton-King"),
                new JProperty("link", "http://james.newtonking.com"),
                new JProperty("description", "James Newton-King's blog."),
                new JProperty("item",
                    new JArray(
                        from p in posts
                        orderby p.Title
                        select new JObject(
                            new JProperty("title", p.Title),
                            new JProperty("description", p.Description),
                            new JProperty("link", p.Link),
                            new JProperty("category",
                                new JArray(
                                    from c in p.Categories
                                    select new JValue(c)))))))));
*/
namespace ImGui.NET.SampleProgram
{
    class NodeData
    {
        public JObject json = new JObject();
        public float Value = 1.0f;
        public Vector4 color;

        class testObject
        {
            [JsonProperty]
            private float value;
        }

        public void AddField<T>(string fieldName, T value)
        {
            JObject fromObject = JObject.FromObject(value);
            fromObject.AddFirst(new JProperty("Type", value.GetType().Name));
            json.Add(fieldName, fromObject);
        }

        public void SetField<T>(string fieldName, T value)
        {
            JObject fromObject = JObject.FromObject(value);
            fromObject.AddFirst(new JProperty("Type", value.GetType().Name));
            json[fieldName].Replace(fromObject);
        }
    }

    class NodeView
    {
        public void Draw(string name, int nodeIdx, NodeData data)
        {
            Im.Text(string.Format($"{name}"));
            // Im.SliderFloat($"##value{nodeIdx}", ref data.Value, 0.0f, 1.0f, "Alpha %.2f");

            foreach (KeyValuePair<string, JToken> keyValuePair in data.json)
            {
                JToken fieldType = keyValuePair.Value["Type"];

                switch (fieldType?.ToString())
                {
                case "Vector4":
                    Vector4 color = keyValuePair.Value.ToObject<Vector4>();
                    var newGuid = "##color" + keyValuePair.Key;
                    Im.ColorEdit4(newGuid, ref color);
                    Im.SameLine();
                    Im.Text($"{keyValuePair.Key}");
                    data.SetField(keyValuePair.Key, color);
                    break;
                    case "MagickImage":
                    Im.Image((IntPtr) 0, Vector2.One * 300.0f);
                        break;
                }
            }

            Im.Text($"{data.json}");
        }
    }

    class Node
    {
        public NodeData Data = new NodeData();
        public NodeView View = new NodeView();

        public readonly int Id;
        public readonly string Name;

        public Vector2 Pos
        {
            get => Data.json["Pos"].ToObject<Vector2>();
            set => Data.json["Pos"].Replace(JObject.FromObject(value));
        }

        public Vector2 Size;
        public int InputsCount;
        public int OutputsCount;
        public bool Selected;
        public bool Hovered;
        public bool Down;
        public bool Dragged;

        public Node(int id, string name, Vector2 pos, NodeData data, Vector4 color, int inputsCount, int outputsCount)
        {
            Data = data;
            // Data.json.PropertyChanged += NodePropertyChanged;
            // Data.json.CollectionChanged += NodeCollectionChanged;
            Id = id;
            Name = name;
            // Data.AddField("Name", name);
            Data.AddField("Pos", pos);
            Size = new Vector2();
            InputsCount = inputsCount;
            OutputsCount = outputsCount;
            Selected = false;
            Hovered = false;
            Down = false;
            Dragged = false;
        }

        private void NodeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine($"Property Changed{sender?.ToString()}: {e.ToString()}");
        }

        private void NodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Console.WriteLine($"Property Changed{sender?.ToString()}: {e.ToString()}");
        }

        public Vector2 GetInputSlotPos(int slotNo)
        {
            return new Vector2(Pos.X, Pos.Y + Size.Y * ((float) slotNo + 1) / ((float) InputsCount + 1));
        }

        public Vector2 GetOutputSlotPos(int slotNo)
        {
            return new Vector2(Pos.X + Size.X, Pos.Y + Size.Y * ((float) slotNo + 1) / ((float) OutputsCount + 1));
        }

        public void Draw(Vector2 panningOffset, ImDrawListPtr drawList, int nodeIdx, ref bool openContextMenu)
        {
            Im.PushItemWidth(StyleSheet.DefaultNodeWidth);

            Im.PushID(Id);
            ImVec2 nodeRectMin = panningOffset + Pos;

            drawList.ChannelsSetCurrent(StyleSheet.ForegroundChannel);
            bool oldAnyActive = Im.IsAnyItemActive();
            Im.SetCursorScreenPos(nodeRectMin + StyleSheet.NodeWindowPadding);

            Im.BeginGroup();
            View.Draw(Name, nodeIdx, Data);
            Im.EndGroup();

            // Save the size of what we have emitted and whether any of the widgets are being used
            bool nodeWidgetsActive = (!oldAnyActive && Im.IsAnyItemActive());
            Size = Im.GetItemRectSize() + StyleSheet.NodeWindowPadding + StyleSheet.NodeWindowPadding;
            ImVec2 nodeRectMax = nodeRectMin + Size;

            // Display node box
            drawList.ChannelsSetCurrent(StyleSheet.BackgroundChannel);
            Im.SetCursorScreenPos(nodeRectMin);
            Im.InvisibleButton("node", Size);

            if (Im.IsItemHovered())
            {
                Hovered = true;

                if (Im.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    Dragged = true;
                }

                if (Im.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Down = true;
                }

                if (Im.IsMouseReleased(ImGuiMouseButton.Left) && Down)
                {
                    Down = false;
                    Selected = !Selected;
                    Dragged = false;
                }

                if (Im.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    Dragged = false;
                }

                if (Im.IsMouseDragging(ImGuiMouseButton.Left) && Down)
                    Down = false;

                openContextMenu |= Im.IsMouseReleased(ImGuiMouseButton.Right);
            }

            if (Dragged)
            {
                ImGuiIOPtr io = Im.GetIO();
                Pos += io.MouseDelta;
            }

            if (Hovered)
            {
                drawList.AddRectFilled(nodeRectMin, nodeRectMax, StyleSheet.GreyColor, Im.GetStyle().FrameRounding);
            }
            else
            {
                drawList.AddRectFilled(nodeRectMin, nodeRectMax, StyleSheet.DarkGreyColor, Im.GetStyle().FrameRounding);
            }

            if (Selected)
            {
                drawList.AddRect(nodeRectMin, nodeRectMax, StyleSheet.White, Im.GetStyle().FrameRounding);
            }
            else
            {
                drawList.AddRect(nodeRectMin, nodeRectMax, StyleSheet.NodeBorderColor, Im.GetStyle().FrameRounding);
            }

            for (int slotIdx = 0; slotIdx < InputsCount; slotIdx++)
            {
                drawList.AddCircleFilled(panningOffset + GetInputSlotPos(slotIdx), StyleSheet.NodeSlotRadius, 0xffffffff);
                drawList.AddCircleFilled(panningOffset + GetInputSlotPos(slotIdx), StyleSheet.NodeSlotRadius / 2, 0xff777777);
            }

            for (int slotIdx = 0; slotIdx < OutputsCount; slotIdx++)
            {
                drawList.AddCircleFilled(panningOffset + GetOutputSlotPos(slotIdx), StyleSheet.NodeSlotRadius, 0xffffffff);
                drawList.AddCircleFilled(panningOffset + GetOutputSlotPos(slotIdx), StyleSheet.NodeSlotRadius / 2, 0xff777777);
            }

            Im.PopID();

            Im.PopItemWidth();
        }
    }

    class NodeLink
    {
        public int InputIdx;
        public int InputSlot;
        public int OutputIdx;
        public int OutputSlot;

        public NodeLink(int inputIdx, int inputSlot, int outputIdx, int outputSlot)
        {
            InputIdx = inputIdx;
            InputSlot = inputSlot;
            OutputIdx = outputIdx;
            OutputSlot = outputSlot;
        }
    }
}