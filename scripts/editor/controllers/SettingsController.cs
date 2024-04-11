using Godot;
using System;

public partial class SettingsController : Node
{
    [Export] public Control SettingsContainerNode { get; private set; }

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
