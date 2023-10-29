using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentBullet : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.BULLET;
        public Texture2D Icon { get; set; }
        private readonly LookupHelper _lookupHelper;

        public ComponentBullet(string name, TreeItem treeItem, Texture2D icon)
        {
            Name = name;
            TreeItem = treeItem;
            Icon = icon;
            Modifiers = new List<IModifier>
            {
                //Modifiers here
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }

        public IModifier GetModifier(ModifierID modifierID)
        {
            return _lookupHelper.GetModifier(modifierID);
        }
    }
}
