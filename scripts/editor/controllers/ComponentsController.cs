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
        [Export] public PackedScene FieldReferenceObj { get; private set; }
        private IComponent _openedComponent;
        private Button _newModifierPlusButton;

        public void OpenComponent(IComponent component)
        {
            ClearComponentsVBox();

            _openedComponent = component;
            ComponentNameLabel.Text = component.Name;

            foreach (IModifier modifier in component.Modifiers)
            {
                if (modifier.Active)
                {
                    AddModifierField(modifier);
                }
            }

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

            AddModifierField(modifier);

            ComponentsVBox.MoveChild(_newModifierPlusButton, ComponentsVBox.GetChildCount() - 1);
            CreateBoxController.CloseCreationBox();
        }

        private void AddModifierField(IModifier modifier)
        {
            switch (modifier)
            {
                case ModifierRange modifierRange:
                    var fieldRange = FieldRangeObj.Instantiate<FieldRange>();
                    fieldRange.Init(modifierRange);
                    ComponentsVBox.AddChild(fieldRange);
                    break;
                case ModifierRef modifierRef:
                    var fieldRef = FieldReferenceObj.Instantiate<FieldReference>();
                    fieldRef.Init(modifierRef);
                    ComponentsVBox.AddChild(fieldRef);
                    break;
                default:
                    GD.PrintErr($"AddModifierField in Components Controller doesn't support modifier: {modifier}");
                    break;
            }
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