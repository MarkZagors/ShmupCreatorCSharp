using Godot;
using System;

namespace Editor
{
    public partial class FieldOptions : Control
    {
        [Signal] public delegate void UpdateEventHandler();
        public ModifierOptions ModifierOptions { get; private set; }
        public void Init(ModifierOptions modifierOptions)
        {
            ModifierOptions = modifierOptions;

            GetNode<Label>("FieldName").Text = ModifierNamer.Get(ModifierOptions.ID);
        }
    }
}
