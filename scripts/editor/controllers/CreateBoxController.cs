using Editor;
using Godot;
using System;

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

            Button bulletButton = CreationButtonObj.Instantiate<Button>();
            bulletButton.Text = "Bullet";
            bulletButton.Pressed += () => SequenceController.CreateComponent(ComponentType.BULLET);
            CreationContainerVBox.AddChild(bulletButton);

            Button bundleButton = CreationButtonObj.Instantiate<Button>();
            bundleButton.Text = "Bundle";
            bundleButton.Pressed += () => SequenceController.CreateComponent(ComponentType.BUNDLE);
            CreationContainerVBox.AddChild(bundleButton);

            Button spawnerButton = CreationButtonObj.Instantiate<Button>();
            spawnerButton.Text = "Spawner";
            spawnerButton.Pressed += () => SequenceController.CreateComponent(ComponentType.SPAWNER);
            CreationContainerVBox.AddChild(spawnerButton);

            Button timerButton = CreationButtonObj.Instantiate<Button>();
            timerButton.Text = "Timer";
            timerButton.Pressed += () => SequenceController.CreateComponent(ComponentType.TIMER);
            CreationContainerVBox.AddChild(timerButton);
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
