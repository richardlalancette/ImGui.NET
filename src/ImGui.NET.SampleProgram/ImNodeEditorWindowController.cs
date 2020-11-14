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
        private const float LinkDefaultThickness = 3.0f;

        private List<Node> _nodes = new List<Node>();
        private List<NodeLink> _links = new List<NodeLink>();
        private bool _showGrid = true;
        private int _nodeSelectedId = -1;
        private bool _openContextMenu;
        private int _nodeHoveredInListId = -1;
        private int _nodeHoveredInSceneId = -1;

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
            uint color1 = Im.GetColorU32(new Vector4(0.23f, 0.23f, 0.27f, 0.8f));
            uint greyColor = Im.GetColorU32(new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 255.0f / 255.0f));
            uint lightGreyColor = Im.GetColorU32(new Vector4(60.0f / 255.0f, 60.0f / 255.0f, 60.0f / 255.0f, 255.0f / 255.0f));
            uint lightGrey2Color = Im.GetColorU32(new Vector4(75.0f / 255.0f, 75.0f / 255.0f, 75.0f / 255.0f, 255.0f / 255.0f));

            base.DrawWindowElements();

            DrawNodeList();

            Im.SameLine();

            DrawEditorCanvas(color1, lightGrey2Color, lightGreyColor, greyColor);
        }

        private void DrawEditorCanvas(uint color1, uint lightGrey2Color, uint lightGreyColor, uint greyColor)
        {
            Im.BeginGroup();

            // Create our child canvas
            Im.Text($"Hold middle mouse button to scroll {_panningPosition.X}, {_panningPosition.Y})");
            Im.SameLine();
            Im.Checkbox("Show grid", ref _showGrid);

            Im.PushStyleVar(ImGuiStyleVar.FramePadding, ImVec2.One);
            Im.PushStyleVar(ImGuiStyleVar.WindowPadding, ImVec2.Zero);
            Im.PushStyleColor(ImGuiCol.ChildBg, color1);
            Im.BeginChild("scrolling_region", ImVec2.Zero, true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
            Im.PopStyleVar(); // WindowPadding

            Vector2 panningOffset = Im.GetCursorScreenPos() + _panningPosition;
            ImDrawListPtr drawList = Im.GetWindowDrawList();
            DisplayGrid(GridSize, drawList);
            drawList.ChannelsSplit(2);
            DisplayNodeLinks(ref panningOffset, ref drawList);
            DisplayNodes(ref panningOffset, ref drawList, lightGrey2Color, lightGreyColor, greyColor);
            drawList.ChannelsMerge();
            DrawContextMenu(panningOffset);
            HandlePanning();

            Im.EndChild();
            Im.PopStyleColor();
            Im.PopStyleVar();
            Im.EndGroup();
        }

        private void HandlePanning()
        {
            if (Im.IsWindowHovered() && !Im.IsAnyItemActive() && Im.IsMouseDragging(ImGuiMouseButton.Middle, 0.0f))
            {
                ImGuiIOPtr io = Im.GetIO();
                _panningPosition += io.MouseDelta;
            }
        }

        private void DrawContextMenu(Vector2 scrollingOffset)
        {
            _openContextMenu = false;

            if (Im.IsMouseReleased(ImGuiMouseButton.Right))
            {
                if (Im.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup) || !Im.IsAnyItemHovered())
                {
                    _nodeSelectedId = -1;
                    _nodeHoveredInListId = -1;
                    _nodeHoveredInSceneId = -1;
                    _openContextMenu = true;
                }
            }

            if (_openContextMenu)
            {
                Im.OpenPopup("context_menu");

                if (_nodeHoveredInListId != -1)
                {
                    _nodeSelectedId = _nodeHoveredInListId;
                }

                if (_nodeHoveredInSceneId != -1)
                {
                    _nodeSelectedId = _nodeHoveredInSceneId;
                }
            }

            Im.PushStyleVar(ImGuiStyleVar.WindowPadding, NodeWindowPadding);

            if (Im.BeginPopup("context_menu"))
            {
                Node node = null;

                if (_nodeSelectedId != -1)
                {
                    node = _nodes[_nodeSelectedId];
                }

                ImVec2 scenePos = Im.GetMousePosOnOpeningCurrentPopup() - scrollingOffset;

                if (node != null)
                {
                    Im.Text($"Node {node.Name}");
                    Im.Separator();

                    if (Im.MenuItem("Rename..", null, false, false))
                    {
                    }

                    if (Im.MenuItem("Delete", null, false, false))
                    {
                    }

                    if (Im.MenuItem("Copy", null, false, false))
                    {
                    }
                }
                else
                {
                    if (Im.MenuItem("Add"))
                    {
                        _nodes.Add(new Node(_nodes.Count, "New node", scenePos, 0.5f, new ImVec4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 255.0f / 255.0f), 2, 2));
                    }

                    if (Im.MenuItem("Paste", null, false, false))
                    {
                    }
                }

                Im.EndPopup();
            }

            Im.PopStyleVar();
        }

        private void DisplayNodes(ref Vector2 scrollingOffset, ref ImDrawListPtr drawList, uint lightGrey2Color, uint lightGreyColor, uint greyColor)
        {
            Im.PushItemWidth(DefaultNodeWidth);

            for (int nodeIdx = 0; nodeIdx < _nodes.Count; nodeIdx++)
            {
                var node = _nodes[nodeIdx];

                Im.PushID(node.Id);
                ImVec2 nodeRectMin = scrollingOffset + node.Pos;

                // Display node contents first
                drawList.ChannelsSetCurrent(ForegroundChannel);
                bool oldAnyActive = Im.IsAnyItemActive();
                Im.SetCursorScreenPos(nodeRectMin + NodeWindowPadding);
                Im.BeginGroup(); // Lock horizontal position
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
                    _nodeHoveredInSceneId = node.Id;
                    _openContextMenu |= Im.IsMouseClicked(ImGuiMouseButton.Right);
                }

                bool nodeMovingActive = Im.IsItemActive();

                if (nodeWidgetsActive || nodeMovingActive)
                {
                    _nodeSelectedId = node.Id;
                }

                if (nodeMovingActive && Im.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    ImGuiIOPtr io = Im.GetIO();
                    node.Pos += io.MouseDelta;
                }

                uint nodeBgColor;

                if (_nodeHoveredInListId == node.Id || _nodeHoveredInSceneId == node.Id || (_nodeHoveredInListId == -1 && _nodeSelectedId == node.Id))
                {
                    nodeBgColor = 0xff666666;
                }
                else
                {
                    nodeBgColor = lightGrey2Color;
                }

                drawList.AddRectFilled(nodeRectMin, nodeRectMax, nodeBgColor, Im.GetStyle().FrameRounding);
                drawList.AddRect(nodeRectMin, nodeRectMax, 0xffaaaaaa, Im.GetStyle().FrameRounding);

                for (int slotIdx = 0; slotIdx < node.InputsCount; slotIdx++)
                {
                    drawList.AddCircleFilled(scrollingOffset + node.GetInputSlotPos(slotIdx), NodeSlotRadius, 0xffffffff);
                    drawList.AddCircleFilled(scrollingOffset + node.GetInputSlotPos(slotIdx), NodeSlotRadius / 2, 0xff777777);
                }

                for (int slotIdx = 0; slotIdx < node.OutputsCount; slotIdx++)
                {
                    drawList.AddCircleFilled(scrollingOffset + node.GetOutputSlotPos(slotIdx), NodeSlotRadius, 0xffffffff);
                    drawList.AddCircleFilled(scrollingOffset + node.GetOutputSlotPos(slotIdx), NodeSlotRadius / 2, 0xff777777);
                }

                Im.PopID();
            }

            Im.PopItemWidth();
        }

        private void DisplayNodeLinks(ref Vector2 scrollingOffset, ref ImDrawListPtr drawList)
        {
            drawList.ChannelsSetCurrent(BackgroundChannel);

            foreach (var link in _links)
            {
                Node nodeIn = _nodes[link.InputIdx];
                Node nodeOut = _nodes[link.OutputIdx];
                ImVec2 p1 = scrollingOffset + nodeIn.GetOutputSlotPos(link.InputSlot);
                ImVec2 p2 = scrollingOffset + nodeOut.GetInputSlotPos(link.OutputSlot);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, 0xffdddddd, LinkDefaultThickness * 2);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, 0xff222222, LinkDefaultThickness / 2);
            }
        }

        private void DisplayGrid(float gridSize, ImDrawListPtr drawList)
        {
            if (_showGrid)
            {
                ImVec2 winPos = Im.GetCursorScreenPos();
                ImVec2 canvasSize = Im.GetWindowSize();

                for (float x = _panningPosition.X % gridSize; x < canvasSize.X; x += gridSize)
                {
                    drawList.AddLine(new ImVec2(x, 0.0f) + winPos, new ImVec2(x, canvasSize.Y) + winPos, 0xff555555);
                }

                for (float y = _panningPosition.Y % gridSize; y < canvasSize.Y; y += gridSize)
                {
                    drawList.AddLine(new ImVec2(0.0f, y) + winPos, new ImVec2(canvasSize.X, y) + winPos, 0xff555555);
                }
            }
        }

        // Draw a list of nodes on the left side
        private void DrawNodeList()
        {
            Im.BeginChild("node_list", new ImVec2(100, 0));
            Im.Text("Nodes");
            Im.Separator();

            foreach (var node in _nodes)
            {
                Im.PushID(node.Id);

                if (Im.Selectable(node.Name, node.Id == _nodeSelectedId))
                {
                    _nodeSelectedId = node.Id;
                }

                if (Im.IsItemHovered())
                {
                    _nodeHoveredInListId = node.Id;
                    _openContextMenu |= Im.IsMouseClicked(ImGuiMouseButton.Right);
                }

                Im.PopID();
            }

            Im.EndChild();
        }
    }
}