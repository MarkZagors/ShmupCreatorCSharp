using ExtensionMethods;
using Godot;
using System;

namespace Editor
{
    public partial class FieldReference : Control
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public Label RefLabelNode { get; private set; }
        public ModifierRef ReferenceModifier { get; private set; }
        private SequenceController _sequenceController;
        private ComponentsController _componentsController;
        private bool _mouseHovering = false;

        public void Init(ModifierRef modifierRef)
        {
            ReferenceModifier = modifierRef;

            if (ReferenceModifier.Ref != null)
            {
                RefLabelNode.Text = ReferenceModifier.Ref.Name;
            }

            GetNode<Label>("FieldName").Text = ModifierNamer.Get(modifierRef.ID);
        }

        public override void _Ready()
        {
            //This could be better
            _sequenceController = GetNode<SequenceController>("/root/Editor/Controllers/SequenceController");
            _componentsController = GetNode<ComponentsController>("/root/Editor/Controllers/ComponentsController");
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustReleased("mouse_click"))
            {
                ReleaseMouse();
            }
        }

        private void ReleaseMouse()
        {
            if (_mouseHovering)
            {
                var draggingComponent = _sequenceController.GetSelectedComponent();
                var openedComponent = _componentsController.OpenedComponent;
                if (
                    draggingComponent != null
                    && openedComponent != draggingComponent
                    && ReferenceModifier.AllowedComponentTypes.Contains(draggingComponent.Type)
                )
                {
                    AddReference(draggingComponent);
                }
            }
        }

        private void AddReference(IComponent addingComponent)
        {
            RefLabelNode.Text = addingComponent.Name;
            ReferenceModifier.Ref = addingComponent;
            EmitSignal(SignalName.Update);
        }

        public void OnMouseEntered()
        {
            _mouseHovering = true;
        }

        public void OnMouseExited()
        {
            _mouseHovering = false;
        }
    }
}

