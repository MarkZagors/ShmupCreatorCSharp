using Godot;
using System;

namespace Editor
{
    public partial class FieldOptions : Control
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public OptionButton OptionButton { get; private set; }
        public ModifierOptions ModifierOptions { get; private set; }
        public void Init(ModifierOptions modifierOptions)
        {
            ModifierOptions = modifierOptions;

            GetNode<Label>("FieldName").Text = ModifierNamer.Get(ModifierOptions.ID);

            int index = 0;
            foreach (Option option in ModifierOptions.Options)
            {
                OptionButton.AddItem(
                    OptionsNamer.GetName(option)
                );

                if (ModifierOptions.SelectedOption == option)
                {
                    OptionButton.Select(index);
                }

                index += 1;
            }

            OptionButton.ItemSelected += (index) => OnUpdate(index);
        }

        public void OnUpdate(long index)
        {
            var name = OptionButton.GetItemText((int)index);
            ModifierOptions.SelectedOption = OptionsNamer.GetOption(name);
            EmitSignal(SignalName.Update);
        }
    }
}
