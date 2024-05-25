using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentSpawner : BaseComponent, IComponent
    {
        public string Name { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.SPAWNER;
        public bool Valid { get; set; } = false;
        public Sequence Sequence { get; set; } = null;
        public Texture2D Icon { get; set; }

        public ComponentSpawner(string name, Sequence sequence)
        {
            Name = name;
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

        public ComponentBundle GetBundleComponent()
        {
            return _lookupHelper.GetRefComponent<ComponentBundle>(ModifierID.SPAWNER_REF_BUNDLE);
        }

        public ComponentTimer GetSpawnTimerComponent()
        {
            return _lookupHelper.GetRefComponent<ComponentTimer>(ModifierID.SPAWNER_REF_SPAWN_TIMER);
        }

        public ComponentBullet GetBulletComponent()
        {
            ComponentBundle componentBundle = GetBundleComponent();
            return componentBundle.GetRefBullletComponent();
        }
    }
}
