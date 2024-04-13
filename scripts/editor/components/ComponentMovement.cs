using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class ComponentMovement : BaseComponent, IComponent
    {
        public string Name { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.MOVEMENT;
        public bool Valid { get; set; } = false;
        public Sequence Sequence { get; set; } = null;
        public Texture2D Icon { get; set; }

        public ComponentMovement(string name, Sequence sequence)
        {
            Name = name;
            Sequence = sequence;
            Modifiers = new List<IModifier>
            {
                new ModifierPosition {
                    ID = ModifierID.MOVEMENT_POSITION,
                    Active = true,
                },
                new ModifierDouble {
                    ID = ModifierID.MOVEMENT_TIME,
                    Active = true,
                    IsMovementTimelineUpdating = true,
                    Value = 1,
                    MaxValue = 10000,
                },
                new ModifierRange {
                    ID = ModifierID.MOVEMENT_CURVE,
                    Range = Range.TiltedUp(1,0),
                    IsMovementTimelineUpdating = true
                },
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }
    }
}
