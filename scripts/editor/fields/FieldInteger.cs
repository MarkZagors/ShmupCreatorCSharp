using Godot;
using System;
using System.Linq;

namespace Editor
{
    public partial class FieldInteger : Control, IField
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public LineEdit NumberFieldLineEdit { get; set; }
        public ModifierInteger ModifierInteger { get; private set; }
        private readonly char[] _lineEditAllowedCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        public void Init(ModifierInteger modifierInteger)
        {
            ModifierInteger = modifierInteger;

            NumberFieldLineEdit.Text = ModifierInteger.Value.ToString();

            GetNode<Label>("FieldName").Text = ModifierNamer.Get(ModifierInteger.ID);
        }

        public void OnLineTextChange(string newText)
        {
            string modified = new(newText.Where(c => _lineEditAllowedCharacters.Contains(c)).ToArray());
            NumberFieldLineEdit.Text = modified;

            bool validInteger = int.TryParse(modified, out int value);
            if (validInteger)
            {
                if (value > ModifierInteger.MaxValue)
                {
                    value = ModifierInteger.MaxValue;
                    NumberFieldLineEdit.Text = value.ToString();
                }
            }
            else
            {
                value = 0;
            }

            ModifierInteger.Value = value;

            NumberFieldLineEdit.CaretColumn = 999;
            EmitSignal(SignalName.Update);
        }
    }
}

