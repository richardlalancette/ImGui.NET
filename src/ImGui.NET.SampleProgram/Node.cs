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

        public Node()
        {
        }
        
        public Node(int id, string name, Vector2 pos, NodeData data, int inputsCount, int outputsCount)
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
                var connectorPos = panningOffset + GetInputSlotPos(slotIdx);
                DrawConnector(drawList, connectorPos, StyleSheet.NodeSlotRadius);
            }

            for (int slotIdx = 0; slotIdx < OutputsCount; slotIdx++)
            {
                var connectorPos = panningOffset + GetOutputSlotPos(slotIdx);
                DrawConnector(drawList, connectorPos, StyleSheet.NodeSlotRadius);
            }
        }

        private static void DrawConnector(ImDrawListPtr drawList, Vector2 connectorPosition, float nodeSlotRadius)
        {
            var c = Im.GetCursorScreenPos();
            var buttonPosition = new Vector2(connectorPosition.X - nodeSlotRadius, connectorPosition.Y - nodeSlotRadius);
            Im.SetCursorScreenPos(buttonPosition);
            Im.InvisibleButton("slotidx", new Vector2(nodeSlotRadius * 2, nodeSlotRadius * 2));
            bool connectorHovered = Im.IsItemHovered();
            Im.SetCursorScreenPos(c);

            if (connectorHovered)
            {
                drawList.AddCircleFilled(connectorPosition, nodeSlotRadius * 1.5f, 0xffffffff);
                drawList.AddCircleFilled(connectorPosition, nodeSlotRadius * 1.5f / 2.0f, 0xff777777);
            }
            else
            {
                drawList.AddCircleFilled(connectorPosition, nodeSlotRadius, 0xffffffff);
                drawList.AddCircleFilled(connectorPosition, nodeSlotRadius/ 2.0f, 0xff777777);
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