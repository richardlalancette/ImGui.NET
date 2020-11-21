using System.Numerics;
using ImGuiNET;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;
using Im = ImGuiNET.ImGui;

namespace ImGui.NET.SampleProgram
{
    class Node
    {
        public readonly int Id;
        public readonly string Name;
        public Vector2 Pos;
        public Vector2 Size;
        public float Value;
        public Vector4 Color;
        public int InputsCount;
        public int OutputsCount;
        public bool Selected;
        public bool Hovered;
        public bool Down;
        public bool Dragged;

        public Node(int id, string name, Vector2 pos, float value, Vector4 color, int inputsCount, int outputsCount)
        {
            Id = id;
            Name = name;
            Pos = pos;
            Size = new Vector2();
            Value = value;
            Color = color;
            InputsCount = inputsCount;
            OutputsCount = outputsCount;
            Selected = false;
            Hovered = false;
            Down = false;
            Dragged = false;
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
            Im.Text(string.Format($"{Name}"));
            Im.SliderFloat($"##value{nodeIdx}", ref Value, 0.0f, 1.0f, "Alpha %.2f");
            Im.ColorEdit4("##color", ref Color);
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