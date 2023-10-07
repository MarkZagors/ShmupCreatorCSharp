using Godot;
using System;
using System.Collections.Generic;

namespace Editor
{
    public partial class SequenceController : Node
    {
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public Tree SequenceTree { get; private set; }
        [Export] public Control TemplatesTab { get; private set; }
        [Export] public Control SequenceTab { get; private set; }
        [Export] public Control CreationContainer { get; private set; }
        [Export] public VBoxContainer CreationContainerVBox { get; private set; }
        [Export] public PackedScene CreationButtonObj { get; private set; }

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

        private void CreateBullet()
        {
            TreeItem root = SequenceTree.GetRoot();
            TreeItem bulletComponentTreeItem = root.CreateChild();
            var name = WrapComponentName("Bullet", _openedSequence.Components);
            IComponent bulletComponent = new ComponentBullet
            (
                name: name,
                treeItem: bulletComponentTreeItem
            );

            bulletComponentTreeItem.SetText(0, name);
            _openedSequence.Components.Add(bulletComponent);
            _sequenceTreeLookup.Add(bulletComponentTreeItem, bulletComponent);
            CloseNewComponentBox();
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
                _sequenceTreeLookup[selectedTreeItem]
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

        private void CloseNewComponentBox()
        {
            CreationContainer.Visible = false;
            foreach (Button childButton in CreationContainerVBox.GetChildren())
            {
                childButton.QueueFree();
            }
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

        public void OnClickNewComponent()
        {
            CreationContainer.Visible = true;

            Button bulletButton = CreationButtonObj.Instantiate<Button>();
            bulletButton.Text = "Bullet";
            bulletButton.Pressed += CreateBullet;
            CreationContainerVBox.AddChild(bulletButton);
        }

        public void OnClickCloseNewComponent()
        {
            CloseNewComponentBox();
        }
    }
}

