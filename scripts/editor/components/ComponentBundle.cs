using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class ComponentBundle : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }

        public ComponentBundle(string name, TreeItem treeItem)
        {
            Name = name;
            TreeItem = treeItem;
            Modifiers = new List<IModifier> {
                new ModifierRef {
                    ID = ModifierID.BUNDLE_REF_BULLET,
                },
                new ModifierRange {
                    ID = ModifierID.BUNDLE_ANGLE,
                    Range = Range.From(180,-180)
                },
                new ModifierRange {
                    ID = ModifierID.BUNDLE_SPEED,
                    Range = Range.From(1, -1)
                },
            };
        }
    }
}
