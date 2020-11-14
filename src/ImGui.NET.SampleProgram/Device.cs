using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ImGui.NET.SampleProgram
{
    class Device
    {
        public Vector3 ClearColor = new Vector3(0.33f, 0.33f, 0.33f);

        private Sdl2Window _window;
        private GraphicsDevice _gd;
        private CommandList _cl;
        private ImGuiController _controller;

        public void Create()
        {
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

            // Feed the input events to our ImGui controller, which passes them through to ImGui.
            _controller.Update(1f / 60f, snapshot);

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
}