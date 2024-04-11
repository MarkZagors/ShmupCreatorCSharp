using Editor;
using Godot;
using Godot.Collections;
using System;

public partial class SettingsController : Node
{
    [Export] public Control SettingsContainerNode { get; private set; }
    [Export] public SavingManager SavingManager { get; private set; }

    public override void _Ready()
    {
        LoadLevel();
    }

    private void LoadLevel()
    {
        Dictionary data = SavingManager.GetLevelIndex(TransferLayer.LevelID);
        GD.Print(data);
    }

    public void OnSettingsButtonClicked()
    {
        if (SettingsContainerNode.Visible == false)
        {
            SettingsContainerNode.Visible = true;
        }
        else
        {
            SettingsContainerNode.Visible = false;
        }
    }
}
