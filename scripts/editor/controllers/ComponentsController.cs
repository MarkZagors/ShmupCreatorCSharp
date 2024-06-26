using ExtensionMethods;
using Godot;
using System;
using System.Linq;

namespace Editor
{
    public partial class ComponentsController : Node
    {
        [Signal] public delegate void UpdateEventHandler();
        [Signal] public delegate void OnValidRestructureEventHandler();
        [Signal] public delegate void MoveTimelineUpdateEventHandler();
        [Signal] public delegate void ComponentSelectEventHandler();
        [Export] public CreateBoxController CreateBoxController { get; private set; }
        [Export] public Label ComponentNameLabel { get; private set; }
        [Export] public VBoxContainer ComponentsVBox { get; private set; }
        [Export] public PackedScene NewModifierButtonObj { get; private set; }
        [Export] public PackedScene FieldRangeObj { get; private set; }
        [Export] public PackedScene FieldReferenceObj { get; private set; }
        [Export] public PackedScene FieldIntegerObj { get; private set; }
        [Export] public PackedScene FieldDoubleObj { get; private set; }
        [Export] public PackedScene FieldOptionsObj { get; private set; }
        [Export] public PackedScene FieldPositionObj { get; private set; }
        public IComponent OpenedComponent { get; private set; }
        private Sequence _openedSequence;
        private Button _newModifierPlusButton;

        public void OpenComponent(IComponent component, Sequence sequence)
        {
            ClearComponentsVBox();

            OpenedComponent = component;
            _openedSequence = sequence;

            ComponentNameLabel.Text = component.Name;
            ComponentNameLabel.GetNode<TextureRect>("ComponentIcon").Texture = component.Icon;

            foreach (IModifier modifier in component.Modifiers)
            {
                if (modifier.Active)
                {
                    AddModifierField(modifier);
                }
            }

            _newModifierPlusButton = NewModifierButtonObj.Instantiate<Button>();
            _newModifierPlusButton.Pressed += () => CreateBoxController.OnClickNewModifier(OpenedComponent, this);
            ComponentsVBox.AddChild(_newModifierPlusButton);

            EmitSignal(SignalName.ComponentSelect);
        }

        public void ClearComponent()
        {
            OpenedComponent = null;
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
                    fieldRange.Update += () =>
                    {
                        if (modifierRange.IsMovementTimelineUpdating)
                            EmitSignal(SignalName.MoveTimelineUpdate);
                        EmitSignal(SignalName.Update);
                    };
                    ComponentsVBox.AddChild(fieldRange);
                    break;
                case ModifierRef modifierRef:
                    var fieldRef = FieldReferenceObj.Instantiate<FieldReference>();
                    fieldRef.Init(modifierRef);
                    fieldRef.Update += () =>
                    {
                        EmitSignal(SignalName.Update);
                        CheckValidSpawnerConnection();
                    };
                    ComponentsVBox.AddChild(fieldRef);
                    break;
                case ModifierInteger modifierInteger:
                    var fieldInt = FieldIntegerObj.Instantiate<FieldInteger>();
                    fieldInt.Init(modifierInteger);
                    fieldInt.Update += () =>
                    {
                        EmitSignal(SignalName.Update);
                        if (modifierInteger.IsStructureChanging)
                            CheckValidSpawnerConnection();
                    };
                    ComponentsVBox.AddChild(fieldInt);
                    break;
                case ModifierDouble modifierDouble:
                    var fieldDouble = FieldDoubleObj.Instantiate<FieldDouble>();
                    fieldDouble.Init(modifierDouble);
                    fieldDouble.Update += () =>
                    {
                        if (modifierDouble.IsMovementTimelineUpdating)
                            EmitSignal(SignalName.MoveTimelineUpdate);
                        EmitSignal(SignalName.Update);
                        if (modifierDouble.IsStructureChanging)
                            CheckValidSpawnerConnection();
                    };
                    ComponentsVBox.AddChild(fieldDouble);
                    break;
                case ModifierOptions modifierOptions:
                    var fieldOptions = FieldOptionsObj.Instantiate<FieldOptions>();
                    fieldOptions.Init(modifierOptions);
                    fieldOptions.Update += () =>
                    {
                        EmitSignal(SignalName.Update);
                        if (modifierOptions.IsStructureChanging)
                            CheckValidSpawnerConnection();
                    };
                    ComponentsVBox.AddChild(fieldOptions);
                    break;
                case ModifierPosition modifierPosition:
                    var fieldPosition = FieldPositionObj.Instantiate<FieldPosition>();
                    fieldPosition.Init(modifierPosition);
                    fieldPosition.Update += () =>
                    {
                        EmitSignal(SignalName.MoveTimelineUpdate);
                        EmitSignal(SignalName.Update);
                    };
                    ComponentsVBox.AddChild(fieldPosition);
                    break;
                default:
                    GD.PrintErr($"AddModifierField in Components Controller doesn't support modifier: {modifier}");
                    break;
            }
            EmitSignal(SignalName.Update);
        }

        private void CheckValidSpawnerConnection()
        {
            SpawnerVerifier.Verify(_openedSequence.Components);
            EmitSignal(SignalName.OnValidRestructure);
        }

        // private void CheckValidSpawnerConnection()
        // {
        //     var spawnRef = (ModifierRef)OpenedComponent.GetModifier(ModifierID.SPAWNER_REF_BUNDLE);
        //     var bundleRef = (ModifierRef)OpenedComponent.GetModifier(ModifierID.BUNDLE_REF_BULLET);
        //     if (spawnRef != null && spawnRef.Ref != null)
        //     {
        //         var bundleRefNested = (ModifierRef)spawnRef.Ref.GetModifier(ModifierID.BUNDLE_REF_BULLET);
        //         if (bundleRefNested != null && bundleRefNested.Ref != null)
        //         {
        //             ((ComponentSpawner)OpenedComponent).Valid = true;
        //             EmitSignal(SignalName.OnValidRestructure);
        //         }
        //         return;
        //     }

        //     if (bundleRef != null)
        //     {
        //         CheckValidSpawnerConnectionFromBundleRef(bundleRef);
        //         return;
        //     }

        //     if (
        //         OpenedComponent.Type == ComponentType.TIMER
        //     )
        //     {
        //         //Catch all for different types of valid restructure, add more in the future, if restructure not triggering
        //         EmitSignal(SignalName.OnValidRestructure);
        //         return;
        //     }
        // }

        // private void CheckValidSpawnerConnectionFromBundleRef(ModifierRef bundleRef)
        // {
        //     foreach (IComponent component in _openedSequence.Components)
        //     {
        //         var spawnRefInSequence = (ModifierRef)component.GetModifier(ModifierID.SPAWNER_REF_BUNDLE);
        //         if (spawnRefInSequence != null && spawnRefInSequence.Ref != null)
        //         {
        //             //If there is a spawner in the opened sequence, check if its ref is the same as the modified components
        //             var bundleRefInSpawner = (ModifierRef)spawnRefInSequence.Ref.GetModifier(ModifierID.BUNDLE_REF_BULLET);
        //             if (bundleRefInSpawner.Ref == bundleRef.Ref)
        //             {
        //                 ((ComponentSpawner)component).Valid = true;
        //                 EmitSignal(SignalName.OnValidRestructure);
        //             }
        //         }
        //     }
        // }

        private void ClearComponentsVBox()
        {
            foreach (Control vboxChild in ComponentsVBox.GetChildren().Cast<Control>())
            {
                vboxChild.QueueFree();
            }
        }
    }
}
