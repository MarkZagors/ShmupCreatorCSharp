using ExtensionMethods;
using Godot;
using System;
using System.Linq;

namespace Editor
{
    public partial class ComponentsController : Node
    {
        [Export] public Control ComponentContainer { get; private set; }
        [Export] public Label ComponentNameLabel { get; private set; }
        [Export] public VBoxContainer ComponentsVBox { get; private set; }
        [Export] public PackedScene NewModifierButtonObj { get; private set; }
        [Export] public Control CreationContainer { get; private set; }
        [Export] public VBoxContainer CreationContainerVBox { get; private set; }
        [Export] public PackedScene CreationButtonObj { get; private set; }
        [Export] public PackedScene FieldRangeObj { get; private set; }
        private IComponent _openedComponent;
        private Button _newModifierPlusButton;

        public void OpenComponent(IComponent component)
        {
            ClearComponentsVBox();

            _openedComponent = component;
            ComponentNameLabel.Text = component.Name;

            _newModifierPlusButton = NewModifierButtonObj.Instantiate<Button>();
            _newModifierPlusButton.Pressed += OnClickNewModifierBox;
            ComponentsVBox.AddChild(_newModifierPlusButton);
        }

        public void ClearComponent()
        {
            _openedComponent = null;
            ComponentNameLabel.Text = "[No Component Selected]";
            ClearComponentsVBox();
        }

        private void OnClickNewModifierBox()
        {
            CreationContainer.Visible = true;

            foreach (IModifier modifier in _openedComponent.Modifiers)
            {
                if (!modifier.Active)
                {
                    Button modifierButton = CreationButtonObj.Instantiate<Button>();
                    modifierButton.Text = ModifierNamer.Get(modifier.ID);
                    modifierButton.Pressed += () => CreateModifierField(modifier);
                    CreationContainerVBox.AddChild(modifierButton);
                    GD.Print(_openedComponent.Modifiers.ToStringMembers<IModifier>());
                }
            }
        }

        private void CreateModifierField(IModifier modifier)
        {
            modifier.Active = true;
            Control fieldRange = FieldRangeObj.Instantiate<Control>();
            ComponentsVBox.AddChild(fieldRange);

            // ComponentsVBox.MoveChild(_newModifierPlusButton, ComponentsVBox.GetChildCount() - 1);

            CreationContainer.Visible = false;
        }

        private void ClearComponentsVBox()
        {
            foreach (Control vboxChild in ComponentsVBox.GetChildren().Cast<Control>())
            {
                vboxChild.QueueFree();
            }
        }
    }
}