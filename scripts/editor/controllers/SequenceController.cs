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
        [Export] public LineEdit SequenceRenameLineEdit { get; private set; }

        [ExportGroup("Icons")]
        [Export] public Texture2D IconBullet { get; private set; }
        [Export] public Texture2D IconBundle { get; private set; }
        [Export] public Texture2D IconSpawner { get; private set; }
        [Export] public Texture2D IconTimer { get; private set; }
        [Export] public Texture2D IconWarning { get; private set; }

        private Sequence _openedSequence;
        private readonly Dictionary<TreeItem, IComponent> _sequenceTreeLookup = new();
        private TreeItem _selectedTreeItem;
        private Vector2 _treeItemSelectionMousePos;
        private IComponent _lastSelectedComponent = null;
        private float _doubleClickTimer = 0.0f;

        private const int TREE_ITEM_HEIGHT = 32;
        private const float DOUBLE_CLICK_THRESHOLD = 0.4f;

        public override void _Ready()
        {
            SequenceTab.GetNode<Label>("NoSelectionLabel").Visible = true;
            SequenceTab.GetNode<Control>("TopSequenceBar").Visible = false;
            SequenceTree.Visible = false;

            ComponentsController.Update += OnComponentUpdate;
            ComponentsController.OnValidRestructure += OnComponentUpdate;

            SequenceRenameLineEdit.TextSubmitted += OnRenameTextSubmit;
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustReleased("mouse_click"))
            {
                MouseRelease();
                CheckRenameDeselect();
            }
            _doubleClickTimer += (float)delta;
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
            IComponent component = ComponentFactory.CreateComponent(
                componentType: componentType,
                name: WrapComponentName(ComponentNamer.Get(componentType), _openedSequence.Components),
                sequence: _openedSequence
            );
            componentTreeItem.SetText(0, component.Name);
            componentTreeItem.SetCellMode(1, TreeItem.TreeCellMode.Icon);
            componentTreeItem.SetIcon(0, component.Icon);
            componentTreeItem.SetSelectable(1, false);
            componentTreeItem.SetIconMaxWidth(0, TREE_ITEM_HEIGHT);
            componentTreeItem.SetIconMaxWidth(1, TREE_ITEM_HEIGHT);
            componentTreeItem.CustomMinimumHeight = TREE_ITEM_HEIGHT;
            SequenceTree.SetColumnExpand(0, true);
            SequenceTree.SetColumnExpandRatio(0, 3);

            _openedSequence.Components.Add(component);
            _sequenceTreeLookup.Add(componentTreeItem, component);
            CreateBoxController.CloseCreationBox();
            OnComponentUpdate();
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

        private void CheckRenameDeselect()
        {
            if (!SequenceRenameLineEdit.GetGlobalRect().HasPoint(GetViewport().GetMousePosition()) && SequenceRenameLineEdit.Visible)
            {
                SequenceRenameLineEdit.Visible = false;
            }
        }

        private void ClickOnComponent(TreeItem selectedTreeItem)
        {
            IComponent component = _sequenceTreeLookup[selectedTreeItem];
            if (component == _lastSelectedComponent && _doubleClickTimer < DOUBLE_CLICK_THRESHOLD)
            {
                RenameStart(selectedTreeItem);
            }
            ComponentsController.OpenComponent(
                component,
                _openedSequence
            );
            _doubleClickTimer = 0.0f;
            _lastSelectedComponent = component;
        }

        private void RenameStart(TreeItem selectedTreeItem)
        {
            int treeItemindex = selectedTreeItem.GetIndex();
            int yPos = treeItemindex * 40 + 36 + 50;
            int xPos = 70;
            SequenceRenameLineEdit.Position = new Vector2(xPos, yPos);
            SequenceRenameLineEdit.Visible = true;
            SequenceRenameLineEdit.Text = selectedTreeItem.GetText(0);
            SequenceRenameLineEdit.SelectAll();
            SequenceRenameLineEdit.GrabFocus();
        }

        private void OnRenameTextSubmit(string newText)
        {
            IComponent component = _sequenceTreeLookup[_selectedTreeItem];
            component.Name = WrapComponentName(newText, _openedSequence.Components);
            _selectedTreeItem.SetText(0, component.Name);
            SequenceRenameLineEdit.Visible = false;
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

        private void OnComponentUpdate()
        {
            foreach (var component in _openedSequence.Components)
            {
                //TODO: Fix tree item deletion and reallocation - deleted tree item when chaning sequences
                // if (!IsInstanceValid(component.TreeItem)) continue;
                // if (component is ComponentBundle bundle)
                // {
                //     var bulletRef = bundle.GetRefBullletComponent();
                //     if (bulletRef == null)
                //     {
                //         component.TreeItem.SetIcon(1, IconWarning);
                //     }
                //     else
                //     {
                //         component.TreeItem.SetIcon(1, null);
                //     }
                // }

                // if (component is ComponentSpawner spawner)
                // {
                //     if (spawner.Valid == false)
                //     {
                //         component.TreeItem.SetIcon(1, IconWarning);
                //     }
                //     else
                //     {
                //         component.TreeItem.SetIcon(1, null);
                //     }
                // }
            }
        }
    }
}

