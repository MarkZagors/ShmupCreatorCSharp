using System.Collections.Generic;
using Godot;
using static Editor.Enums;

namespace Editor
{
    public class ComponentSpawner : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public Enums.ComponentType Type { get; set; } = Enums.ComponentType.SPAWNER;

        public ComponentSpawner(string name, TreeItem treeItem)
        {
            Name = name;
            TreeItem = treeItem;
            Modifiers = new List<IModifier>
            {
                new ModifierRef {
                    ID = ModifierID.SPAWNER_REF,
                    Active = true,
                    AllowedComponentTypes = new() {
                        ComponentType.BUNDLE
                    }
                },
            };
        }
    }
}