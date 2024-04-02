using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentBundle : BaseComponent, IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.BUNDLE;
        public Texture2D Icon { get; set; }

        public ComponentBundle(string name, TreeItem treeItem, Texture2D icon)
        {
            Name = name;
            TreeItem = treeItem;
            Icon = icon;
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
                    MaxValue = 5000,
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
                new ModifierRange {
                    ID = ModifierID.BUNDLE_SIZE,
                    Range = Range.From(2, 0)
                },
                new ModifierOptions {
                    ID = ModifierID.BUNDLE_TARGET,
                    Options = new() {
                        Option.TARGET_NO_TARGET,
                        Option.TARGET_PLAYER
                    },
                    SelectedOption = Option.TARGET_NO_TARGET
                },
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }

        public ComponentBullet GetRefBullletComponent()
        {
            return _lookupHelper.GetRefComponent<ComponentBullet>(ModifierID.BUNDLE_REF_BULLET);
        }
    }
}
