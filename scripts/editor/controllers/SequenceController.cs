using Godot;
using System;
using System.Collections.Generic;

namespace Editor
{
    public partial class SequenceController : Node
    {
        [Signal] public delegate void UpdateEventHandler();

        [ExportGroup("Controllers")]
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public CreateBoxController CreateBoxController { get; private set; }

        [ExportGroup("Nodes")]
        [Export] public Tree SequenceTree { get; private set; }
        [Export] public Control TemplatesTab { get; private set; }
        [Export] public Control SequenceTab { get; private set; }

        [ExportGroup("Icons")]
        [Export] public Texture2D IconBullet { get; private set; }
        [Export] public Texture2D IconBundle { get; private set; }
        [Export] public Texture2D IconSpawner { get; private set; }
        [Export] public Texture2D IconTimer { get; private set; }

        private Sequence _openedSequence;
        private readonly Dictionary<TreeItem, IComponent> _sequenceTreeLookup = new();
        private TreeItem _selectedTreeItem;
        private Vector2 _treeItemSelectionMousePos;

        private const int TREE_ITEM_HEIGHT = 32;

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
            root.CustomMinimumHeight = TREE_ITEM_HEIGHT;

            foreach (IComponent component in _openedSequence.Components)
            {
                TreeItem treeItem = root.CreateChild();
                treeItem.CustomMinimumHeight = TREE_ITEM_HEIGHT;
                treeItem.SetText(0, component.Name);
                _sequenceTreeLookup.Add(treeItem, component);
            }
        }

        public void CreateComponent(ComponentType componentType)
        {
            TreeItem root = SequenceTree.GetRoot();
            TreeItem componentTreeItem = root.CreateChild();
            IComponent component = null;
            string name = "NOT ADDED";

            switch (componentType)
            {
                case ComponentType.BULLET:
                    name = WrapComponentName("Bullet", _openedSequence.Components);
                    component = new ComponentBullet
                    (
                        name: name,
                        treeItem: componentTreeItem,
                        icon: IconBullet
                    );
                    break;
                case ComponentType.BUNDLE:
                    name = WrapComponentName("Bundle", _openedSequence.Components);
                    component = new ComponentBundle
                    (
                        name: name,
                        treeItem: componentTreeItem,
                        icon: IconBundle
                    );
                    break;
                case ComponentType.SPAWNER:
                    name = WrapComponentName("Spawner", _openedSequence.Components);
                    component = new ComponentSpawner
                    (
                        name: name,
                        treeItem: componentTreeItem,
                        sequence: _openedSequence,
                        icon: IconSpawner
                    );
                    break;
                case ComponentType.TIMER:
                    name = WrapComponentName("Timer", _openedSequence.Components);
                    component = new ComponentTimer
                    (
                        name: name,
                        treeItem: componentTreeItem,
                        icon: IconTimer
                    );
                    break;
                default:
                    GD.PushError($"Component type not implemented in Create Component!: {componentType}");
                    break;
            }

            componentTreeItem.SetText(0, name);
            componentTreeItem.SetCellMode(1, TreeItem.TreeCellMode.Icon);
            componentTreeItem.SetIcon(0, component.Icon);
            componentTreeItem.SetIcon(1, null);
            componentTreeItem.SetSelectable(1, false);
            componentTreeItem.SetIconMaxWidth(0, TREE_ITEM_HEIGHT);
            componentTreeItem.SetIconMaxWidth(1, TREE_ITEM_HEIGHT);
            componentTreeItem.CustomMinimumHeight = TREE_ITEM_HEIGHT;
            SequenceTree.SetColumnExpand(0, true);
            SequenceTree.SetColumnExpandRatio(0, 3);

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
                return component;
            else
            {
                return null;
            }
        }
    }
}

