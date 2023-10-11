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

        SPAWNER_REF_BUNDLE,
        SPAWNER_REF_SPAWN_TIMER,

        TIMER_LOOP_COUNT,
        TIMER_PROCESS_TIME,
        TIMER_WAIT_TIME,
        TIMER_PROCESS_CURVE,
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

            {ModifierID.SPAWNER_REF_BUNDLE, "Bundle Data"},
            {ModifierID.SPAWNER_REF_SPAWN_TIMER, "Spawn Timer"},

            {ModifierID.TIMER_LOOP_COUNT, "Loop Count"},
            {ModifierID.TIMER_PROCESS_TIME, "Process Time"},
            {ModifierID.TIMER_WAIT_TIME, "Wait Time"},
            {ModifierID.TIMER_PROCESS_CURVE, "Process Curve"},
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