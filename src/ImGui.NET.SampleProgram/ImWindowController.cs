using System.Numerics;
using ImGuiNET;

namespace ImGui.NET.SampleProgram
{
  public class ImWindowController : AbstractImController
  {
    public readonly string Title;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public ImGuiCond SizeCondition { get; set; } = ImGuiCond.FirstUseEver;
    private ImGuiWindowFlags WindowStyle { get; set; }

    public ImWindowController(string title)
    {
      Title = title;
    }

    public ImWindowController(string title, int width, int height)
    {
      Title = title;
      Width = width;
      Height = height;
    }

    public ImWindowController(string title, int width, int height, ImGuiCond sizeCondition)
    {
      Title = title;
      Width = width;
      Height = height;
      SizeCondition = sizeCondition;
    }

    public ImWindowController(string title, int width, int height, ImGuiCond sizeCondition, ImGuiWindowFlags windowStyle)
    {
      Title = title;
      Width = width;
      Height = height;
      SizeCondition = sizeCondition;
      WindowStyle = windowStyle;
    }

    public override void Draw()
    {
      if (Enabled)
      {
        ImGuiNET.ImGui.SetNextWindowSize(new Vector2(Width, Height), SizeCondition);
        Styles?.Apply();
        bool enabled = Enabled;
        ImGuiNET.ImGui.Begin(Title, ref enabled, WindowStyle);
        Enabled = enabled;
        DrawWindowElements();
        ImGuiNET.ImGui.End();
        Styles?.Restore();
      }
    }

    protected virtual void DrawWindowElements()
    {
    }
  }
}
