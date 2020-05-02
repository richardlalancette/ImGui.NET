using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RtMidi.Core;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using static ImGuiNET.ImGuiNative;

using RtMidi.Core.Devices;
using RtMidi.Core.Messages;

namespace ImGuiNET
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        private static MemoryEditor _memoryEditor;

        // UI state
        private static float _f = 0.0f;
        private static int _counter = 0;
        private static int _dragInt = 0;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showDemoWindow = true;
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;
        private static byte[] _memoryEditorData;
        private static uint s_tab_bar_flags = (uint) ImGuiTabBarFlags.Reorderable;
        static bool[] s_opened = {true, true, true, true}; // Persistent user state
        private static readonly HttpClient client = new HttpClient();
        private static Player _player = null;

        internal class Player
        {
            internal class character
            {
                public string Name = default;
            }

            internal class characterConfiguration
            {
            }

            internal class characterData
            {
            }

            public character Character = new character();
            public characterConfiguration CharacterConfiguration = new characterConfiguration();
            public characterData CharacterData = new characterData();
        }

        static void SetThing(out float i, float val)
        {
            i = val;
        }

        static void Main(string[] args)
        {
            foreach (var api in MidiDeviceManager.Default.GetAvailableMidiApis())
                Console.WriteLine($"Available API: {api}");
                
            // Listen to all available midi devices
            void ControlChangeHandler(IMidiInputDevice sender, in ControlChangeMessage msg)
            {
                Console.WriteLine($"[{sender.Name}] ControlChange: Channel:{msg.Channel} Control:{msg.Control} Value:{msg.Value}");
            }

            void ChannelPressureMessageHandler(IMidiInputDevice sender, in ChannelPressureMessage msg)
            {
                Console.WriteLine($"[{sender.Name}] Channel Pressure Message: Channel:{msg.Channel} Pressure:{msg.Pressure}");
            }
            
            void NoteOnHandler(IMidiInputDevice sender, in NoteOnMessage msg)
            {
                Console.WriteLine($"[{sender.Name}] Note On Channel:{msg.Channel} Key:{msg.Key} Velocity:{msg.Velocity}");
            }
            
            void NoteOffHandler(IMidiInputDevice sender, in NoteOffMessage msg)
            {
                Console.WriteLine($"[{sender.Name}] Note Off - Channel:{msg.Channel} Key:{msg.Key} Velocity:{msg.Velocity}");
            }
            
            void NrpnHandler(IMidiInputDevice sender, in NrpnMessage msg)
            {
                Console.WriteLine($"[{sender.Name}] Nrpn - Channel:{msg.Channel} Parameter:{msg.Parameter} Value:{msg.Value}");
            }
            
            void ProgramChangeHandler(IMidiInputDevice sender, in ProgramChangeMessage msg)
            {
                Console.WriteLine($"[{sender.Name}] Note Program Change - Channel:{msg.Channel} Program:{msg.Program}");
            }
            
            var devices = new List<IMidiInputDevice>();
            
            foreach (var inputDeviceInfo in MidiDeviceManager.Default.InputDevices)
            {
                Console.WriteLine($"Opening {inputDeviceInfo.Name}");

                var inputDevice = inputDeviceInfo.CreateDevice();
                devices.Add(inputDevice);
                    
                inputDevice.ControlChange += ControlChangeHandler;
                inputDevice.ChannelPressure += ChannelPressureMessageHandler;
                inputDevice.Nrpn += NrpnHandler;
                inputDevice.NoteOn += NoteOnHandler;
                inputDevice.NoteOff += NoteOffHandler;
                inputDevice.ProgramChange += ProgramChangeHandler;
                inputDevice.Open();
            } 
                
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
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width,
                _window.Height);
            _memoryEditor = new MemoryEditor();
            Random random = new Random();
            _memoryEditorData = Enumerable.Range(0, 1024).Select(i => (byte) random.Next(255)).ToArray();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists)
                {
                    break;
                }

                _controller.Update(1f / 60f,
                    snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                DnDClientUI();
                // SubmitUI();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        private static async Task LoadPlayer(string url)
        {
            //Create a new instance of HttpClient
            using (HttpClient c = new HttpClient())
            {
                //Setting up the response... 
                c.DefaultRequestHeaders.Accept.Clear();
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"));
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");
                
                using (HttpResponseMessage res = await c.GetAsync("http://www.dndbeyond.com/profile/TocOfDarnasus/characters/26466854/json"))
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    if (data != null)
                    {
                        Console.WriteLine(data);
                    }
                }
            }

            // var streamTask =
                // client.GetStreamAsync("https://www.dndbeyond.com/profile/TocOfDarnasus/characters/26466854/json");
            // _player = await JsonSerializer.DeserializeAsync<Player>(await streamTask);
            // Console.Write(_player.ToString());
        }

        private static unsafe void DnDClientUI()
        {
            if (ImGui.BeginTabBar("test"))
            {
                if (ImGui.BeginTabItem("MK2"))
                {
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Movement"))
                {
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Combat"))
                {
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Player"))
                {
                    if (_player != null)
                    {
                        ImGui.Text(_player.Character.Name); // Display some text (you can use a format string too)
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Settings"))
                {
                    string url = new string("https://www.dndbeyond.com/profile/TocOfDarnasus/characters/26466854/json");
                    ImGui.InputText("url", ref url, 100);
                    if (ImGui.Button("Load"))
                    {
                        LoadPlayer(url);
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static unsafe void SubmitUI()
        {
            // Demo code adapted from the official Dear ImGui demo program:
            // https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172

            // 1. Show a simple window.
            // Tip: if we don't call ImGui.BeginWindow()/ImGui.EndWindow() the widgets automatically appears in a window called "Debug".
            {
                ImGui.Text("Hello, world!"); // Display some text (you can use a format string too)
                ImGui.SliderFloat("float", ref _f, 0, 1, _f.ToString("0.000"),
                    1); // Edit 1 float using a slider from 0.0f to 1.0f    
                //ImGui.ColorEdit3("clear color", ref _clearColor);                   // Edit 3 floats representing a color

                ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");

                ImGui.Checkbox("Demo Window", ref _showDemoWindow); // Edit bools storing our windows open/close state
                ImGui.Checkbox("Another Window", ref _showAnotherWindow);
                ImGui.Checkbox("Memory Editor", ref _showMemoryEditor);
                if (ImGui.Button("Button")
                ) // Buttons return true when clicked (NB: most widgets return true when edited/activated)
                    _counter++;
                ImGui.SameLine(0, -1);
                ImGui.Text($"counter = {_counter}");

                ImGui.DragInt("Draggable Int", ref _dragInt);

                float framerate = ImGui.GetIO().Framerate;
                ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
            }

            // 2. Show another simple window. In most cases you will use an explicit Begin/End pair to name your windows.
            if (_showAnotherWindow)
            {
                ImGui.Begin("Another Window", ref _showAnotherWindow);
                ImGui.Text("Hello from another window!");
                if (ImGui.Button("Close Me"))
                    _showAnotherWindow = false;
                ImGui.End();
            }

            // 3. Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
            if (_showDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowDemoWindow(ref _showDemoWindow);
            }

            if (ImGui.TreeNode("Tabs"))
            {
                if (ImGui.TreeNode("Basic"))
                {
                    ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                    if (ImGui.BeginTabBar("MyTabBar", tab_bar_flags))
                    {
                        if (ImGui.BeginTabItem("Avocado"))
                        {
                            ImGui.Text("This is the Avocado tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Broccoli"))
                        {
                            ImGui.Text("This is the Broccoli tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Cucumber"))
                        {
                            ImGui.Text("This is the Cucumber tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }

                        ImGui.EndTabBar();
                    }

                    ImGui.Separator();
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Advanced & Close Button"))
                {
                    // Expose a couple of the available flags. In most cases you may just call BeginTabBar() with no flags (0).
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_Reorderable", ref s_tab_bar_flags,
                        (uint) ImGuiTabBarFlags.Reorderable);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_AutoSelectNewTabs", ref s_tab_bar_flags,
                        (uint) ImGuiTabBarFlags.AutoSelectNewTabs);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_NoCloseWithMiddleMouseButton", ref s_tab_bar_flags,
                        (uint) ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
                    if ((s_tab_bar_flags & (uint) ImGuiTabBarFlags.FittingPolicyMask) == 0)
                        s_tab_bar_flags |= (uint) ImGuiTabBarFlags.FittingPolicyDefault;
                    if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyResizeDown", ref s_tab_bar_flags,
                        (uint) ImGuiTabBarFlags.FittingPolicyResizeDown))
                        s_tab_bar_flags &= ~((uint) ImGuiTabBarFlags.FittingPolicyMask ^
                                             (uint) ImGuiTabBarFlags.FittingPolicyResizeDown);
                    if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyScroll", ref s_tab_bar_flags,
                        (uint) ImGuiTabBarFlags.FittingPolicyScroll))
                        s_tab_bar_flags &= ~((uint) ImGuiTabBarFlags.FittingPolicyMask ^
                                             (uint) ImGuiTabBarFlags.FittingPolicyScroll);

                    // Tab Bar
                    string[] names = {"Artichoke", "Beetroot", "Celery", "Daikon"};

                    for (int n = 0; n < s_opened.Length; n++)
                    {
                        if (n > 0)
                        {
                            ImGui.SameLine();
                        }

                        ImGui.Checkbox(names[n], ref s_opened[n]);
                    }

                    // Passing a bool* to BeginTabItem() is similar to passing one to Begin(): the underlying bool will be set to false when the tab is closed.
                    if (ImGui.BeginTabBar("MyTabBar", (ImGuiTabBarFlags) s_tab_bar_flags))
                    {
                        for (int n = 0; n < s_opened.Length; n++)
                            if (s_opened[n] && ImGui.BeginTabItem(names[n], ref s_opened[n]))
                            {
                                ImGui.Text($"This is the {names[n]} tab!");
                                if ((n & 1) != 0)
                                    ImGui.Text("I am an odd tab.");
                                ImGui.EndTabItem();
                            }

                        ImGui.EndTabBar();
                    }

                    ImGui.Separator();
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }

            ImGuiIOPtr io = ImGui.GetIO();
            SetThing(out io.DeltaTime, 2f);

            if (_showMemoryEditor)
            {
                _memoryEditor.Draw("Memory Editor", _memoryEditorData, _memoryEditorData.Length);
            }
        }
    }
}