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

        private Sequence _openedSequence;

        public override void _Ready()
        {
            SequenceTab.GetNode<Label>("NoSelectionLabel").Visible = true;
            SequenceTab.GetNode<Control>("TopSequenceBar").Visible = false;
            SequenceTree.Visible = false;
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
        }

        public void OnClickCloseNewComponent()
        {
            CloseNewComponent();
        }

        public void OnClickCreateBullet()
        {
            CreateBullet();
        }

        public void OnClickCreateBundle()
        {

        }

        public void OnClickCreateSpawner()
        {

        }
    }
}

