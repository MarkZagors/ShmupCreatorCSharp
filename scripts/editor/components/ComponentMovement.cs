using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class ComponentMovement : BaseComponent, IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.SPAWNER;
        public bool Valid { get; set; } = false;
        public Sequence Sequence { get; set; } = null;
        public Texture2D Icon { get; set; }

        public ComponentMovement(string name, TreeItem treeItem, Sequence sequence, Texture2D icon)
        {
            Name = name;
            TreeItem = treeItem;
            Sequence = sequence;
            Icon = icon;
            Modifiers = new List<IModifier>
            {
                new ModifierRange {
                    ID = ModifierID.MOVEMENT_CURVE,
                    Range = Range.TiltedUp(1,0)
                },
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }
    }
}
