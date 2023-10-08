using System.Collections.Generic;
using static Editor.Enums;

namespace Editor
{
    public class ModifierRef : IModifier
    {
        public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public List<ComponentType> AllowedComponentTypes { get; set; }
        public IComponent Ref { get; set; } = null;
    }
}