using System.Collections.Generic;
using Godot;

namespace Editor {
    public enum ComponentType
    {
        BULLET,
        BUNDLE,
        SPAWNER,
        TIMER
    }

    public class ComponentNamer {
        private static readonly Dictionary<ComponentType, string> _nameList = new()
        {
            {ComponentType.BULLET, "Bullet"},
            {ComponentType.BUNDLE, "Bundle"},
            {ComponentType.SPAWNER, "Spawner"},
            {ComponentType.TIMER, "Timer"},
        };

        public static string Get(ComponentType id)
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