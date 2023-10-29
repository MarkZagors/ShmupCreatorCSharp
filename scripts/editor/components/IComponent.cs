using System.Collections.Generic;
using Godot;

namespace Editor
{
    public interface IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; }
        public Texture2D Icon { get; set; }
        public IModifier GetModifier(ModifierID modifierID);
    }
}
