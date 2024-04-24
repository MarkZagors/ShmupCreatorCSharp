using Editor;
using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Linq;

public partial class SettingsController : Node
{
    [Export] public Control SettingsContainerNode { get; private set; }
    [Export] public SavingManager SavingManager { get; private set; }

    public override void _Ready()
    {
        SavingManager.OnSave += SaveLevel;
        LoadLevel();

        GetTree().Root.FilesDropped += OnFileDropped;
    }

    private void LoadLevel()
    {
        Dictionary data = SavingManager.LoadLevelIndex(TransferLayer.LevelID);

        string levelName = (string)data["levelName"];
        string levelAuthor = (string)data["levelAuthor"];

        SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelNameField/TextEdit").Text = levelName;
        SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelAuthorField/TextEdit").Text = levelAuthor;
        SettingsContainerNode.GetNode<Label>("LevelIDLabel").Text = $"Level ID: {TransferLayer.LevelID}";
    }

    private void SaveLevel()
    {
        SavingManager.SaveLevelIndex(
            levelID: TransferLayer.LevelID,
            levelName: SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelNameField/TextEdit").Text,
            levelAuthor: SettingsContainerNode.GetNode<TextEdit>("FieldsVBox/LevelAuthorField/TextEdit").Text
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

    public void OnFileDropped(String[] files)
    {
        if (files.Length == 1 && files[0][^4..] == ".mp3")
        {
            string musicFilePath = files[0];
            string absoluteMusicPath = OS.GetUserDataDir();
            absoluteMusicPath += $"/levels/{TransferLayer.LevelID}/music.mp3";
            System.IO.File.Copy(musicFilePath, absoluteMusicPath, overwrite: true);

            // _isMusicAdded = true;
            SaveLevel();
        }
    }
}
