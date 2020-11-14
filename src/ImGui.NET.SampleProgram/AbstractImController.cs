namespace ImGui.NET.SampleProgram
{
    internal abstract class AbstractImController
    {
        protected bool Enabled = true;
        public abstract void Draw();
    }
}