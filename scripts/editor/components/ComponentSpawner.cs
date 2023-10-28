using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentSpawner : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.SPAWNER;
        public bool Valid { get; set; } = false;
        public Sequence Sequence { get; set; } = null;
        private readonly LookupHelper _lookupHelper;

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
                new ModifierRef {
                    ID = ModifierID.SPAWNER_REF_SPAWN_TIMER,
                    AllowedComponentTypes = new() {
                        ComponentType.TIMER
                    }
                },
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }

        public IModifier GetModifier(ModifierID modifierID)
        {
            return _lookupHelper.GetModifier(modifierID);
        }

        public ComponentBundle GetBundleComponent()
        {
            return _lookupHelper.GetRefComponent<ComponentBundle>(ModifierID.SPAWNER_REF_BUNDLE);
        }

        public ComponentTimer GetSpawnTimerComponent()
        {
            return _lookupHelper.GetRefComponent<ComponentTimer>(ModifierID.SPAWNER_REF_SPAWN_TIMER);
        }
    }
}