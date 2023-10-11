using System.Collections.Generic;
using System.Linq;
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
        public bool Valid { get; set; } = false;
        public Sequence Sequence { get; set; } = null;
        private readonly Dictionary<ModifierID, IModifier> _modifiersLookup;

        public ComponentSpawner(string name, TreeItem treeItem, Sequence sequence)
        {
            Name = name;
            TreeItem = treeItem;
            Sequence = sequence;
            Modifiers = new List<IModifier>
            {
                new ModifierRef {
                    ID = ModifierID.SPAWNER_REF_BUNDLE,
                    Active = true,
                    AllowedComponentTypes = new() {
                        ComponentType.BUNDLE
                    }
                },
            };
            _modifiersLookup = Modifiers.ToDictionary(modifier => modifier.ID);
        }

        public IModifier GetModifier(ModifierID modifierID)
        {
            if (_modifiersLookup.ContainsKey(modifierID))
            {
                return _modifiersLookup[modifierID];
            }
            else
            {
                return null;
            }
        }

        public ComponentBundle GetBundleComponent()
        {
            return (ComponentBundle)((ModifierRef)GetModifier(ModifierID.SPAWNER_REF_BUNDLE)).Ref;
        }
    }
}