using Godot;
using Godot.Collections;
using System;

namespace Editor
{
    public partial class LevelPickController : Node
    {
        [Export] public PackedScene LevelNodeObj { get; private set; }
        [Export] public PackedScene EditorScene { get; private set; }
        [Export] public PackedScene PlayScene { get; private set; }
        [Export] public VBoxContainer LevelsVbox { get; private set; }
        [Export] public VBoxContainer DescriptionVbox { get; private set; }
        [Export] public SavingManager SavingManager { get; private set; }
        private string _selectedLevelID = null;

        public override void _Ready()
        {
            DirAccess dir = DirAccess.Open("user://");
            if (!dir.DirExists("levels"))
            {
                dir.MakeDir("levels");
            }

            UpdateLocalLevelsList();
        }

        public void OnCreateNewButtonClick()
        {
            SavingManager.CreateNewLevel();
            UpdateLocalLevelsList();
        }

        public void OnLevelButtonSelected(string levelID, string levelName, string levelAuthor, string songName, string songAuthor)
        {
            _selectedLevelID = levelID;
            DescriptionVbox.GetNode<Label>("LevelNameLabel").Text = levelName;
            DescriptionVbox.GetNode<Label>("LevelAuthorLabel").Text = levelAuthor;
            DescriptionVbox.GetNode<Label>("SongNameLabel").Text = songName;
            DescriptionVbox.GetNode<Label>("SongAuthorLabel").Text = songAuthor;
        }

        public void OnPlayButtonClick()
        {
            if (_selectedLevelID == null) return;
            TransferLayer.LevelID = _selectedLevelID;
            GetTree().ChangeSceneToPacked(PlayScene);
        }

        public void OnEditButtonClick()
        {
            if (_selectedLevelID == null) return;
            TransferLayer.LevelID = _selectedLevelID;
            GetTree().ChangeSceneToPacked(EditorScene);
        }

        private void UpdateLocalLevelsList()
        {
            DirAccess levelsDirectory = DirAccess.Open("user://levels");

            foreach (var child in LevelsVbox.GetChildren())
                child.QueueFree();

            foreach (string levelID in levelsDirectory.GetDirectories())
            {
                Control levelNode = LevelNodeObj.Instantiate<Control>();
                Dictionary data = SavingManager.LoadLevelIndex(levelID);
                string levelName = (string)data["levelName"];
                string levelAuthor = (string)data["levelAuthor"];
                string songName = (string)data["songName"];
                string songAuthor = (string)data["songAuthor"];

                levelNode.GetNode<Label>("LevelLabel").Text = $"{levelName} - {levelAuthor}";
                levelNode.GetNode<Label>("SongLabel").Text = $"Song: {songName} - {songAuthor}";

                levelNode.GetNode<Button>("Button").Pressed += () => OnLevelButtonSelected(
                    levelID,
                    levelName,
                    levelAuthor,
                    songName,
                    songAuthor
                );

                LevelsVbox.AddChild(levelNode);
            }
        }
    }
}
