using ImGuiNET;

namespace ImGui.NET.SampleProgram
{
    internal static class Program
    {
        private static readonly Device Device = new Device();
        private static readonly ImWindowController MainWindow = new ImWindowController("Main");
        private static readonly ImWindowController SecondWindow = new ImWindowController("Second");
        private static readonly ImWindowController ThirdWindow = new ImWindowController("Third", 1024, 768, ImGuiCond.Always);

        private static void Main(string[] args)
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
            MainWindow.Draw();
            SecondWindow.Draw();
            ThirdWindow.Draw();
        }
    }
}