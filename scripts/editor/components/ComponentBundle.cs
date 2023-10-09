using System.Collections.Generic;
using System.Linq;
using Godot;
using static Editor.Enums;

namespace Editor
{
    public class ComponentBundle : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public Enums.ComponentType Type { get; set; } = Enums.ComponentType.BUNDLE;
        private readonly Dictionary<ModifierID, IModifier> _modifiersLookup;

        public ComponentBundle(string name, TreeItem treeItem)
        {
            Name = name;
            TreeItem = treeItem;
            Modifiers = new List<IModifier> {
                new ModifierRef {
                    ID = ModifierID.BUNDLE_REF_BULLET,
                    Active = true,
                    AllowedComponentTypes = new() {
                        ComponentType.BULLET
                    }
                },
                new ModifierInteger {
                    ID = ModifierID.BUNDLE_COUNT,
                    Active = true,
                    Value = 1,
                    IsStructureChanging = true
                },
                new ModifierRange {
                    ID = ModifierID.BUNDLE_ANGLE,
                    Range = Range.From(180,-180)
                },
                new ModifierRange {
                    ID = ModifierID.BUNDLE_SPEED,
                    Range = Range.From(500, 0)
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
    }
}
