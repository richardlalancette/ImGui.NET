namespace ImGui.NET.SampleProgram
{
    public abstract class AbstractImController
    {
        protected bool Enabled = true;
        public StyleSheet Styles { get; private set; }
        public abstract void Draw();

        protected AbstractImController()
        {
        }

        protected AbstractImController(bool enabled)
        {
            Enabled = enabled;
        }

        protected AbstractImController(StyleSheet styleSheet)
        {
            Styles = styleSheet;
        }

        protected AbstractImController(bool enabled, StyleSheet styleSheet)
        {
            Enabled = enabled;
            Styles = styleSheet;
        }

        public void ApplyStyleSheet(StyleSheet styleSheet)
        {
            Styles = styleSheet;
        }
    }
}