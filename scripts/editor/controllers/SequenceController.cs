using Godot;
using System;
using System.Collections.Generic;

namespace Editor
{
    public partial class SequenceController : Node
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public CreateBoxController CreateBoxController { get; private set; }
        [Export] public Tree SequenceTree { get; private set; }
        [Export] public Control TemplatesTab { get; private set; }
        [Export] public Control SequenceTab { get; private set; }

        private Sequence _openedSequence;
        private Dictionary<TreeItem, IComponent> _sequenceTreeLookup = new();
        private TreeItem _selectedTreeItem;
        private Vector2 _treeItemSelectionMousePos;

        public override void _Ready()
        {
            SequenceTab.GetNode<Label>("NoSelectionLabel").Visible = true;
            SequenceTab.GetNode<Control>("TopSequenceBar").Visible = false;
            SequenceTree.Visible = false;
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustReleased("mouse_click"))
            {
                MouseRelease();
            }
        }

        public void OpenSequence(Sequence sequence)
        {
            bool newOpening = false;
            if (_openedSequence != sequence) newOpening = true;

            _openedSequence = sequence;

            if (newOpening)
            {
                PopulateSequenceTree();
                ComponentsController.ClearComponent();
            }

            OpenSequenceTab();
        }

        private void PopulateSequenceTree()
        {
            SequenceTab.GetNode<Label>("NoSelectionLabel").Visible = false;
            SequenceTab.GetNode<Control>("TopSequenceBar").Visible = true;
            SequenceTree.Visible = true;

            SequenceTree.Clear();
            _sequenceTreeLookup.Clear();

            TreeItem root = SequenceTree.CreateItem();
            root.SetText(0, "Sequence");

            foreach (IComponent component in _openedSequence.Components)
            {
                TreeItem treeItem = root.CreateChild();
                treeItem.SetText(0, component.Name);
                _sequenceTreeLookup.Add(treeItem, component);
            }
        }

        public void CreateBullet()
        {
            CreateComponent(Enums.ComponentType.BULLET);
        }

        public void CreateBundle()
        {
            CreateComponent(Enums.ComponentType.BUNDLE);
        }

        public void CreateSpawner()
        {
            CreateComponent(Enums.ComponentType.SPAWNER);
        }

        private void CreateComponent(Enums.ComponentType componentType)
        {
            TreeItem root = SequenceTree.GetRoot();
            TreeItem componentTreeItem = root.CreateChild();
            IComponent component = null;
            string name = "NOT ADDED";

            switch (componentType)
            {
                case Enums.ComponentType.BULLET:
                    name = WrapComponentName("Bullet", _openedSequence.Components);
                    component = new ComponentBullet
                    (
                        name: name,
                        treeItem: componentTreeItem
                    );
                    break;
                case Enums.ComponentType.BUNDLE:
                    name = WrapComponentName("Bundle", _openedSequence.Components);
                    component = new ComponentBundle
                    (
                        name: name,
                        treeItem: componentTreeItem
                    );
                    break;
                case Enums.ComponentType.SPAWNER:
                    name = WrapComponentName("Spawner", _openedSequence.Components);
                    component = new ComponentSpawner
                    (
                        name: name,
                        treeItem: componentTreeItem
                    );
                    break;
                default:
                    GD.PushError($"Component type not implemented in Create Component!: {componentType}");
                    break;
            }

            componentTreeItem.SetText(0, name);
            _openedSequence.Components.Add(component);
            _sequenceTreeLookup.Add(componentTreeItem, component);
            CreateBoxController.CloseCreationBox();
            EmitSignal(SignalName.Update);
        }

        public void OnItemMouseSelected(Vector2 mousePos, int mouseIndex)
        {
            if (mouseIndex == 1)
            {
                _selectedTreeItem = SequenceTree.GetSelected();
                _treeItemSelectionMousePos = mousePos;
            }
        }

        private void MouseRelease()
        {
            if (_selectedTreeItem == null) return;

            var mousePosDiff = _treeItemSelectionMousePos - GetViewport().GetMousePosition();
            if (Math.Abs(mousePosDiff.X) < 10
                && mousePosDiff.Y < -40
                && mousePosDiff.Y > -90)
            {
                if (_selectedTreeItem != SequenceTree.GetRoot())
                {
                    ClickOnComponent(_selectedTreeItem);
                }
            }
        }

        private void ClickOnComponent(TreeItem selectedTreeItem)
        {
            ComponentsController.OpenComponent(
                _sequenceTreeLookup[selectedTreeItem],
                _openedSequence
            );
        }

        private static string WrapComponentName(string originalName, List<IComponent> components)
        {
            bool isValidName = false;
            int iterationCount = 1; //First duplicate will be 2
            string modifiedName = (string)originalName.Clone();

            while (!isValidName)
            {
                bool foundDuplicate = false;
                foreach (IComponent component in components)
                {
                    if (component.Name == modifiedName)
                    {
                        iterationCount += 1;
                        modifiedName = $"{originalName} {iterationCount}";
                        foundDuplicate = true;
                        break;
                    }
                }

                if (!foundDuplicate) isValidName = true;
            }

            return modifiedName;
        }

        public void OpenSequenceTab()
        {
            TemplatesTab.Visible = false;
            SequenceTab.Visible = true;
        }

        public void OpenTemplatesTab()
        {
            TemplatesTab.Visible = true;
            SequenceTab.Visible = false;
        }

        public IComponent GetSelectedComponent()
        {
            var valid = _sequenceTreeLookup.TryGetValue(_selectedTreeItem, out IComponent component);
            if (valid)
                return _sequenceTreeLookup[_selectedTreeItem];
            else
            {
                return null;
            }
        }
    }
}

