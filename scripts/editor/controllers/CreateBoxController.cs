using Godot;

namespace Editor
{
    public partial class CreateBoxController : Node
    {
        [Export] public SequenceController SequenceController { get; private set; } //Remove when adding component button programatically
        [Export] public Control CreationContainer { get; private set; }
        [Export] public VBoxContainer CreationContainerVBox { get; private set; }
        [Export] public PackedScene CreationButtonObj { get; private set; }

        public void CloseCreationBox()
        {
            CreationContainer.Visible = false;
            foreach (Button childButton in CreationContainerVBox.GetChildren())
            {
                childButton.QueueFree();
            }
        }

        public void OnClickNewComponent()
        {
            CreationContainer.Visible = true;

            RenderNewComponent(ComponentType.BULLET);
            RenderNewComponent(ComponentType.BUNDLE);
            RenderNewComponent(ComponentType.SPAWNER);
            RenderNewComponent(ComponentType.TIMER);
            RenderNewComponent(ComponentType.MOVEMENT);
        }

        private void RenderNewComponent(ComponentType componentType)
        {
            Button bulletButton = CreationButtonObj.Instantiate<Button>();
            bulletButton.Text = ComponentNamer.Get(componentType);
            bulletButton.Pressed += () => SequenceController.CreateComponent(componentType);
            CreationContainerVBox.AddChild(bulletButton);
        }

        public void OnClickNewModifier(IComponent component, ComponentsController componentsController)
        {
            CreationContainer.Visible = true;

            foreach (IModifier modifier in component.Modifiers)
            {
                if (!modifier.Active)
                {
                    Button modifierButton = CreationButtonObj.Instantiate<Button>();
                    modifierButton.Text = ModifierNamer.Get(modifier.ID);
                    modifierButton.Pressed += () => componentsController.CreateModifierField(modifier);
                    CreationContainerVBox.AddChild(modifierButton);
                }
            }
        }

        public void OnClickCloseNewComponent()
        {
            CloseCreationBox();
        }
    }
}
