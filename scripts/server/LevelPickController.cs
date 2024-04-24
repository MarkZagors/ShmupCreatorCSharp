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
        public string SelectedLevelID { get; private set; } = null;

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

        public void OnLevelButtonSelected(string levelID, string levelName, string levelAuthor)
        {
            SelectedLevelID = levelID;
            DescriptionVbox.GetNode<Label>("LevelNameLabel").Text = levelName;
            DescriptionVbox.GetNode<Label>("LevelAuthorLabel").Text = levelAuthor;
        }

        public void OnPlayButtonClick()
        {
            if (SelectedLevelID == null) return;
            TransferLayer.LevelID = SelectedLevelID;
            GetTree().ChangeSceneToPacked(PlayScene);
        }

        public void OnEditButtonClick()
        {
            if (SelectedLevelID == null) return;
            TransferLayer.LevelID = SelectedLevelID;
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

                levelNode.GetNode<Label>("LevelLabel").Text = $"{levelName} - {levelAuthor}";

                levelNode.GetNode<Button>("Button").Pressed += () => OnLevelButtonSelected(
                    levelID,
                    levelName,
                    levelAuthor
                );

                LevelsVbox.AddChild(levelNode);
            }
        }
    }
}
