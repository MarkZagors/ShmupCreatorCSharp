using Godot;
using System;

namespace Editor
{
    public partial class SequenceController : Node
    {
        [Export] public Tree SequenceTree { get; private set; }
        [Export] public Control TemplatesTab { get; private set; }
        [Export] public Control SequenceTab { get; private set; }
        [Export] public Control CreationContainer { get; private set; }
        [Export] public VBoxContainer CreationContainerVBox { get; private set; }
        [Export] public PackedScene CreationButtonObj { get; private set; }

        private Sequence _openedSequence;
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

            if (newOpening) PopulateSequenceTree();

            OpenSequenceTab();
        }

        private void PopulateSequenceTree()
        {
            SequenceTab.GetNode<Label>("NoSelectionLabel").Visible = false;
            SequenceTab.GetNode<Control>("TopSequenceBar").Visible = true;
            SequenceTree.Visible = true;

            SequenceTree.Clear();

            TreeItem root = SequenceTree.CreateItem();
            root.SetText(0, "Sequence");
        }

        private void CreateBullet()
        {
            TreeItem root = SequenceTree.GetRoot();
            TreeItem bulletComponentTreeItem = root.CreateChild();
            bulletComponentTreeItem.SetText(0, "Bullet");

            CloseNewComponent();
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
                GD.Print(mousePosDiff);
                GD.Print("Click!");
            }
        }

        private void CloseNewComponent()
        {
            CreationContainer.Visible = false;
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
            bulletButton.Text = "Button";
            bulletButton.Pressed += CreateBullet;
            CreationContainerVBox.AddChild(bulletButton);
        }

        public void OnClickCloseNewComponent()
        {
            CloseNewComponent();
        }
    }
}

