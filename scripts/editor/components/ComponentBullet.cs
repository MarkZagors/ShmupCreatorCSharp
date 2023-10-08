using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class ComponentBullet : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public Enums.ComponentType Type { get; set; } = Enums.ComponentType.BULLET;

        public ComponentBullet(string name, TreeItem treeItem)
        {
            Name = name;
            TreeItem = treeItem;
            Modifiers = new List<IModifier>
            {
                //Modifiers here
            };
        }
    }
}
