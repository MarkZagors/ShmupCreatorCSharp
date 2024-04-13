using System.Collections.Generic;
using Godot;

namespace Editor
{
    public enum ModifierType
    {
        DOUBLE,
        INTEGER,
        OPTIONS,
        POSITION,
        RANGE,
        REFERENCE,
    }

    public class ModifierTypeNamer
    {
        private static readonly Dictionary<ModifierType, string> _nameList = new()
        {
            {ModifierType.DOUBLE, "Double"},
            {ModifierType.INTEGER, "Integer"},
            {ModifierType.OPTIONS, "Options"},
            {ModifierType.RANGE, "Range"},
            {ModifierType.REFERENCE, "Reference"},
        };

        public static string Get(ModifierType id)
        {
            var valid = _nameList.TryGetValue(id, out string value);
            if (valid)
            {
                return value;
            }
            else
            {
                GD.Print($"ERROR: ComponentNamer has no name set for: {id}");
                GD.PushError($"ComponentNamer has no name set for: {id}");
                return null;
            }
        }
    }
}
