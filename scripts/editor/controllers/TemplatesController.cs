using Godot;
using System;

namespace Editor
{
    public partial class TemplatesController : Node
    {
        [Export] public Tree TemplatesTree;
        [Export] public PlayController PlayController;

        private TreeItem _draggingTreeItem = null;
        private bool _isMouseInMidContainer = false;

        private const int TREE_ITEM_HEIGHT = 32;

        public override void _Ready()
        {
            TreeItem root = TemplatesTree.CreateItem();
            root.SetText(0, "Templates");
            root.CustomMinimumHeight = TREE_ITEM_HEIGHT;

            TreeItem newSequenceItem = root.CreateChild();
            newSequenceItem.SetText(0, "New Sequence");
            newSequenceItem.CustomMinimumHeight = TREE_ITEM_HEIGHT;
            // TreeItem testGroup = root.CreateChild();
            // testGroup.SetText(0, "Test Group");
            // TreeItem testChild = testGroup.CreateChild();
            // testChild.SetText(0, "Test Child");
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustReleased("mouse_click"))
            {
                MouseRelease();
            }
        }

        private void MouseRelease()
        {
            if (_draggingTreeItem != null && _isMouseInMidContainer)
            {
                if (_draggingTreeItem.GetText(0) == "New Sequence")
                    PlayController.AddSequence();
            }

            _draggingTreeItem = null;
        }

        private void StartDragging(TreeItem selectedItem)
        {
            _draggingTreeItem = selectedItem;
        }

        public void OnItemMouseSelected(Vector2 _, int mouseIndex)
        {
            if (mouseIndex == 1)
            {
                StartDragging(TemplatesTree.GetSelected());
            }
        }

        public void OnMidContainerMouseEntered()
        {
            _isMouseInMidContainer = true;
        }

        public void OnMidContainerMouseExited()
        {
            _isMouseInMidContainer = false;
        }
    }
}
