using System;

namespace Editor
{
    public class ProjectData
    {
        public Guid ID { get; private set; }
        public string LevelName { get; private set; }
        public string LevelAuthor { get; private set; }
        public string SongName { get; private set; }
        public string SongAuthor { get; private set; }

        public ProjectData(Guid id, string levelName, string levelAuthor, string songName, string songAuthor)
        {
            ID = id;
            LevelName = levelName;
            LevelAuthor = levelAuthor;
            SongName = songName;
            SongAuthor = songAuthor;
        }
    }
}
