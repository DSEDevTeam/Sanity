using UnityEditor;

namespace DraconisNexus
{
    public static class DraconisNexusMenu
    {
        [MenuItem("Draconis Nexus/Open")]
        private static void OpenDraconisNexus()
        {
            DraconisNexusWindow.ShowWindow();
        }
    }
}
