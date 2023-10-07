using ExtensionMethods;
using Godot;
using System;
using System.Linq;

namespace Editor
{
    public partial class ComponentsController : Node
    {
        [Export] public CreateBoxController CreateBoxController { get; private set; }
        [Export] public Label ComponentNameLabel { get; private set; }
        [Export] public VBoxContainer ComponentsVBox { get; private set; }
        [Export] public PackedScene NewModifierButtonObj { get; private set; }
        [Export] public PackedScene FieldRangeObj { get; private set; }
        private IComponent _openedComponent;
        private Button _newModifierPlusButton;

        public void OpenComponent(IComponent component)
        {
            ClearComponentsVBox();

            _openedComponent = component;
            ComponentNameLabel.Text = component.Name;

            _newModifierPlusButton = NewModifierButtonObj.Instantiate<Button>();
            _newModifierPlusButton.Pressed += () => CreateBoxController.OnClickNewModifier(_openedComponent, this);
            ComponentsVBox.AddChild(_newModifierPlusButton);
        }

        public void ClearComponent()
        {
            _openedComponent = null;
            ComponentNameLabel.Text = "[No Component Selected]";
            ClearComponentsVBox();
        }

        public void CreateModifierField(IModifier modifier)
        {
            modifier.Active = true;

            FieldRange fieldRange = FieldRangeObj.Instantiate<FieldRange>();
            fieldRange.Init((ModifierRange)modifier);
            ComponentsVBox.AddChild(fieldRange);

            ComponentsVBox.MoveChild(_newModifierPlusButton, ComponentsVBox.GetChildCount() - 1);
            CreateBoxController.CloseCreationBox();
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