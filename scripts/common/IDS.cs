using System;
using System.Collections.Generic;
using Godot;

namespace Editor
{
    public enum ModifierID
    {
        BUNDLE_REF_BULLET,
        BUNDLE_ANGLE,
        BUNDLE_SPEED,
    }

    public static class ModifierNamer
    {
        private static readonly Dictionary<ModifierID, string> _nameList = new()
        {
            {ModifierID.BUNDLE_REF_BULLET, "Bullet Data"},
            {ModifierID.BUNDLE_ANGLE, "Angle"},
            {ModifierID.BUNDLE_SPEED, "Speed"},
        };

        public static string Get(ModifierID id)
        {
            var valid = _nameList.TryGetValue(id, out string value);
            if (valid)
            {
                return value;
            }
            else
            {
                GD.Print($"ERROR: ModifierNamer has no name set for: {id}");
                GD.PushError($"ModifierNamer has no name set for: {id}");
                return null;
            }
        }
    }
}