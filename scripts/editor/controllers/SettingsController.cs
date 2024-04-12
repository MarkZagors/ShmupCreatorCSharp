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
        SavingManager.OnSave += SaveLevel;
        LoadLevel();
    }

    private void LoadLevel()
    {
        Dictionary data = SavingManager.LoadLevelIndex(TransferLayer.LevelID);

        string levelName = (string)data["levelName"];
        string levelAuthor = (string)data["levelAuthor"];
        string songName = (string)data["songName"];
        string songAuthor = (string)data["songAuthor"];

        SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelNameField/TextEdit").Text = levelName;
        SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelAuthorField/TextEdit").Text = levelAuthor;
        SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/SongNameField/TextEdit").Text = songName;
        SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/SongAuthorField/TextEdit").Text = songAuthor;
        SettingsContainerNode.GetNode<Label>("LevelIDLabel").Text = $"Level ID: {TransferLayer.LevelID}";
    }

    private void SaveLevel()
    {
        SavingManager.SaveLevelIndex(
            levelID: TransferLayer.LevelID,
            levelName: SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelNameField/TextEdit").Text,
            levelAuthor: SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelAuthorField/TextEdit").Text,
            songName: SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/SongNameField/TextEdit").Text,
            songAuthor: SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/SongAuthorField/TextEdit").Text
        );
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
