using Godot;
using System;

namespace Editor
{
    public partial class FieldPosition : Control
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public Button PositionButton { get; private set; }
        public ModifierPosition ModifierPosition { get; private set; }

        public void Init(ModifierPosition modifierPosition)
        {
            ModifierPosition = modifierPosition;
        }

        public void OnButtonPressed()
        {
            GD.Print("Press button");
        }
    }
}
