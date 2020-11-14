namespace ImGui.NET.SampleProgram
{
    internal static class Program
    {
        private static readonly Device Device = new Device();
        private static readonly AbstractImController MainUiController = new DemoController();

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
            MainUiController.Draw();
        }
    }
}