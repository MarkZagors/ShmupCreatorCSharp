using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentBullet : BaseComponent, IComponent
    {
        public string Name { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.BULLET;
        public Texture2D Icon { get; set; }

        public ComponentBullet(string name, Texture2D icon)
        {
            Name = name;
            Icon = icon;
            Modifiers = new List<IModifier>
            {
                //Modifiers here
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }
    }
}
