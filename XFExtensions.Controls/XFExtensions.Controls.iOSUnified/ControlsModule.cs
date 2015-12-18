namespace XFExtensions.Controls.iOSUnified
{
    [Preserve(AllMembers = true)]
    public static class ControlsModule
    {
        public static void Init() { }
    }

    public sealed class PreserveAttribute : System.Attribute 
    {
        public bool AllMembers;
        public bool Conditional;
    }
}