using Godot;
using System;

namespace Editor
{
    public partial class SequenceController : Node
    {
        [Export] public Control TemplatesTab { get; private set; }
        [Export] public Control SequenceTab { get; private set; }

        private Sequence _openedSequence;

        public void OpenSequence(Sequence sequence)
        {
            _openedSequence = sequence;

            OpenSequenceTab();
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
    }
}

