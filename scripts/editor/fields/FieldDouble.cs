using Godot;
using System;
using System.Linq;

namespace Editor
{
    public partial class FieldDouble : Control, IField
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public LineEdit NumberFieldLineEdit { get; set; }
        public ModifierDouble ModifierDouble { get; private set; }
        private string _oldText = "";
        private readonly char[] _lineEditAllowedCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
        public void Init(ModifierDouble modifierDouble)
        {
            ModifierDouble = modifierDouble;

            NumberFieldLineEdit.Text = ModifierDouble.Value.ToString();

            GetNode<Label>("FieldName").Text = ModifierNamer.Get(ModifierDouble.ID);
        }

        public void OnLineTextChange(string newText)
        {
            string modified = new(newText.Where(c => _lineEditAllowedCharacters.Contains(c)).ToArray());

            if (modified.Count(c => c == '.') > 1)
            {
                //Disable multiple dots in input
                modified = _oldText;
            }

            NumberFieldLineEdit.Text = modified;

            bool validDouble = double.TryParse(modified, out double value);
            if (validDouble)
            {
                if (value > ModifierDouble.MaxValue)
                {
                    value = ModifierDouble.MaxValue;
                    NumberFieldLineEdit.Text = value.ToString();
                }
            }
            else
            {
                value = 0;
            }

            ModifierDouble.Value = value;

            _oldText = NumberFieldLineEdit.Text;

            NumberFieldLineEdit.CaretColumn = 999;
            EmitSignal(SignalName.Update);
        }
    }
}

