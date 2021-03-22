using System.Numerics;
using ImGui.Extensions;
using ImGuiNET;
using Newtonsoft.Json;
using ImVec2 = System.Numerics.Vector2;
using Im = ImGuiNET.ImGui;

// https: //mikecodes.net/2020/05/11/in-app-scripts-with-c-roslyn/
// https://www.newtonsoft.com/json/help/html/CreateJsonAnonymousObject.htm
namespace NodeEditor
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
        public static StyleSheet Styles = new();
        
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
            Im.PushItemWidth(Styles.DefaultNodeWidth);
            Im.PushID(Id);

            {
                drawList.ChannelsSetCurrent(Styles.ForegroundChannel);
                Im.SetCursorScreenPos(panningOffset + Position + Styles.NodeWindowPadding);

                DrawNodeContentView();
                DrawNodeBox(panningOffset, drawList, ref openEditContextMenu);
                DrawNodeConnectors(panningOffset, drawList);
            }
            
            Im.PopID();
            Im.PopItemWidth();
        }

        private void DrawNodeConnectors(Vector2 panningOffset, ImDrawListPtr drawList)
        {
            for (int inputConnectorIndex = 0; inputConnectorIndex < InputsCount; inputConnectorIndex++)
            {
                var connectorPos = panningOffset + GetInputSlotPos(inputConnectorIndex);
                DrawConnector(drawList, connectorPos, Styles.NodeSlotRadius);
            }

            for (int outputConnectorIndex = 0; outputConnectorIndex < OutputsCount; outputConnectorIndex++)
            {
                var connectorPos = panningOffset + GetOutputSlotPos(outputConnectorIndex);
                DrawConnector(drawList, connectorPos, Styles.NodeSlotRadius);
            }
        }

        private static void DrawConnector(ImDrawListPtr drawList, Vector2 connectorPosition, float nodeSlotRadius)
        {
            var c = Im.GetCursorScreenPos();
            
            var centeredButtonPosition = new Vector2(connectorPosition.X - nodeSlotRadius, connectorPosition.Y - nodeSlotRadius);
            
            Im.SetCursorScreenPos(centeredButtonPosition);
            
            Im.InvisibleButton("slotidx", new Vector2(nodeSlotRadius * 2, nodeSlotRadius * 2));
            bool connectorHovered = Im.IsItemHovered();
            bool connectorActive = Im.IsItemActive();
            
            Im.SetCursorScreenPos(c);

            if (connectorHovered)
            {
                if (connectorActive)
                {
                    bool mouseDragging = Im.IsMouseDragging(ImGuiMouseButton.Left);
                    
                    drawList.AddCircleFilled(connectorPosition, nodeSlotRadius * 2.5f, 0xffffffff);
                    drawList.AddCircleFilled(connectorPosition, nodeSlotRadius * 2.5f / 2.0f, 0xff777777);
                }
                else
                {
                    drawList.AddCircleFilled(connectorPosition, nodeSlotRadius * 1.5f, 0xffffffff);
                    drawList.AddCircleFilled(connectorPosition, nodeSlotRadius * 1.5f / 2.0f, 0xff777777);
                }            
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
            drawList.ChannelsSetCurrent(Styles.BackgroundChannel);
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
                drawList.AddRectFilled(nodeRectMin, nodeRectMax, Styles.GreyColor, Im.GetStyle().FrameRounding);
            }
            else
            {
                drawList.AddRectFilled(nodeRectMin, nodeRectMax, Styles.DarkGreyColor, Im.GetStyle().FrameRounding);
            }

            if (Selected)
            {
                drawList.AddRect(nodeRectMin, nodeRectMax, Styles.White, Im.GetStyle().FrameRounding);
            }
            else
            {
                drawList.AddRect(nodeRectMin, nodeRectMax, Styles.NodeBorderColor, Im.GetStyle().FrameRounding);
            }
        }

        private void DrawNodeContentView()
        {
            Im.BeginGroup();
            View.Draw(Name, this);
            Im.EndGroup();
            Size = Im.GetItemRectSize() + Styles.NodeWindowPadding + Styles.NodeWindowPadding;
        }
    }
}