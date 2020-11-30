using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ImageMagick;
using ImGuiNET;
using Newtonsoft.Json;
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
        [JsonProperty]
        private List<Node> _nodes = new List<Node>();

        [JsonProperty]
        private List<NodeLink> _links = new List<NodeLink>();

        [JsonProperty]
        private bool _showGrid = true;

        [JsonProperty]
        private bool _openContextMenu;

        [JsonProperty]
        private ImVec2 _panningPosition = ImVec2.Zero;

        public ImNodeEditorWindowController(string title) : base(title)
        {
            var firstNodeData = new NodeData();
            firstNodeData.AddField("Vector4", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            firstNodeData.AddField("MagickImage", new MagickImage(MagickColors.White, 10, 10));
            _nodes.Add(new Node(0, "MagickImage", new ImVec2(75, 40), firstNodeData, new ImVec4(1.0f, 0.4f, 0.4f, 1.0f), 1, 1));
            _nodes.Add(new Node(1, "MagickImage", new ImVec2(75, 555), new NodeData(), new ImVec4(0.8f, 0.4f, 0.8f, 1.0f), 1, 1));
            _nodes.Add(new Node(2, "Composite", new ImVec2(420, 300), new NodeData(), new ImVec4(0, 0.8f, 0.4f, 1.0f), 2, 1));
            _nodes.Add(new Node(3, "Output", new ImVec2(700, 300), new NodeData(), new ImVec4(0, 0.8f, 0.4f, 1.0f), 1, 0));
            _links.Add(new NodeLink(0, 0, 2, 0));
            _links.Add(new NodeLink(1, 0, 2, 1));
            _links.Add(new NodeLink(2, 0, 3, 0));
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
                Im.PushStyleColor(ImGuiCol.ChildBg, StyleSheet.GreyColor);
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

            Im.PushStyleVar(ImGuiStyleVar.WindowPadding, StyleSheet.NodeWindowPadding);

            if (Im.BeginPopup("context_menu"))
            {
                ImVec2 scenePos = Im.GetMousePosOnOpeningCurrentPopup() - panningOffset;

                if (Im.MenuItem("Add"))
                {
                    _nodes.Add(new Node(_nodes.Count, "New node", scenePos, new NodeData(), new ImVec4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 255.0f / 255.0f), 2, 2));
                }

                Im.EndPopup();
            }

            Im.PopStyleVar();
        }

        private void DisplayNodes(Vector2 panningOffset, ref ImDrawListPtr drawList)
        {
            for (int nodeIdx = 0; nodeIdx < _nodes.Count; nodeIdx++)
            {
                var node = _nodes[nodeIdx];

                node.Draw(panningOffset, drawList, nodeIdx, ref _openContextMenu);
            }
        }

        private void DisplayNodeLinks(Vector2 panningOffset, ref ImDrawListPtr drawList)
        {
            drawList.ChannelsSetCurrent(StyleSheet.BackgroundChannel);

            foreach (var link in _links)
            {
                Node nodeIn = _nodes[link.InputIdx];
                Node nodeOut = _nodes[link.OutputIdx];
                ImVec2 p1 = panningOffset + nodeIn.GetOutputSlotPos(link.InputSlot);
                ImVec2 p2 = panningOffset + nodeOut.GetInputSlotPos(link.OutputSlot);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, StyleSheet.LinkBorderColor, StyleSheet.LinkDefaultThickness);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, StyleSheet.LinkColor, 1);
            }
        }

        private void DisplayGrid(ImDrawListPtr drawList)
        {
            if (_showGrid)
            {
                ImVec2 winPos = Im.GetCursorScreenPos();
                ImVec2 canvasSize = Im.GetWindowSize();

                for (float x = _panningPosition.X % StyleSheet.GridSize; x < canvasSize.X; x += StyleSheet.GridSize)
                {
                    drawList.AddLine(new ImVec2(x, 0.0f) + winPos, new ImVec2(x, canvasSize.Y) + winPos, 0xff555555);
                }

                for (float y = _panningPosition.Y % StyleSheet.GridSize; y < canvasSize.Y; y += StyleSheet.GridSize)
                {
                    drawList.AddLine(new ImVec2(0.0f, y) + winPos, new ImVec2(canvasSize.X, y) + winPos, 0xff555555);
                }
            }
        }

        // Draw a list of nodes on the left side
        private void DrawNodeList()
        {
            Im.BeginChild("node_list", new ImVec2(StyleSheet.NodeListDefaultWidth, 0));
            Im.Text("Nodes");
            Im.Separator();

            foreach (var node in _nodes)
            {
                Im.PushID(node.Id);

                if (node.Hovered)
                    Im.PushStyleColor(ImGuiCol.Text, StyleSheet.White);
                else
                    Im.PushStyleColor(ImGuiCol.Text, StyleSheet.LightGrey);

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