using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public enum Option
    {
        NULL,
        TARGET_NO_TARGET,
        TARGET_PLAYER,
        SPRITE_RED,
        SPRITE_GREEN,
        SPRITE_BLUE,
    }

    public static class OptionsNamer
    {
        private static readonly Dictionary<Option, string> _nameList = new()
        {
            {Option.NULL, "NULL"},
            {Option.TARGET_NO_TARGET, "No Target"},
            {Option.TARGET_PLAYER, "Player"},

            {Option.SPRITE_RED, "Red"},
            {Option.SPRITE_GREEN, "Green"},
            {Option.SPRITE_BLUE, "Blue"},
        };

        public static string GetName(Option option)
        {
            var valid = _nameList.TryGetValue(option, out string value);
            if (valid)
            {
                return value;
            }
            else
            {
                GD.Print($"ERROR: OptionsNamer has no name set for: {option}");
                GD.PushError($"OptionsNamer has no name set for: {option}");
                return null;
            }
        }

        public static Option GetOption(string name)
        {
            var keys = _nameList.Where(x => x.Value == name).Select(x => x.Key).ToList();
            if (keys.Count == 1)
            {
                return keys[0];
            }
            else
            {
                GD.Print($"ERROR: OptionsNamer found not 1 number of keys: {keys.Count}");
                GD.PushError($"OptionsNamer found not 1 number of keys: {keys.Count}");
                return Option.NULL;
            }
        }
    }
}
