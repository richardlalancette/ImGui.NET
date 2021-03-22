using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ImGui.Extensions;
using ImGuiNET;
using Newtonsoft.Json;
using ImVec2 = System.Numerics.Vector2;
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

namespace NodeEditor
{
    public class Graph
    {
        [JsonProperty]
        public List<Node> Nodes = new();

        [JsonProperty]
        public List<NodeLink> Links = new();
    }

    public class ImNodeEditorWindowController : ImWindowController
    {
        [JsonProperty]
        private bool _showGrid = true;

        [JsonProperty]
        private bool _openEditorContextMenu;

        [JsonProperty]
        private bool _openNodeContextMenu;

        [JsonProperty]
        private ImVec2 _panningPosition = ImVec2.Zero;

        private readonly Graph _graph;

        public ImNodeEditorWindowController(string title) : base(title)
        {
            _graph = new Graph();
            _graph.Nodes.Add(new TestNode(0, "MagickImage", new ImVec2(75, 40), new NodeData(), 1, 1));
            _graph.Nodes.Add(new Node(1, "MagickImage", new ImVec2(75, 555), new NodeData(), 1, 1));
            _graph.Nodes.Add(new Node(2, "Composite", new ImVec2(420, 300), new NodeData(), 2, 1));
            _graph.Nodes.Add(new Node(3, "Output", new ImVec2(700, 300), new NodeData(), 1, 0));
            _graph.Links.Add(new NodeLink(0, 0, 2, 0));
            _graph.Links.Add(new NodeLink(1, 0, 2, 1));
            _graph.Links.Add(new NodeLink(2, 0, 3, 0));
        }

        public override void Draw()
        {
            base.Draw();
            
            Im.ShowDemoWindow();
        }

        protected override void DrawWindowElements()
        {
            base.DrawWindowElements();

            foreach (var node in _graph.Nodes)
            {
                node.Hovered = false;
            }

            DrawNodeList(); Im.SameLine(); DrawEditor();
        }

