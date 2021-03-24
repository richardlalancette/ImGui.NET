using System.Collections.Generic;
using ImGui.Extensions;
using ImGui.NET.NodeEditor;
using ImGuiNET;
using NodeEditor;
using System;
using System.Threading.Tasks;
using Grpc.Net.Client;

// http://shader-playground.timjones.io/#
namespace ImGui.NET.SampleProgram
{
	internal static class Program
	{
		private static readonly Device Device = new();
		private static readonly ImWindowController MainWindow = new("Main");
		private static readonly ImWindowController SecondWindow = new("Second");
		private static readonly ImStyleWindowController StylesWindow = new("Styles", 400, 800, ImGuiCond.Once);
		private static readonly ImNodeEditorWindowController NodeEditorWindow = new("NodeEditor");
		private static readonly ImShaderEditor ShaderEditor = new("Shader Editor", 400, 800, ImGuiCond.Appearing);
		private static Dictionary<string, ImWindowController> Controllers { get; set; } = new();

		private static void Main()
		{
			Setup();
			Run();
		}
		
		private static async Task gRPCExample()
		{
			using var channel = GrpcChannel.ForAddress("https://localhost:5001");
			var client = new Greeter.GreeterClient(channel);
			var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
			Console.WriteLine(reply);
		}

		private static void Setup()
		{
			var mainStyleSheet = new StyleSheet();
			mainStyleSheet.Styles.Add("DarkWindowStyle", new DarkWindowStyle());

			MainWindow.ApplyStyleSheet(mainStyleSheet);
			SecondWindow.ApplyStyleSheet(mainStyleSheet);
			StylesWindow.ApplyStyleSheet(mainStyleSheet);
			NodeEditorWindow.ApplyStyleSheet(mainStyleSheet);
			ShaderEditor.ApplyStyleSheet(mainStyleSheet);
			
			Controllers.Add(MainWindow.Title, MainWindow);
			Controllers.Add(SecondWindow.Title, SecondWindow);
			Controllers.Add(StylesWindow.Title, StylesWindow);
			Controllers.Add(NodeEditorWindow.Title, NodeEditorWindow);
			Controllers.Add(ShaderEditor.Title, ShaderEditor);

			// NodeEditorWindow.Enabled = false;
			// MainWindow.Enabled = false;
			// SecondWindow.Enabled = false;
			// StylesWindow.Enabled = false;

			// The port number(5001) must match the port of the gRPC server.
		}

		private static void Run()
		{
			Device.Create();
			ShaderEditor.Resource = Device.ResourceFactory();

			while (Device.Exists())
			{
				if (Device.Begin())
				{
					Draw();
				}

				Device.End();
			}

			Device.Shutdown();
		}

		private static void Draw()
		{
			foreach(var controller in Controllers)
			{
				controller.Value.Draw();
			}
		}
	}
}
