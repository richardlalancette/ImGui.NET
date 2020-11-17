using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImVec2 = System.Numerics.Vector2;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;
using Im = ImGuiNET.ImGui;

// Currently using https://github.com/ocornut/imgui/issues/306#issuecomment-134657997
// Maybe use https://github.com/Nelarius/imnodes instead?
// or: https://github.com/juhgiyo/EpForceDirectedGraph.cs
// see also: https://stackoverflow.com/questions/44909321/optimising-a-force-directed-graph
// https://live.yworks.com/demos/index.html
// https://www.yworks.com/products/yfiles/features#layout
// https://github.com/alelievr/Mixture
// https://github.com/miroiu/nodify
// https://github.com/Wouterdek/NodeNetwork

// Helper libraries we could use for layout
// https://github.com/benzuk/box2d-netstandard

namespace ImGui.NET.SampleProgram
{
    public class ImNodeEditorWindowController : ImWindowController
    {
        private const int BackgroundChannel = 0;
        private const int ForegroundChannel = 1;
        private const float NodeSlotRadius = 4.0f;
        private const float GridSize = 64.0f;
        private const float DefaultNodeWidth = 160.0f;
        private const float LinkDefaultThickness = 5.0f;
        private const int NodeListDefaultWidth = 200;
        private const uint NodeBorderColor = 0xFF999999;
        private const uint GreyColor = 0xFF2B2B2B;
        private const uint DarkGreyColor = 0xFF101010;
        private const uint LightGrey = 0xFFC0C0C0;
        private const uint White = 0xFFFFFFFF;
        private const uint LinkColor = 0xFF111111;
        private const uint LinkBorderColor = 0xFFCCCCCC;

        private List<Node> _nodes = new List<Node>();
        private List<NodeLink> _links = new List<NodeLink>();
        private bool _showGrid = true;
        private bool _openContextMenu;

        private ImVec2 _panningPosition = ImVec2.Zero;
        private static readonly Vector2 NodeWindowPadding = new ImVec2(8.0f, 8.0f);

        public ImNodeEditorWindowController(string title) : base(title)
        {
            _nodes.Add(new Node(0, "MainTex", new ImVec2(40, 50), 0.5f, new ImVec4(1.0f, 0.4f, 0.4f, 1.0f), 1, 1));
            _nodes.Add(new Node(1, "BumpMap", new ImVec2(40, 150), 0.42f, new ImVec4(0.8f, 0.4f, 0.8f, 1.0f), 1, 1));
            _nodes.Add(new Node(2, "Combine", new ImVec2(270, 80), 1.0f, new ImVec4(0, 0.8f, 0.4f, 1.0f), 2, 2));
            _links.Add(new NodeLink(0, 0, 2, 0));
            _links.Add(new NodeLink(1, 0, 2, 1));
        }

        protected override void DrawWindowElements()
        {
            base.DrawWindowElements();

            foreach (var node in _nodes)
            {
                node.Hovered = false;
            }

            DrawNodeList();
            Im.SameLine();
            DrawEditor();
        }

