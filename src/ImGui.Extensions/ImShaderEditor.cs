using System.Numerics;
using System.Text;
using ImGuiNET;
using Veldrid;
using Veldrid.SPIRV;
using Im = ImGuiNET.ImGui;

namespace ImGui.Extensions
{
	public class ImShaderEditor : ImWindowController
	{
		private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

		private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

		public ResourceFactory Resource
		{
			get;
			set;
		}
		private string _spirvVertexShader = VertexCode;
		private string _spirvFragmentShader = FragmentCode;
		private string _hlslVertexShader = new("");
		private string _hlslFragmentShader = new("");

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

			if (Im.Button("Convert"))
			{
				ConvertAll();
			}

			Im.InputTextMultiline("Spriv Vertex Shader", ref _spirvVertexShader, 65536, new Vector2(400, 200));
			Im.InputTextMultiline("Spriv Fragment Shader", ref _spirvFragmentShader, 65536, new Vector2(400, 200));
			Im.InputTextMultiline("Glsl Vertex Shader", ref _hlslVertexShader, 65536, new Vector2(400, 200));
			Im.InputTextMultiline("Glsl Fragment  Shader", ref _hlslFragmentShader, 65536, new Vector2(400, 200));
		}
		/*!
        case GraphicsBackend.Direct3D11:
            return CrossCompileTarget.HLSL;
        case GraphicsBackend.OpenGL:
            return CrossCompileTarget.GLSL;
        case GraphicsBackend.Metal:
            return CrossCompileTarget.MSL;
        case GraphicsBackend.OpenGLES:
            return CrossCompileTarget.ESSL;
        */
		private void ConvertAll()
		{
			ResourceFactory resourceFactory = Resource;
			ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(_spirvVertexShader), "main");
			ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(_spirvFragmentShader), "main");
			Shader[] shaders = null;
			try
			{
				shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
			}
			catch
			{
				_hlslVertexShader = "Error";
				_hlslFragmentShader = "Error ";
			}
			finally
			{
				if (shaders != null && shaders.Length == 2)
				{
					_hlslVertexShader = shaders[1].ToString(); // TODO have yet to find a way to take the compiled version and turn back to text :)
					_hlslFragmentShader = shaders[0].ToString();
				}
			}

			// byte[] vertexShaderSpirvBytes = File.ReadAllBytes("myshader.vert.spv");
			// byte[] fragmentShaderSpirvBytes = File.ReadAllBytes("myshader.frag.spv");
			// var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexShaderSpirvBytes, "main");
			// var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentShaderSpirvBytes, "main");
			// var shaders = resourceFactory.CreateFromSpirv(vertexShaderDescription, fragmentShaderDescription);

		}
	}
}
