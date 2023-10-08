using System.Collections.Generic;
using Godot;

namespace Editor
{
    public interface IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public Enums.ComponentType Type { get; set; }
        public IModifier GetModifier(ModifierID modifierID);
    }
}