        private void DrawEditor()
        {
            Im.BeginGroup();
            {
                DrawTopBar();

                Im.PushStyleVar(ImGuiStyleVar.FramePadding, ImVec2.One);
                Im.PushStyleVar(ImGuiStyleVar.WindowPadding, ImVec2.Zero);
                Im.PushStyleColor(ImGuiCol.ChildBg, GreyColor);
                Im.BeginChild("panning_region", ImVec2.Zero, true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
                Im.PopStyleVar(); // WindowPadding

                Vector2 panningOffset = Im.GetCursorScreenPos() + _panningPosition;
                ImDrawListPtr drawList = Im.GetWindowDrawList();
                DisplayGrid(drawList);
                drawList.ChannelsSplit(2);
                DisplayNodeLinks(panningOffset, ref drawList);
                DisplayNodes(panningOffset, ref drawList);
                drawList.ChannelsMerge();
                DrawContextMenu(panningOffset);
                HandlePanning();

                Im.EndChild();
                Im.PopStyleColor();
                Im.PopStyleVar();
            }
            Im.EndGroup();
        }

        private void DrawTopBar()
        {
            Im.Checkbox("Show grid", ref _showGrid);
            Im.SameLine();
            Im.Text($"- Hold middle mouse button to pan {_panningPosition.X}, {_panningPosition.Y})");
        }

        private void HandlePanning()
        {
            if (Im.IsWindowHovered() && !Im.IsAnyItemActive() && Im.IsMouseDragging(ImGuiMouseButton.Middle, 0.0f))
            {
                ImGuiIOPtr io = Im.GetIO();
                _panningPosition += io.MouseDelta;
            }
        }

        private void DrawContextMenu(Vector2 panningOffset)
        {
            _openContextMenu = false;

            if (Im.IsMouseReleased(ImGuiMouseButton.Right))
            {
                if (Im.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup) || !Im.IsAnyItemHovered())
                {
                    _openContextMenu = true;
                }
            }

            if (_openContextMenu)
            {
                Im.OpenPopup("context_menu");
            }

            Im.PushStyleVar(ImGuiStyleVar.WindowPadding, NodeWindowPadding);

            if (Im.BeginPopup("context_menu"))
            {
                ImVec2 scenePos = Im.GetMousePosOnOpeningCurrentPopup() - panningOffset;

                if (Im.MenuItem("Add"))
                {
                    _nodes.Add(new Node(_nodes.Count, "New node", scenePos, 0.5f, new ImVec4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 255.0f / 255.0f), 2, 2));
                }

                Im.EndPopup();
            }

            Im.PopStyleVar();
        }

        private void DisplayNodes(Vector2 panningOffset, ref ImDrawListPtr drawList)
        {
            Im.PushItemWidth(DefaultNodeWidth);

            for (int nodeIdx = 0; nodeIdx < _nodes.Count; nodeIdx++)
            {
                var node = _nodes[nodeIdx];

                Im.PushID(node.Id);
                ImVec2 nodeRectMin = panningOffset + node.Pos;

                drawList.ChannelsSetCurrent(ForegroundChannel);
                bool oldAnyActive = Im.IsAnyItemActive();
                Im.SetCursorScreenPos(nodeRectMin + NodeWindowPadding);

                Im.BeginGroup();
                Im.Text(string.Format($"{node.Name}"));
                Im.SliderFloat($"##value{nodeIdx}", ref node.Value, 0.0f, 1.0f, "Alpha %.2f");
                Im.ColorEdit4("##color", ref node.Color);
                Im.EndGroup();

                // Save the size of what we have emitted and whether any of the widgets are being used
                bool nodeWidgetsActive = (!oldAnyActive && Im.IsAnyItemActive());
                node.Size = Im.GetItemRectSize() + NodeWindowPadding + NodeWindowPadding;
                ImVec2 nodeRectMax = nodeRectMin + node.Size;

                // Display node box
                drawList.ChannelsSetCurrent(BackgroundChannel);
                Im.SetCursorScreenPos(nodeRectMin);
                Im.InvisibleButton("node", node.Size);

                if (Im.IsItemHovered())
                {
                    node.Hovered = true;

                    if (Im.IsMouseDragging(ImGuiMouseButton.Left))
                    {
                        node.Dragged = true;
                    }

                    if (Im.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        node.Down = true;
                    }
                    
                    if (Im.IsMouseReleased(ImGuiMouseButton.Left) && node.Down)
                    {
                        node.Down = false;
                        node.Selected = !node.Selected;
                        node.Dragged = false;
                    }

                    if (Im.IsMouseReleased(ImGuiMouseButton.Left))
                    {
                        node.Dragged = false;
                    }

                    if (Im.IsMouseDragging(ImGuiMouseButton.Left) && node.Down)
                        node.Down = false;

                    _openContextMenu |= Im.IsMouseReleased(ImGuiMouseButton.Right);
                }

                if (node.Dragged)
                {
                    ImGuiIOPtr io = Im.GetIO();
                    node.Pos += io.MouseDelta;
                }

                if (node.Hovered)
                {
                    drawList.AddRectFilled(nodeRectMin, nodeRectMax, GreyColor, Im.GetStyle().FrameRounding);
                }
                else
                {
                    drawList.AddRectFilled(nodeRectMin, nodeRectMax, DarkGreyColor, Im.GetStyle().FrameRounding);
                }

                if (node.Selected)
                {
                    drawList.AddRect(nodeRectMin, nodeRectMax, White, Im.GetStyle().FrameRounding);
                }
                else
                {
                    drawList.AddRect(nodeRectMin, nodeRectMax, NodeBorderColor, Im.GetStyle().FrameRounding);
                }

                for (int slotIdx = 0; slotIdx < node.InputsCount; slotIdx++)
                {
                    drawList.AddCircleFilled(panningOffset + node.GetInputSlotPos(slotIdx), NodeSlotRadius, 0xffffffff);
                    drawList.AddCircleFilled(panningOffset + node.GetInputSlotPos(slotIdx), NodeSlotRadius / 2, 0xff777777);
                }

                for (int slotIdx = 0; slotIdx < node.OutputsCount; slotIdx++)
                {
                    drawList.AddCircleFilled(panningOffset + node.GetOutputSlotPos(slotIdx), NodeSlotRadius, 0xffffffff);
                    drawList.AddCircleFilled(panningOffset + node.GetOutputSlotPos(slotIdx), NodeSlotRadius / 2, 0xff777777);
                }

                Im.PopID();
            }

            Im.PopItemWidth();
        }

