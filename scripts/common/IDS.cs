using System.Collections.Generic;

namespace Editor
{
    public enum ModifierID
    {
        BUNDLE_ANGLE
    }

    public static class ModifierNamer
    {
        private static Dictionary<ModifierID, string> _nameList = new Dictionary<ModifierID, string>
        {
            {ModifierID.BUNDLE_ANGLE, "Angle"},
        };

        public static string Get(ModifierID id)
        {
            return _nameList[id];
        }
    }
}