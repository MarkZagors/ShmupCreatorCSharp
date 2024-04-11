using Godot;
using Godot.Collections;
using System;

namespace Editor
{
    public partial class LevelPickController : Node
    {
        [Export] public PackedScene LevelNodeObj { get; private set; }
        [Export] public VBoxContainer LevelsVbox { get; private set; }
        [Export] public VBoxContainer DescriptionVbox { get; private set; }
        private string _selectedLevelID = null;

        public override void _Ready()
        {
            DirAccess dir = DirAccess.Open("user://");
            if (!dir.DirExists("levels"))
            {
                dir.MakeDir("levels");
            }
        }

        public void OnCreateNewButtonClick()
        {
            DirAccess levelsDirectory = DirAccess.Open("user://levels");
            Guid newLevelID = Guid.NewGuid();
            levelsDirectory.MakeDir(newLevelID.ToString());

            FileAccess indexFile = FileAccess.Open($"user://levels/{newLevelID}/index.json", FileAccess.ModeFlags.Write);
            indexFile.StoreString(
$@"{{
    ""id"": ""{newLevelID}"",
    ""levelName"": ""New Level"",
    ""levelAuthor"": ""Unknown"",
    ""songName"": ""Unknown song"",
    ""songAuthor"": ""Unknown Artist"",
}}"
            );
            indexFile.Close();

            FileAccess phasesFile = FileAccess.Open($"user://levels/{newLevelID}/phases.json", FileAccess.ModeFlags.Write);
            phasesFile.StoreString(
$@"{{
    ""phases"": [],
}}"
            );
            phasesFile.Close();

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

        private void UpdateLocalLevelsList()
        {
            DirAccess levelsDirectory = DirAccess.Open("user://levels");

            foreach (var child in LevelsVbox.GetChildren())
                child.QueueFree();

            foreach (string levelID in levelsDirectory.GetDirectories())
            {
                Control levelNode = LevelNodeObj.Instantiate<Control>();
                FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Read);
                Dictionary data = (Dictionary)Json.ParseString(indexFile.GetAsText());
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
