using Godot;
using Godot.Collections;
using System;

namespace Editor
{
    public partial class SavingManager : Node
    {
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

        public Dictionary GetLevelIndex(string levelID)
        {
            FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Read);
            Dictionary data = (Dictionary)Json.ParseString(indexFile.GetAsText());
            return data;
        }

    }
}
