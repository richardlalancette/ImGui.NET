using System.Collections.Generic;
using ImGuiNET;

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

        private static void Main(string[] args)
        {
            Setup();
            Run();
        }

        private static void Setup()
        {
            var mainStyleSheet = new StyleSheet();
            mainStyleSheet.Styles.Add("DarkWindowStyle", new DarkWindowStyle());

            MainWindow.ApplyStyleSheet(mainStyleSheet);

            Controllers.Add(MainWindow.Title, MainWindow);
            Controllers.Add(SecondWindow.Title, SecondWindow);
            Controllers.Add(StylesWindow.Title, StylesWindow);
            Controllers.Add(NodeEditorWindow.Title, NodeEditorWindow);
            Controllers.Add(ShaderEditor.Title, ShaderEditor);
            
            NodeEditorWindow.Enabled = false;
            MainWindow.Enabled = false;
            SecondWindow.Enabled = false;
            StylesWindow.Enabled = false;
        }

        private static void Run()
        {
            Device.Create();

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
            foreach (var controller in Controllers)
            {
                controller.Value.Draw();
            }
        }
    }
}