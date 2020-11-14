using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ImGui.NET.SampleProgram
{
    internal class ImController
    {
        private bool _enabled = true;

        public void Draw()
        {
            // 3. Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
            if (_enabled)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                ImGuiNET.ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGuiNET.ImGui.ShowDemoWindow(ref _enabled);
            }
        }
    }

    class Device
    {
        protected Sdl2Window _window;
        protected GraphicsDevice _gd;
        protected CommandList _cl;
        protected ImGuiController _controller;
        protected readonly Vector3 ClearColor = new Vector3(0.45f, 0.55f, 0.6f);

        public void Create()
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImGui.NET Sample Program"),
                new GraphicsDeviceOptions(true, null, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint) _window.Width, (uint) _window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
        }

        public void Shutdown()
        {
            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        public bool Begin()
        {
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists)
            {
                return false;
            }

            _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.
            return true;
        }

        public void End()
        {
            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }


        public bool Exists()
        {
            return _window.Exists;
        }
    };

    internal static class Program
    {
        private static readonly Device Device = new Device();

        private static void Main(string[] args)
        {
            ImController mainUiController = new ImController();

            Device.Create();

            while (Device.Exists())
            {
                if (Device.Begin())
                {
                    mainUiController.Draw();
                }
                
                Device.End();
            }

            Device.Shutdown();
        }
    }
}