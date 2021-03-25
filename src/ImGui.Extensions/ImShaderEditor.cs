using System.Numerics;
using System.Text;
using ImGuiNET;
using Veldrid;
using Veldrid.SPIRV;
using Im = ImGuiNET.ImGui;

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

		private string _spirvVertexShader = VertexCode;
		private string _spirvFragmentShader = FragmentCode;
		private readonly string[] _vertexShader = new string[4];
		private readonly string[] _fragmentShader = new string[4];

		private readonly CrossCompileTarget[] _targets = 
		{
				CrossCompileTarget.HLSL,
				CrossCompileTarget.GLSL,
				CrossCompileTarget.ESSL,
				CrossCompileTarget.MSL
		};

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

			int id = 0;
			Im.InputTextMultiline("Spriv Vertex Shader", ref _spirvVertexShader, 65536, new Vector2(400, 100));

			foreach(var vertexShader in _vertexShader)
			{
				var vsr = vertexShader;
				if (vsr != null)
				{
					Im.InputTextMultiline(id + ":" + _targets[id], ref vsr, 65536, new Vector2(400, 100));
					id++;
				}
			}

			Im.InputTextMultiline("Spriv Fragment Shader", ref _spirvFragmentShader, 65536, new Vector2(400, 100));
			foreach(var fragmentShader in _fragmentShader)
			{
				var fsr = fragmentShader;
				if (fsr != null)
				{
					Im.InputTextMultiline(id + ":" + _targets[id%4], ref fsr, 65536, new Vector2(400, 100));
					id++;
				}
			}
		}
		
		private void ConvertAll()
		{
			VertexFragmentCompilationResult result = null;
			byte[] vsBytes = Encoding.UTF8.GetBytes(_spirvVertexShader);
			byte[] fsBytes = Encoding.UTF8.GetBytes(_spirvFragmentShader);
			int i = 0;

			foreach(var target in _targets)
			{
				try
				{
					SpecializationConstant[] specializations = {
							new(100, 125u),
							new(101, true),
							new(102, 0.75f),
					};

					result = SpirvCompilation.CompileVertexFragment(
						vsBytes,
						fsBytes,
						target,
						new CrossCompileOptions(false, false, specializations));
				}
				catch
				{
					_vertexShader[i] = "Error";
					_fragmentShader[i] = "Error ";
				}
				finally
				{
					if (result != null)
					{
						_vertexShader[i] = result.VertexShader;
						_fragmentShader[i] = result.FragmentShader;
					}
					i++;
				}
			}
		}
	}
}
