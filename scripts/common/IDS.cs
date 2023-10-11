using System;
using System.Collections.Generic;
using Godot;

namespace Editor
{
    public enum ModifierID
    {
        BUNDLE_REF_BULLET,
        BUNDLE_COUNT,
        BUNDLE_ANGLE,
        BUNDLE_SPEED,
        BUNDLE_SIZE,
        SPAWNER_REF,
        TIMER_LOOP_COUNT,
        TIMER_PROCESS_TIME,
        TIMER_LOOP_TIME
    }

    public static class ModifierNamer
    {
        private static readonly Dictionary<ModifierID, string> _nameList = new()
        {
            {ModifierID.BUNDLE_REF_BULLET, "Bullet Data"},
            {ModifierID.BUNDLE_COUNT, "Count"},
            {ModifierID.BUNDLE_ANGLE, "Angle"},
            {ModifierID.BUNDLE_SPEED, "Speed"},
            {ModifierID.BUNDLE_SIZE, "Size"},
            {ModifierID.SPAWNER_REF, "Bundle Data"},
            {ModifierID.TIMER_LOOP_COUNT, "Loop Count"},
            {ModifierID.TIMER_PROCESS_TIME, "Process Time"},
            {ModifierID.TIMER_LOOP_TIME, "Loop Time"},
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