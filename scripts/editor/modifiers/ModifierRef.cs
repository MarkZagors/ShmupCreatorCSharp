using System.Collections.Generic;

namespace Editor
{
    public class ModifierRef : IModifier
    {
        required public ModifierID ID { get; set; }
        public ModifierType Type { get; set; } = ModifierType.REFERENCE;
        required public List<ComponentType> AllowedComponentTypes { get; set; }
        public bool Active { get; set; } = false;
        public IComponent Ref { get; set; } = null;
        public string LoadedRefName { get; set; } = null;
    }
}
