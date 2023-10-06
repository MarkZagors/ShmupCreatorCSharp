using Godot;
using System;

namespace Editor
{
    public partial class ComponentsController : Node
    {
        [Export] public Control ComponentContainer { get; private set; }
        [Export] public Label ComponentNameLabel { get; private set; }
        [Export] public VBoxContainer ComponentsVBox { get; private set; }
        private IComponent _openedComponent;

        public void OpenComponent(IComponent component)
        {
            _openedComponent = component;
            ComponentNameLabel.Text = component.Name;
        }

        public void ClearComponent() {
            _openedComponent = null;
            ComponentNameLabel.Text = "[No Component Selected]";
        }
    }
}