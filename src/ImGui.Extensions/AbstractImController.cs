namespace ImGui.Extensions
{
    public abstract class AbstractImController
    {
        public bool Enabled { get; set; }
        public StyleSheet Styles { get; private set; }
        public abstract void Draw();

        protected AbstractImController()
        {
            Enabled = true;
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