        private void DisplayNodeLinks(Vector2 panningOffset, ref ImDrawListPtr drawList)
        {
            drawList.ChannelsSetCurrent(BackgroundChannel);

            foreach (var link in _links)
            {
                Node nodeIn = _nodes[link.InputIdx];
                Node nodeOut = _nodes[link.OutputIdx];
                ImVec2 p1 = panningOffset + nodeIn.GetOutputSlotPos(link.InputSlot);
                ImVec2 p2 = panningOffset + nodeOut.GetInputSlotPos(link.OutputSlot);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, LinkBorderColor, LinkDefaultThickness);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, LinkColor, 1);
            }
        }

        private void DisplayGrid(ImDrawListPtr drawList)
        {
            if (_showGrid)
            {
                ImVec2 winPos = Im.GetCursorScreenPos();
                ImVec2 canvasSize = Im.GetWindowSize();

                for (float x = _panningPosition.X % GridSize; x < canvasSize.X; x += GridSize)
                {
                    drawList.AddLine(new ImVec2(x, 0.0f) + winPos, new ImVec2(x, canvasSize.Y) + winPos, 0xff555555);
                }

                for (float y = _panningPosition.Y % GridSize; y < canvasSize.Y; y += GridSize)
                {
                    drawList.AddLine(new ImVec2(0.0f, y) + winPos, new ImVec2(canvasSize.X, y) + winPos, 0xff555555);
                }
            }
        }

        // Draw a list of nodes on the left side
        private void DrawNodeList()
        {
            Im.BeginChild("node_list", new ImVec2(NodeListDefaultWidth, 0));
            Im.Text("Nodes");
            Im.Separator();

            foreach (var node in _nodes)
            {
                Im.PushID(node.Id);

                if (node.Hovered)
                    Im.PushStyleColor(ImGuiCol.Text, White);
                else
                    Im.PushStyleColor(ImGuiCol.Text, LightGrey);

                Im.Selectable(node.Name, node.Selected);

                if (node.Hovered)
                    Im.PopStyleColor();

                if (Im.IsItemHovered())
                {
                    node.Hovered = true;

                    if (Im.IsMouseDown(ImGuiMouseButton.Left))
                        node.Down = true;

                    if (Im.IsMouseReleased(ImGuiMouseButton.Left) && node.Down)
                        node.Selected = !node.Selected;

                    if (Im.IsMouseDragging(ImGuiMouseButton.Left) && node.Down)
                        node.Down = false;

                    _openContextMenu |= Im.IsMouseReleased(ImGuiMouseButton.Right);
                }

                Im.PopID();
            }

            Im.EndChild();
        }
    }
}