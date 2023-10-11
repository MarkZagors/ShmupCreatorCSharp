using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentTimer : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public Enums.ComponentType Type { get; set; } = Enums.ComponentType.TIMER;
        private readonly Dictionary<ModifierID, IModifier> _modifiersLookup;

        public ComponentTimer(string name, TreeItem treeItem)
        {
            Name = name;
            TreeItem = treeItem;
            Modifiers = new List<IModifier>
            {
                new ModifierInteger {
                    ID = ModifierID.TIMER_LOOP_COUNT,
                    Active = false,
                    Value = 1,
                    MaxValue = 5000,
                    IsStructureChanging = true
                },
                new ModifierDouble {
                    ID = ModifierID.TIMER_PROCESS_TIME,
                    Active = false,
                    Value = 0,
                    MaxValue = 5000,
                },
                new ModifierDouble {
                    ID = ModifierID.TIMER_WAIT_TIME,
                    Active = false,
                    Value = 1,
                    MaxValue = 5000,
                },
                new ModifierRange {
                    ID = ModifierID.TIMER_PROCESS_CURVE,
                    Range = Range.TiltedUp(1,0)
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
