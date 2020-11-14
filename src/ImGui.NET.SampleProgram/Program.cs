using System.Collections.Generic;
using ImGuiNET;

namespace ImGui.NET.SampleProgram
{
    internal static class Program
    {
        private static readonly Device Device = new Device();
        private static readonly ImWindowController MainWindow = new ImWindowController("Main");
        private static readonly ImWindowController SecondWindow = new ImWindowController("Second");
        private static readonly ImWindowController StylesWindow = new ImStyleWindowController("Styles", 400, 1024, ImGuiCond.Once);
        private static readonly ImWindowController NodeEditorWindow = new ImNodeEditorWindowController("NodeEditor");
        private static Dictionary<string, ImWindowController> Controllers { get; set; } = new Dictionary<string, ImWindowController>();

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