using System.Numerics;
using ImGuiNET;
using Im = ImGuiNET.ImGui;

namespace ImGui.NET.SampleProgram
{
    internal class ImShaderEditor : ImWindowController
    {
        private string _glslShader = new("");
        
        public ImShaderEditor(string title) : base(title)
        {
        }
        public ImShaderEditor(string title, int width, int height) : base(title, width, height)
        {
        }
        public ImShaderEditor(string title, int width, int height, ImGuiCond sizeCondition) : base(title, width, height, sizeCondition)
        {
        }
        public ImShaderEditor(string title, int width, int height, ImGuiCond sizeCondition, ImGuiWindowFlags windowStyle) : base(title, width, height, sizeCondition, windowStyle)
        {
        }

        protected override void DrawWindowElements()
        {
            base.DrawWindowElements();
            
            Im.InputTextMultiline("GlslShader", ref _glslShader, 65536, new Vector2(400,400));
        }
    }
}
