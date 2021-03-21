using System.IO;
using Veldrid;
using Veldrid.SPIRV;

namespace ImGui.NET.SampleProgram
{
  public class ShaderTranspiler
  {
    // https://github.com/mellinoe/veldrid-spirv/tree/master/src/Veldrid.SPIRV.Tests/TestShaders
    static Shader[] Transpile(ResourceFactory factory)
    {
      byte[] vertexShaderSpirvBytes = File.ReadAllBytes("myshader.vert.spv");
      byte[] fragmentShaderSpirvBytes = File.ReadAllBytes("myshader.frag.spv");
      var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexShaderSpirvBytes, "main");
      var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentShaderSpirvBytes, "main");
      return factory.CreateFromSpirv(vertexShaderDescription, fragmentShaderDescription);
    }
  }
}
