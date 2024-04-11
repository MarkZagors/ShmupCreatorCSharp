using Godot;
using System;

namespace Editor
{
    public partial class LevelPickController : Node
    {
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

            FileAccess levelFile = FileAccess.Open($"user://levels/{newLevelID}/level.json", FileAccess.ModeFlags.Write);
            levelFile.StoreString(
$@"{{
    ""id"": ""{newLevelID}"",
    ""levelName"": ""New Level"",
    ""levelAuthor"": ""Unknown"",
    ""songName"": ""Unknown song"",
    ""songAuthor"": ""Unknown Artist"",
    ""phases"": [],
}}"
            );
            levelFile.Close();
        }
    }
}