        private void DrawEditor()
        {
            Im.BeginGroup();
            {
                DrawTopBar();

                Im.PushStyleVar(ImGuiStyleVar.FramePadding, ImVec2.One);
                Im.PushStyleVar(ImGuiStyleVar.WindowPadding, ImVec2.Zero);
                Im.PushStyleColor(ImGuiCol.ChildBg, Styles.GreyColor);
                Im.BeginChild("panning_region", ImVec2.Zero, true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
                Im.PopStyleVar(); // WindowPadding

                Vector2 panningOffset = Im.GetCursorScreenPos() + _panningPosition;
                ImDrawListPtr drawList = Im.GetWindowDrawList();
                DisplayGrid(drawList);
                drawList.ChannelsSplit(2);
                DisplayNodeLinks(panningOffset, ref drawList);
                DisplayNodes(panningOffset, ref drawList);
                drawList.ChannelsMerge();
                DrawEditorContextMenu(panningOffset);
                DrawNodeContextMenu(panningOffset);
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

        private void DrawEditorContextMenu(Vector2 panningOffset)
        {
            _openEditorContextMenu = false;

            if (Im.IsMouseReleased(ImGuiMouseButton.Right))
            {
                if (Im.IsWindowHovered(ImGuiHoveredFlags.RootWindow) || !Im.IsAnyItemHovered())
                {
                    _openEditorContextMenu = true;
                }
            }

            if (_openEditorContextMenu)
            {
                Im.OpenPopup("editor_context_menu");
            }

            Im.PushStyleVar(ImGuiStyleVar.WindowPadding, Styles.NodeWindowPadding);

            if (Im.BeginPopup("editor_context_menu"))
            {
                ImVec2 scenePos = Im.GetMousePosOnOpeningCurrentPopup() - panningOffset;

                var nodeTypes = NodeFactory.TypeList();

                foreach (Type type in nodeTypes)
                {
                    if (Im.MenuItem($"Add {type.Name}"))
                    {
                        var nodeInstance = NodeFactory.CreateInstance(type, scenePos, _graph.Nodes.Count);

                        _graph.Nodes.Add(nodeInstance);
                    }
                }

                Im.EndPopup();
            }

            Im.PopStyleVar();
        }

        private void DrawNodeContextMenu(Vector2 panningOffset)
        {
            _openNodeContextMenu = false;

            if (Im.IsMouseReleased(ImGuiMouseButton.Right))
            {
                if (Im.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && Im.IsAnyItemHovered())
                {
                    _openNodeContextMenu = true;
                }
            }

            if (_openNodeContextMenu)
            {
                Im.OpenPopup("node_context_menu");
            }

            Im.PushStyleVar(ImGuiStyleVar.WindowPadding, Styles.NodeWindowPadding);

            if (Im.BeginPopup("node_context_menu"))
            {
                ImVec2 scenePos = Im.GetMousePosOnOpeningCurrentPopup() - panningOffset;

                if (Im.MenuItem("Node ContextMenu"))
                {
                }

                Im.EndPopup();
            }

            Im.PopStyleVar();
        }
        
        private void DisplayNodes(Vector2 panningOffset, ref ImDrawListPtr drawList)
        {
            for (int nodeIdx = 0; nodeIdx < _graph.Nodes.Count; nodeIdx++)
            {
                var node = _graph.Nodes[nodeIdx];

                node.Draw(panningOffset, drawList, nodeIdx, ref _openEditorContextMenu);
            }
        }

        private void DisplayNodeLinks(Vector2 panningOffset, ref ImDrawListPtr drawList)
        {
            drawList.ChannelsSetCurrent(Styles.BackgroundChannel);

            foreach (var link in _graph.Links)
            {
                Node nodeIn = _graph.Nodes[link.InputIdx];
                Node nodeOut = _graph.Nodes[link.OutputIdx];
                ImVec2 p1 = panningOffset + nodeIn.GetOutputSlotPos(link.InputSlot);
                ImVec2 p2 = panningOffset + nodeOut.GetInputSlotPos(link.OutputSlot);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, Styles.LinkBorderColor, Styles.LinkDefaultThickness);
                drawList.AddBezierCurve(p1, p1 + new ImVec2(+20, 0), p2 + new ImVec2(-20, 0), p2, Styles.LinkColor, 1);
            }
        }

        private void DisplayGrid(ImDrawListPtr drawList)
        {
            if (_showGrid)
            {
                ImVec2 winPos = Im.GetCursorScreenPos();
                ImVec2 canvasSize = Im.GetWindowSize();

                for (float x = _panningPosition.X % Styles.GridSize; x < canvasSize.X; x += Styles.GridSize)
                {
                    drawList.AddLine(new ImVec2(x, 0.0f) + winPos, new ImVec2(x, canvasSize.Y) + winPos, 0xff555555);
                }

                for (float y = _panningPosition.Y % Styles.GridSize; y < canvasSize.Y; y += Styles.GridSize)
                {
                    drawList.AddLine(new ImVec2(0.0f, y) + winPos, new ImVec2(canvasSize.X, y) + winPos, 0xff555555);
                }
            }
        }

        // Draw a list of nodes on the left side
        private void DrawNodeList()
        {
            Im.BeginChild("node_list", new ImVec2(Styles.NodeListDefaultWidth, 0));
            Im.Text("Nodes");
            Im.Separator();

            if (Im.IsMouseReleased(ImGuiMouseButton.Left))
            {
                foreach (var node in _graph.Nodes)
                {
                    node.LeftMouseButtonDown = false;
                }
            }

            foreach (var node in _graph.Nodes)
            {
                Im.PushID(node.Id);

                if (node.Hovered)
                {
                    Im.PushStyleColor(ImGuiCol.Text, Styles.White);
                }
                else
                {
                    Im.PushStyleColor(ImGuiCol.Text, Styles.LightGrey);
                }

                Im.Selectable(node.Name, node.Selected);

                if (node.Hovered)
                {
                    Im.PopStyleColor();
                }

                if (Im.IsItemHovered(ImGuiHoveredFlags.RectOnly))
                {
                    node.Hovered = true;

                    if (Im.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        node.LeftMouseButtonDown = true;
                    }

                    if (Im.IsMouseReleased(ImGuiMouseButton.Left) && node.LeftMouseButtonDown)
                    {
                        node.Selected = !node.Selected;
                        node.LeftMouseButtonDown = false;
                    }

                    if (Im.IsMouseDragging(ImGuiMouseButton.Left) && node.LeftMouseButtonDown)
                    {
                        node.LeftMouseButtonDown = false;
                    }

                    if (Im.IsMouseReleased(ImGuiMouseButton.Right))
                    {
                        _openNodeContextMenu = true;
                    }
                }

                Im.PopID();
            }

            Im.EndChild();
        }
    }

    public class CompositeOperationNode : Node
    {
        public CompositeOperationNode()
        {
        }

        public CompositeOperationNode(int id, string name, Vector2 pos, NodeData data, int inputsCount, int outputsCount) : base(id, name, pos, data, inputsCount, outputsCount)
        {
            data.Add("Tint", Color.Salmon);
        }
    }

    public class ImageNode : Node
    {
        public ImageNode(int id, string name, Vector2 pos, NodeData data, int inputsCount, int outputsCount) : base(id, name, pos, data, inputsCount, outputsCount)
        {
            data.Add("Color", Color.Salmon);
        }
    }

    public class TestNode : Node
    {
        public TestNode(int id, string name, Vector2 pos, NodeData data, int inputsCount, int outputsCount) : base(id, name, pos, data, inputsCount, outputsCount)
        {
            data.Add("Color", Color.Salmon);
            data.Add("Color2", Color.Red);
            data.Add("Color3", Color.FromArgb(202, 255, 50));
            data.Add("v2", new Vector2());
            data.Add("v3", new Vector3());
            data.Add("v4", new Vector4());
        }
    }
}