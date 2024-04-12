using Godot;
using Godot.Collections;
using System;
using System.ComponentModel.DataAnnotations;

namespace Editor
{
    public partial class SavingManager : Node
    {
        [Signal] public delegate void OnSaveEventHandler();

        public void OnSavingButtonClicked()
        {
            EmitSignal(SignalName.OnSave);
        }

        public void OnSaveAndExitClicked()
        {
            EmitSignal(SignalName.OnSave);
            GetTree().ChangeSceneToFile("res://scenes/pages/LevelPicker.tscn");
        }

        public void CreateNewLevel()
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
        }

        public void SaveLevelIndex(string levelID, string levelName, string levelAuthor, string songName, string songAuthor)
        {
            FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Write);
            indexFile.StoreString(
$@"{{
    ""id"": ""{levelID}"",
    ""levelName"": ""{levelName}"",
    ""levelAuthor"": ""{levelAuthor}"",
    ""songName"": ""{songName}"",
    ""songAuthor"": ""{songAuthor}"",
}}"
            );
            indexFile.Close();
        }

        public Dictionary LoadLevelIndex(string levelID)
        {
            FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Read);
            Dictionary data = (Dictionary)Json.ParseString(indexFile.GetAsText());
            return data;
        }

    }
}
