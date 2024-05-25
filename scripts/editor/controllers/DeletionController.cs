using Editor;
using Godot;
using System;
using System.ComponentModel.Design;

public partial class DeletionController : Node
{
    [Export] public SequenceController SequenceController { get; private set; }
    [Export] public ComponentsController ComponentsController { get; private set; }
    [Export] public PlayController PlayController { get; private set; }
    private Sequence openedSequence = null;
    private SelectType selectType = SelectType.NULL;

    private enum SelectType
    {
        NULL,
        SEQUENCE,
        COMPONENT
    }

    public override void _Ready()
    {
        SequenceController.SelectSequence += OnSequenceSelect;
        ComponentsController.ComponentSelect += OnComponentSelect;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("delete"))
        {
            if (selectType == SelectType.SEQUENCE)
            {
                PlayController.DeleteSequence(openedSequence);
            }
            if (selectType == SelectType.COMPONENT)
            {

            }
        }
    }

    private void OnSequenceSelect()
    {
        selectType = SelectType.SEQUENCE;
        openedSequence = SequenceController.GetOpenedSequence();
    }

    private void OnComponentSelect()
    {
        selectType = SelectType.COMPONENT;
    }
}
