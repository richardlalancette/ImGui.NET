using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;
using Im = ImGuiNET.ImGui;

// https: //mikecodes.net/2020/05/11/in-app-scripts-with-c-roslyn/
// https://www.newtonsoft.com/json/help/html/CreateJsonAnonymousObject.htm
namespace ImGui.NET.SampleProgram
{
    public class Node
    {
        public NodeData Data = new NodeData();
        public NodeView View = new NodeView();

        [JsonProperty]
        public readonly int Id;

        [JsonProperty]
        public string Name;

        [JsonProperty]
        public Vector2 Position;

        [JsonProperty]
        public Vector2 Size;

        public int InputsCount;
        public int OutputsCount;
        public bool Selected;
        public bool Hovered;
        public bool LeftMouseButtonDown;
        public bool Dragged;

        public Node(int id, string name, Vector2 pos, NodeData data, Vector4 backgroundColor, int inputsCount, int outputsCount)
        {
            Data = data;

            // Todo convert these as fields eventually.
            Id = id;
            Position = pos;
            Name = name;
            Size = new Vector2();
            InputsCount = inputsCount;
            OutputsCount = outputsCount;
            Selected = false;
            Hovered = false;
            LeftMouseButtonDown = false;
            Dragged = false;

        }

        public Vector2 GetInputSlotPos(int slotNo)
        {
            return new Vector2(Position.X, Position.Y + Size.Y * ((float) slotNo + 1) / ((float) InputsCount + 1));
        }

        public Vector2 GetOutputSlotPos(int slotNo)
        {
            return new Vector2(Position.X + Size.X, Position.Y + Size.Y * ((float) slotNo + 1) / ((float) OutputsCount + 1));
        }

        public void Draw(Vector2 panningOffset, ImDrawListPtr drawList, int nodeIdx, ref bool openEditContextMenu)
        {
            Im.PushItemWidth(StyleSheet.DefaultNodeWidth);
            Im.PushID(Id);

            {
                drawList.ChannelsSetCurrent(StyleSheet.ForegroundChannel);
                Im.SetCursorScreenPos(panningOffset + Position + StyleSheet.NodeWindowPadding);

                DrawNodeContentView();
                DrawNodeBox(panningOffset, drawList, ref openEditContextMenu);
                DrawNodeSlots(panningOffset, drawList);
            }
            
            Im.PopID();
            Im.PopItemWidth();
        }

        private void DrawNodeSlots(Vector2 panningOffset, ImDrawListPtr drawList)
        {
            for (int slotIdx = 0; slotIdx < InputsCount; slotIdx++)
            {
                drawList.AddCircleFilled(panningOffset + GetInputSlotPos(slotIdx), StyleSheet.NodeSlotRadius, 0xffffffff);
                drawList.AddCircleFilled(panningOffset + GetInputSlotPos(slotIdx), StyleSheet.NodeSlotRadius / 2, 0xff777777);

                var c = Im.GetCursorPos();
                Im.SetCursorPos(panningOffset + GetInputSlotPos(slotIdx));
                Im.Button("slotidx", new Vector2(StyleSheet.NodeSlotRadius * 2, StyleSheet.NodeSlotRadius * 2));

                if (Im.IsItemHovered())
                {
                    Im.BeginTooltip();
                    Im.Text("Hi");
                    Im.EndTooltip();
                }

                Im.SetCursorPos(c);
            }

            for (int slotIdx = 0; slotIdx < OutputsCount; slotIdx++)
            {
                drawList.AddCircleFilled(panningOffset + GetOutputSlotPos(slotIdx), StyleSheet.NodeSlotRadius, 0xffffffff);
                drawList.AddCircleFilled(panningOffset + GetOutputSlotPos(slotIdx), StyleSheet.NodeSlotRadius / 2, 0xff777777);
            }
        }

        private void DrawNodeBox(Vector2 panningOffset, ImDrawListPtr drawList, ref bool openContextMenu)
        {
            ImVec2 nodeRectMin = panningOffset + Position;
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
                    LeftMouseButtonDown = true;
                }

                if (Im.IsMouseReleased(ImGuiMouseButton.Left) && LeftMouseButtonDown)
                {
                    LeftMouseButtonDown = false;
                    Selected = !Selected;
                    Dragged = false;
                }

                if (Im.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    Dragged = false;
                }

                if (Im.IsMouseDragging(ImGuiMouseButton.Left) && LeftMouseButtonDown)
                    LeftMouseButtonDown = false;

                openContextMenu |= Im.IsMouseReleased(ImGuiMouseButton.Right);
            }

            if (Dragged)
            {
                ImGuiIOPtr io = Im.GetIO();
                Position += io.MouseDelta;
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
        }

        private void DrawNodeContentView()
        {
            Im.BeginGroup();
            View.Draw(Name, this);
            Im.EndGroup();
            Size = Im.GetItemRectSize() + StyleSheet.NodeWindowPadding + StyleSheet.NodeWindowPadding;
        }
    }
}